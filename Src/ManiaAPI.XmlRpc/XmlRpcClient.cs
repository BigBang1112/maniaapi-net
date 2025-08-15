using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MinimalXmlReader;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace ManiaAPI.XmlRpc;

public class XmlRpcClient
{
    private const string Handshake = "GBXRemote 2";

    private TcpClient? tcp;

    private uint handle = 0x80000000;

#if NET9_0_OR_GREATER
    private readonly Lock handleLock = new();
#else
    private readonly object handleLock = new();
#endif

    private readonly ConcurrentDictionary<uint, string?> pendingRequests = new();
    private readonly ConcurrentQueue<string> callbacks = new();

    private readonly SemaphoreSlim callbackSemaphore = new(initialCount: 0);
    private readonly SemaphoreSlim methodSemaphore = new(initialCount: 0);

    private BinaryReader? reader;
    private BinaryWriter? writer;

    public event RemoteCallback? Callback;

    public bool WaitingForMessages { get; private set; }

    private readonly ILogger logger;

    public XmlRpcClient(ILogger? logger = null)
    {
        this.logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="SocketException"></exception>
    public async ValueTask<bool> ConnectAsync(string ip, int port = 5000, CancellationToken cancellationToken = default)
    {
        tcp = new();

        await tcp.ConnectAsync(ip, port, cancellationToken);

        return ValidateHeader(tcp.GetStream());
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

    public async Task<object?[]> CallAsync(string methodName, object?[] methodParams, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogDebug("Calling {MethodName}...", methodName);

        var startTime = Stopwatch.GetTimestamp();

        var xmlPayload = GenerateXmlPayload(methodName, methodParams);

        var elapsed = Stopwatch.GetElapsedTime(startTime);

        logger.LogTrace("Generated XML for {MethodName} (in {ElapsedMilliseconds}ms): {XmlPayload}", methodName, elapsed.TotalMilliseconds, xmlPayload);

        return ReadXmlRpcMethodResponse(await SendAndReceiveAsync(methodName, xmlPayload, cancellationToken));
    }

    private async Task<string> SendAndReceiveAsync(string methodName, string xmlPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var startTime = Stopwatch.GetTimestamp();

        var handle = SendXmlPayload(xmlPayload);

        pendingRequests[handle] = null;

        logger.LogDebug("{MethodName} (0x{Handle}) has been sent. Waiting for response...", methodName, handle.ToString("x8"));

        string? xml;

        while (!pendingRequests.TryGetValue(handle, out xml) || xml is null)
        {
            await methodSemaphore.WaitAsync(cancellationToken);
        }

        var elapsed = Stopwatch.GetElapsedTime(startTime);

        logger.LogDebug("{MethodName} (0x{Handle}) response received (in {ElapsedMilliseconds}ms).", methodName, handle.ToString("x8"), elapsed.TotalMilliseconds);

        pendingRequests.Remove(handle, out _);

        return xml;
    }

    public async Task<object?[]> CallAsync(string methodName, CancellationToken cancellationToken = default)
    {
        return await CallAsync(methodName, [], cancellationToken);
    }

    public async Task<string> CallXmlAsync(string methodName, CancellationToken cancellationToken = default)
    {
        return await CallXmlAsync(methodName, [], cancellationToken);
    }

    public Task ListenAsync(CancellationToken cancellationToken)
    {
        var r = GetReader();

        while (!cancellationToken.IsCancellationRequested)
        {
            WaitingForMessages = true;

            var handle = ReceiveXmlPayload(r, out string payload);

            WaitingForMessages = false;

            // if handle is sent method (not callback)
            var isCallback = (handle >> 31) == 0;

            if (isCallback)
            {
                logger.LogTrace("Received XML callback (0x{Handle}): {Payload}", handle.ToString("x8"), payload);
            }
            else if (!pendingRequests.ContainsKey(handle))
            {
                logger.LogWarning("Unknown handle, skipping...");
                continue;
            }
            else
            {
                logger.LogTrace("Received XML response (0x{Handle}): {Payload}", handle, payload);
            }

            if (isCallback)
            {
                // Callback thread should wait when there are no callbacks, and it is already released when >1 callbacks in queue
                // As it only enqueues here and not anywhere else, if it will enqueue elsewhere, it needs to be addressed!!
                callbacks.Enqueue(payload);

                if (callbacks.Count == 1)
                {
                    callbackSemaphore.Release();
                }
            }
            else
            {
                pendingRequests[handle] = payload;

                // Wait, release, wait, release, ... on each received response, so it should always release here
                methodSemaphore.Release();
            }
        }

        return Task.CompletedTask;
    }

    public async Task ProcessCallbacksAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await callbackSemaphore.WaitAsync(cancellationToken);

            while (callbacks.TryDequeue(out var r))
            {
                ProcessCallback(r);
            }
        }
    }

