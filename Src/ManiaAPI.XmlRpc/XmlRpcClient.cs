using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MinimalXmlReader;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace ManiaAPI.XmlRpc;

public partial class XmlRpcClient : IDisposable
{
    private const string Handshake = "GBXRemote 2";

    private uint handle = 0x80000000;

#if NET9_0_OR_GREATER
    private readonly Lock handleLock = new();
#else
    private readonly object handleLock = new();
#endif
    private readonly Channel<KeyValuePair<uint, string>> callbackChannel = Channel.CreateUnbounded<KeyValuePair<uint, string>>();
    private readonly ConcurrentDictionary<uint, Channel<string>> pendingRequests = new();

    private readonly TcpClient tcp;
    private readonly ILogger<XmlRpcClient> logger;

    private readonly NetworkStream stream;

    private readonly CancellationTokenSource cts = new();

    public bool IsWaitingForMessages { get; private set; }

    public Task ListenTask { get; }
    public Task CallbackTask { get; }

    public event XmlRpcCallback? Callback;

    private XmlRpcClient(TcpClient tcp, ILogger<XmlRpcClient> logger)
    {
        this.tcp = tcp ?? throw new ArgumentNullException(nameof(tcp));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        stream = tcp.GetStream();

        ListenTask = Task.Run(() => ListenAsync(cts.Token));
        CallbackTask = Task.Run(() => RunCallbacksAsync(cts.Token));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="SocketException"></exception>
    /// <exception cref="XmlRpcClientException">Header is invalid.</exception>
    public static async ValueTask<XmlRpcClient> ConnectAsync(
        string ip,
        int port = 5000,
        ILogger<XmlRpcClient>? logger = null,
        CancellationToken cancellationToken = default)
    {
        var tcp = new TcpClient();
        await tcp.ConnectAsync(ip, port, cancellationToken);
        await ValidateHeaderOrThrowAsync(tcp.GetStream(), cancellationToken);
        return new XmlRpcClient(tcp, logger ?? NullLogger<XmlRpcClient>.Instance);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="SocketException"></exception>
    /// <exception cref="XmlRpcClientException">Header is invalid.</exception>
    public static async ValueTask<XmlRpcClient> ConnectAsync(
        IPAddress ip,
        int port = 5000,
        ILogger<XmlRpcClient>? logger = null,
        CancellationToken cancellationToken = default)
    {
        var tcp = new TcpClient();
        await tcp.ConnectAsync(ip, port, cancellationToken);
        await ValidateHeaderOrThrowAsync(tcp.GetStream(), cancellationToken);
        return new XmlRpcClient(tcp, logger ?? NullLogger<XmlRpcClient>.Instance);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="SocketException"></exception>
    /// <exception cref="XmlRpcClientException">Header is invalid.</exception>
    public static async ValueTask<XmlRpcClient> ConnectAsync(
        IPEndPoint endpoint,
        ILogger<XmlRpcClient>? logger = null,
        CancellationToken cancellationToken = default)
    {
        var tcp = new TcpClient();
        await tcp.ConnectAsync(endpoint, cancellationToken);
        await ValidateHeaderOrThrowAsync(tcp.GetStream(), cancellationToken);
        return new XmlRpcClient(tcp, logger ?? NullLogger<XmlRpcClient>.Instance);
    }

    private static async Task ValidateHeaderOrThrowAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var lengthBuffer = new byte[4];
        await stream.ReadExactlyAsync(lengthBuffer, cancellationToken);
        
        if (lengthBuffer[1] != 0 || lengthBuffer[2] != 0 || lengthBuffer[3] != 0)
        {
            throw new XmlRpcClientException("GBXRemote header has invalid length");
        }

        var length = lengthBuffer[0];
        var headerBuffer = new byte[length];
        await stream.ReadExactlyAsync(headerBuffer, cancellationToken);

        for (int i = 0; i < Handshake.Length; i++)
        {
            if (headerBuffer[i] != Handshake[i])
            {
                throw new XmlRpcClientException("GBXRemote header is invalid");
            }
        }
    }

    private async Task ListenAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            IsWaitingForMessages = true;

            var payloadPrefix = new byte[8];
            await stream.ReadExactlyAsync(payloadPrefix, cancellationToken);

            var payloadSize = BitConverter.ToInt32(payloadPrefix, 0);
            var handle = BitConverter.ToUInt32(payloadPrefix, 4);
            var payloadBuffer = new byte[payloadSize];
            await stream.ReadExactlyAsync(payloadBuffer, cancellationToken);

            IsWaitingForMessages = false;

            var payload = Encoding.UTF8.GetString(payloadBuffer);

            // if handle is sent method (not callback)
            var isCallback = (handle >> 31) == 0;

            if (isCallback)
            {
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("Received XML callback (0x{Handle}): {Payload}", handle.ToString("x8"), payload);
                }

                await callbackChannel.Writer.WriteAsync(new(handle, payload), cancellationToken);
            }
            else
            {
                if (!pendingRequests.ContainsKey(handle))
                {
                    logger.LogWarning("Unknown handle (0x{Handle}), skipping...", handle.ToString("x8"));
                    continue;
                }

                logger.LogTrace("Received XML response (0x{Handle}): {Payload}", handle.ToString("x8"), payload);

                var channel = GetOrCreatePendingRequestChannel(handle);
                await channel.Writer.WriteAsync(payload, cancellationToken);
            }
        }
    }

    private async Task RunCallbacksAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var (handle, xml) = await callbackChannel.Reader.ReadAsync(cancellationToken);

            var r = new MiniXmlReader(xml);

            Debug.Assert(r.SkipProcessingInstruction());
            Debug.Assert(r.SkipStartElement("methodCall"));
            Debug.Assert(r.SkipStartElement("methodName"));

            var methodName = r.ReadContentAsString();

            Debug.Assert(r.SkipEndElement());

            var parameters = ReadXmlRpcParams(xml, ref r);

            if (Callback is not null)
            {
                await Callback.Invoke(methodName, parameters);
            }
        }
    }

    public async Task<string> CallXmlAsync(string methodName, object?[] methodParams, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogDebug("Calling {MethodName}...", methodName);

        var startTime = Stopwatch.GetTimestamp();
        var xmlPayload = GenerateXmlPayload(methodName, methodParams);
        var elapsed = Stopwatch.GetElapsedTime(startTime);

        logger.LogTrace("Generated XML for {MethodName} (in {ElapsedMilliseconds}ms): {XmlPayload}", methodName, elapsed.TotalMilliseconds, xmlPayload);

        return await SendAndReceiveAsync(methodName, xmlPayload, cancellationToken);
    }

    public async Task<string> CallXmlAsync(string methodName, CancellationToken cancellationToken = default)
    {
        return await CallXmlAsync(methodName, [], cancellationToken);
    }

    public async Task<object?[]> CallAsync(string methodName, object?[] methodParams, CancellationToken cancellationToken = default)
    {
        var xmlResult = await CallXmlAsync(methodName, methodParams, cancellationToken);

        return ParseXmlRpcMethodResponse(xmlResult);
    }

    public async Task<object?[]> CallAsync(string methodName, CancellationToken cancellationToken = default)
    {
        return await CallAsync(methodName, [], cancellationToken);
    }

    private async Task<string> SendAndReceiveAsync(string methodName, string xmlPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var startTime = Stopwatch.GetTimestamp();

        var handle = await SendXmlPayloadAsync(xmlPayload, cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("{MethodName} (0x{Handle}) has been sent. Waiting for response...", methodName, handle.ToString("x8"));
        }

        var channel = GetOrCreatePendingRequestChannel(handle);
        var xml = await channel.Reader.ReadAsync(cancellationToken);

        pendingRequests.Remove(handle, out _);

        var elapsed = Stopwatch.GetElapsedTime(startTime);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("{MethodName} (0x{Handle}) response received (in {ElapsedMilliseconds}ms).", methodName, handle.ToString("x8"), elapsed.TotalMilliseconds);
        }

        return xml;
    }

    private Channel<string> GetOrCreatePendingRequestChannel(uint handle)
    {
        return pendingRequests.GetOrAdd(handle, _ => Channel.CreateBounded<string>(1));
    }

    private static object?[] ParseXmlRpcMethodResponse(string xml)
    {
        var r = new MiniXmlReader(xml);

        Debug.Assert(r.SkipProcessingInstruction());
        Debug.Assert(r.SkipStartElement("methodResponse"));

        return ReadXmlRpcParams(xml, ref r);
    }

    private static object?[] ReadXmlRpcParams(string xml, ref MiniXmlReader r)
    {
        if (!r.SkipStartElement("params"))
        {
            if (!r.SkipStartElement("fault"))
            {
                throw new XmlRpcClientException(xml);
            }

            if (ReadXmlRpcValue(ref r) is not Dictionary<string, object?> faultDict)
            {
                throw new XmlRpcClientException("Fault is not dictionary, cannot gather details");
            }

            if (faultDict.TryGetValue("faultString", out var faultString))
            {
                throw new XmlRpcFaultException(faultString?.ToString());
            }

            throw new XmlRpcClientException("Cannot gather fault details (faultString not found)");
        }

        var parameters = new List<object?>();

        while (r.SkipStartElement("param"))
        {
            parameters.Add(ReadXmlRpcValue(ref r));
            Debug.Assert(r.SkipEndElement("param"));
        }

        return parameters.ToArray();
    }

    private static object? ReadXmlRpcValue(ref MiniXmlReader r)
    {
        Debug.Assert(r.SkipStartElement("value"));

        var type = r.ReadStartElement();

        object? value = type switch
        {
            "i4" or "int" => int.Parse(r.ReadContent()),
            "string" => WebUtility.HtmlDecode(r.ReadContentAsString()),
            "boolean" => r.ReadContentAsBoolean(),
            "double" => double.Parse(r.ReadContent(), NumberStyles.Number, CultureInfo.InvariantCulture),
            "struct" => ReadXmlRpcStruct(ref r),
            "array" => ReadXmlRpcArray(ref r),
            _ => throw new XmlRpcClientException($"unknown type {type}"),
        };

        Debug.Assert(r.SkipEndElement());
        Debug.Assert(r.SkipEndElement("value"));

        return value;
    }

    private static Dictionary<string, object?> ReadXmlRpcStruct(ref MiniXmlReader r)
    {
        var dict = new Dictionary<string, object?>();

        while (r.SkipStartElement("member"))
        {
            Debug.Assert(r.SkipStartElement("name"));
            var memberName = r.ReadContentAsString();
            Debug.Assert(r.SkipEndElement("name"));
            var memberValue = ReadXmlRpcValue(ref r);

            dict.Add(memberName, memberValue);

            Debug.Assert(r.SkipEndElement("member"));
        }

        return dict;
    }

    private static List<object?> ReadXmlRpcArray(ref MiniXmlReader r)
    {
        var list = new List<object?>();

        Debug.Assert(r.SkipStartElement("data"));

        while (!r.SkipEndElement("data"))
        {
            list.Add(ReadXmlRpcValue(ref r));
        }

        return list;
    }

    private static string GenerateXmlPayload(string methodName, object?[] methodParams)
    {
        var sb = new StringBuilder("<?xml version=\"1.0\"?><methodCall><methodName>");
        sb.Append(methodName);
        sb.Append("</methodName><params>");

        foreach (var param in methodParams)
        {
            AppendXmlRpcParam(sb, param);
        }

        sb.Append("</params></methodCall>");

        return sb.ToString();
    }

    private static void AppendXmlRpcParam<T>(StringBuilder sb, T param)
    {
        sb.Append("<param><value>");

        switch (param)
        {
            case int integer:
                sb.Append("<int>");
                sb.Append(integer);
                sb.Append("</int>");
                break;
            case double doub:
                sb.Append("<double>");
                sb.Append(doub);
                sb.Append("</double>");
                break;
            case bool boolean:
                sb.Append("<boolean>");
                sb.Append(boolean ? '1' : '0');
                sb.Append("</boolean>");
                break;
            case byte[] byteArr:
                sb.Append("<base64>");
                sb.Append(Convert.ToBase64String(byteArr));
                sb.Append("</base64>");
                break;
            case string str:
                sb.Append(str);
                break;
            default:
                sb.Append(param);
                break;
        }

        sb.Append("</value></param>");
    }

    private async Task<uint> SendXmlPayloadAsync(string xmlPayload, CancellationToken cancellationToken)
    {
        const int headerSize = sizeof(uint) + sizeof(uint);

        var xmlPayloadByteCount = Encoding.UTF8.GetByteCount(xmlPayload);

        // uint32 xmlPayloadByteCount (4 bytes)
        // uint32 handle (+4 bytes = 8)
        // bytes xmlPayload (length of xmlPayloadByteCount)
        var buffer = new byte[xmlPayloadByteCount + headerSize];

        var bufferedXmlPayloadByteCount = Encoding.UTF8.GetBytes(xmlPayload, buffer.AsSpan().Slice(headerSize));

        if (bufferedXmlPayloadByteCount != xmlPayloadByteCount)
        {
            throw new XmlRpcClientException($"Invalid string buffering (expected {xmlPayloadByteCount} bytes, got {bufferedXmlPayloadByteCount} bytes)");
        }

        if (!BitConverter.TryWriteBytes(buffer, xmlPayloadByteCount))
        {
            throw new XmlRpcClientException("Failed to write XML payload byte count to buffer");
        }

        var handle = GetNextHandle();

        // Write handle as uint32 at offset 4
        if (!BitConverter.TryWriteBytes(buffer.AsSpan().Slice(4), handle))
        {
            throw new XmlRpcClientException("Failed to write handle to buffer");
        }

        await stream.WriteAsync(buffer, cancellationToken);

        return handle;
    }

    private uint GetNextHandle()
    {
        lock (handleLock)
        {
            if (handle + 1 == 0xFFFFFFFF)
            {
                handle = 0x80000000;
            }

            return handle++;
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        tcp.Dispose();
        GC.SuppressFinalize(this);
    }
}