    private void ProcessCallback(string xml)
    {
        var r = new MiniXmlReader(xml);

        Debug.Assert(r.SkipProcessingInstruction());
        Debug.Assert(r.SkipStartElement("methodCall"));
        Debug.Assert(r.SkipStartElement("methodName"));

        var methodName = r.ReadContentAsString();

        Debug.Assert(r.SkipEndElement());

        var parameters = ReadXmlRpcParams(xml, ref r);

        Callback?.Invoke(methodName, parameters);
    }

    private static object?[] ReadXmlRpcMethodResponse(string xml)
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
            "string" => r.ReadContentAsString(),
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

    private uint ReceiveXmlPayload(BinaryReader r, out string payload)
    {
        var payloadSize = r.ReadInt32();
        var handle = r.ReadUInt32();

        if (payloadSize < short.MaxValue)
        {
            Span<byte> buffer = stackalloc byte[payloadSize];

            if (r.Read(buffer) != payloadSize)
            {
                throw new XmlRpcClientException("Invalid payload size");
            }

            payload = Encoding.UTF8.GetString(buffer);
        }
        else
        {
            var buffer = new byte[payloadSize];

            if (r.Read(buffer, 0, payloadSize) != payloadSize)
            {
                throw new XmlRpcClientException("Invalid payload size");
            }

            payload = Encoding.UTF8.GetString(buffer);
        }

        return handle;
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

    // TODO: should be refactored to use Slice
    private uint SendXmlPayload(string xmlPayload)
    {
        const int headerSize = sizeof(uint) + sizeof(uint);

        var xmlPayloadByteCount = Encoding.UTF8.GetByteCount(xmlPayload);

        // uint32 xmlPayloadByteCount (4 bytes)
        // uint32 handle (+4 bytes = 8)
        // bytes xmlPayload (length of xmlPayloadByteCount)
        Span<byte> buffer = stackalloc byte[xmlPayloadByteCount + headerSize];

        if (Encoding.UTF8.GetBytes(xmlPayload, buffer) != xmlPayloadByteCount)
        {
            throw new XmlRpcClientException("Invalid string buffering");
        }

        for (var i = 0; i < xmlPayloadByteCount; i++)
        {
            buffer[xmlPayloadByteCount + headerSize - 1 - i] = buffer[xmlPayloadByteCount - 1 - i];
        }

        BitConverter.TryWriteBytes(buffer, xmlPayloadByteCount);

        var handle = GetNextHandle();

        // Refers to BitConverter.GetBytes implementation
        Unsafe.As<byte, uint>(ref buffer[4]) = handle;

        GetWriter().Write(buffer);

        return handle;
    }

    public void Dispose()
    {
        tcp?.Close();
        tcp = null;
    }

    private bool ValidateHeader(NetworkStream stream)
    {
        Span<byte> lengthBuf = stackalloc byte[4];

        if (stream.Read(lengthBuf) != 4 || lengthBuf[1] != 0 || lengthBuf[2] != 0 || lengthBuf[3] != 0)
        {
            return false;
        }

        var length = lengthBuf[0];

        Span<byte> headerBuf = stackalloc byte[length];

        if (stream.Read(headerBuf) != length)
        {
            throw new XmlRpcClientException("Invalid header length");
        }

        for (int i = 0; i < Handshake.Length; i++)
        {
            if (headerBuf[i] != Handshake[i])
            {
                return false;
            }
        }

        return true;
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

    private BinaryReader GetReader()
    {
        _ = tcp ?? throw new XmlRpcClientException("TCP connection not available");
        return reader ??= new BinaryReader(tcp.GetStream());
    }

    private BinaryWriter GetWriter()
    {
        _ = tcp ?? throw new XmlRpcClientException("TCP connection not available");
        return writer ??= new BinaryWriter(tcp.GetStream());
    }
}