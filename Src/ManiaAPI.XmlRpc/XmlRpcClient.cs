using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MinimalXmlReader;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Channels;

namespace ManiaAPI.XmlRpc;

public partial class XmlRpcClient : IDisposable, IAsyncDisposable
{
    private const string HandshakeV1 = "GBXRemote 1";
    private const string HandshakeV2 = "GBXRemote 2";

    // GBXRemote 1 messages carry no handle, so a fixed placeholder is used to key pending requests
    private const uint NoHandle = 0;

    private uint handle = 0x80000000;

#if NET9_0_OR_GREATER
    private readonly Lock handleLock = new();
#else
    private readonly object handleLock = new();
#endif
    private readonly Channel<KeyValuePair<uint, string>> callbackChannel = Channel.CreateUnbounded<KeyValuePair<uint, string>>();
    private readonly ConcurrentDictionary<uint, Channel<string>> pendingRequests = new();
    private readonly ConcurrentDictionary<string, ImmutableList<Func<object[], CancellationToken, Task>>> routeHandlers = new();

    // GBXRemote 1 has no handle to correlate requests/responses,
    // so calls must be strictly sequential to avoid cross-talk between callers
    private readonly SemaphoreSlim? v1CallSemaphore;

    private readonly TcpClient tcp;
    private readonly int version;
    private readonly ILogger<XmlRpcClient> logger;

    private readonly NetworkStream stream;

    private readonly CancellationTokenSource cts = new();

    public bool IsWaitingForMessages { get; private set; }

    private Task ListenTask { get; }
    private Task CallbackTask { get; }

    public int Version => version;

    public event XmlRpcCallback? Callback;

    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Received XML response (0x{Handle:x8}): {Payload}")]
    private static partial void LogReceivedXmlResponse(ILogger logger, uint handle, string payload);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Received XML callback (0x{Handle:x8}): {Payload}")]
    private static partial void LogReceivedXmlCallback(ILogger logger, uint handle, string payload);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Unknown handle (0x{Handle:x8}), skipping...")]
    private static partial void LogUnknownHandle(ILogger logger, uint handle);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Calling {MethodName}...")]
    private static partial void LogCallingMethod(ILogger logger, string methodName);

    [LoggerMessage(EventId = 5, Level = LogLevel.Trace, Message = "Generated XML for {MethodName} (in {ElapsedMilliseconds}ms): {XmlPayload}")]
    private static partial void LogGeneratedXml(ILogger logger, string methodName, double elapsedMilliseconds, string xmlPayload);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "{MethodName} (0x{Handle:x8}) has been sent. Waiting for response...")]
    private static partial void LogMethodSent(ILogger logger, string methodName, uint handle);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "{MethodName} (0x{Handle:x8}) response received (in {ElapsedMilliseconds}ms).")]
    private static partial void LogMethodResponseReceived(ILogger logger, string methodName, uint handle, double elapsedMilliseconds);

    private XmlRpcClient(TcpClient tcp, int version, ILogger<XmlRpcClient> logger)
    {
        this.tcp = tcp ?? throw new ArgumentNullException(nameof(tcp));
        this.version = version;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        v1CallSemaphore = version < 2 ? new SemaphoreSlim(1, 1) : null;

        stream = tcp.GetStream();

        ListenTask = Task.Run(async () =>
        {
            try
            {
                await ListenAsync(cts.Token);
            }
            finally
            {
                callbackChannel.Writer.TryComplete();

                if (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                }

                // Unblock any calls still awaiting a response so they fail fast instead of
                // hanging forever once the connection is lost/closed (see SendAndReceiveAsync).
                foreach (var pendingChannel in pendingRequests.Values)
                {
                    pendingChannel.Writer.TryComplete();
                }
            }
        });

        CallbackTask = Task.Run(() => ProcessCallbacksAsync(cts.Token));
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
        var version = await ValidateHeaderOrThrowAsync(tcp.GetStream(), cancellationToken);
        return new XmlRpcClient(tcp, version, logger ?? NullLogger<XmlRpcClient>.Instance);
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
        var version = await ValidateHeaderOrThrowAsync(tcp.GetStream(), cancellationToken);
        return new XmlRpcClient(tcp, version, logger ?? NullLogger<XmlRpcClient>.Instance);
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
        var version = await ValidateHeaderOrThrowAsync(tcp.GetStream(), cancellationToken);
        return new XmlRpcClient(tcp, version, logger ?? NullLogger<XmlRpcClient>.Instance);
    }

    private static async Task<int> ValidateHeaderOrThrowAsync(NetworkStream stream, CancellationToken cancellationToken)
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

        var header = Encoding.ASCII.GetString(headerBuffer);

        return header switch
        {
            HandshakeV1 => 1,
            HandshakeV2 => 2,
            _ => throw new XmlRpcClientException($"GBXRemote header is invalid: {header}"),
        };
    }

    public void On(string methodName, Func<object[], CancellationToken, Task> handler)
    {
        routeHandlers.AddOrUpdate(
            methodName,
            _ => [handler],
            (_, list) => list.Add(handler)
        );
    }

    private async Task ListenAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            IsWaitingForMessages = true;

            uint handle;
            int payloadSize;

            if (version >= 2)
            {
                var payloadPrefix = new byte[8];
                await stream.ReadExactlyAsync(payloadPrefix, cancellationToken);

                payloadSize = BitConverter.ToInt32(payloadPrefix, 0);
                handle = BitConverter.ToUInt32(payloadPrefix, 4);
            }
            else
            {
                // GBXRemote 1 has no handle in the message header
                var payloadPrefix = new byte[4];
                await stream.ReadExactlyAsync(payloadPrefix, cancellationToken);

                payloadSize = BitConverter.ToInt32(payloadPrefix, 0);
                handle = NoHandle;
            }

            var payloadBuffer = new byte[payloadSize];
            await stream.ReadExactlyAsync(payloadBuffer, cancellationToken);

            IsWaitingForMessages = false;

            var payload = Encoding.UTF8.GetString(payloadBuffer);

            // if handle is sent method (not callback)
            // GBXRemote 1 has no handle, so distinguish by the actual XML root element instead.
            var isCallback = version >= 2
                ? (handle >> 31) == 0
                : IsMethodCallPayload(payload);

            if (isCallback)
            {
                LogReceivedXmlCallback(logger, handle, payload);

                await callbackChannel.Writer.WriteAsync(new(handle, payload), cancellationToken);
            }
            else
            {
                if (!pendingRequests.ContainsKey(handle))
                {
                    LogUnknownHandle(logger, handle);
                    continue;
                }

                LogReceivedXmlResponse(logger, handle, payload);

                var channel = GetOrCreatePendingRequestChannel(handle);
                await channel.Writer.WriteAsync(payload, cancellationToken);
            }
        }
    }

    public async IAsyncEnumerable<XmlRpcCallbackMessage> StreamCallbacksAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
        
        var streamChannel = Channel.CreateUnbounded<XmlRpcCallbackMessage>();

        Task HandleCallback(string methodName, object[] parameters, CancellationToken token)
        {
            streamChannel.Writer.TryWrite(new XmlRpcCallbackMessage(methodName, parameters));
            return Task.CompletedTask;
        }

        Callback += HandleCallback;

        try
        {
            await foreach (var message in streamChannel.Reader.ReadAllAsync(linkedCts.Token))
            {
                yield return message;
            }
        }
        finally
        {
            Callback -= HandleCallback;
            streamChannel.Writer.TryComplete();
        }
    }

    private async Task ProcessCallbacksAsync(CancellationToken cancellationToken)
    {
        await foreach (var (handle, xml) in callbackChannel.Reader.ReadAllAsync(cancellationToken))
        {
            var r = new MiniXmlReader(xml);

            _ = r.SkipProcessingInstruction();
            _ = r.SkipStartElement("methodCall");
            _ = r.SkipStartElement("methodName");

            var methodName = r.ReadContentAsString();

            _ = r.SkipEndElement();

            var parameters = ReadXmlRpcParams(xml, ref r);

            if (routeHandlers.TryGetValue(methodName, out var handlers))
            {
                await Task.WhenAll(handlers.Select(async handler =>
                {
                    try
                    {
                        await handler.Invoke(parameters, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Routed handler threw an exception for {MethodName}.", methodName);
                    }
                }));
            }

            var callback = Callback;
            if (callback is null) continue;

            await Task.WhenAll(callback.GetInvocationList().Select(async invocation =>
            {
                try
                {
                    await ((XmlRpcCallback)invocation).Invoke(methodName, parameters, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Global callback threw an exception for {MethodName}.", methodName);
                }
            }));
        }
    }

    public async Task<string> CallXmlAsync(string methodName, object[] methodParams, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LogCallingMethod(logger, methodName);

        var startTime = Stopwatch.GetTimestamp();
        var xmlPayload = GenerateXmlPayload(methodName, methodParams);
        var elapsed = Stopwatch.GetElapsedTime(startTime);

        LogGeneratedXml(logger, methodName, elapsed.TotalMilliseconds, xmlPayload);

        return await SendAndReceiveAsync(methodName, xmlPayload, cancellationToken);
    }

    public async Task<string> CallXmlAsync(string methodName, CancellationToken cancellationToken = default)
    {
        return await CallXmlAsync(methodName, [], cancellationToken);
    }

    public async Task<object> CallAsync(string methodName, object[] methodParams, CancellationToken cancellationToken = default)
    {
        var xmlResult = await CallXmlAsync(methodName, methodParams, cancellationToken);

        return ParseXmlRpcMethodResponse(xmlResult);
    }

    public async Task<object> CallAsync(string methodName, params object[] methodParams)
    {
        return await CallAsync(methodName, methodParams, CancellationToken.None);
    }

    public async Task<object> CallAsync(string methodName, CancellationToken cancellationToken = default)
    {
        return await CallAsync(methodName, [], cancellationToken);
    }

    public async Task<T> CallAsync<T>(string methodName, object[] methodParams, CancellationToken cancellationToken = default)
    {
        return (T)await CallAsync(methodName, methodParams, cancellationToken);
    }

    public async Task<T> CallAsync<T>(string methodName, params object[] methodParams)
    {
        return (T)await CallAsync(methodName, methodParams);
    }

    public async Task<T> CallAsync<T>(string methodName, CancellationToken cancellationToken = default)
    {
        return (T)await CallAsync(methodName, cancellationToken);
    }

    private async Task<string> SendAndReceiveAsync(string methodName, string xmlPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var semaphoreAcquired = false;
        uint? handle = null;

        try
        {
            if (v1CallSemaphore is not null)
            {
                await v1CallSemaphore.WaitAsync(cancellationToken);
                semaphoreAcquired = true;
            }

            // Determine the handle and register its response channel *before* sending the
            // request. Sending first and registering afterwards leaves a window where a very
            // fast (local) response can arrive and be discarded as an "unknown handle"
            // by ListenAsync before this method gets a chance to create the channel - the
            // subsequent read would then wait forever with no writer left to complete it.
            handle = version >= 2 ? GetNextHandle() : NoHandle;
            var channel = GetOrCreatePendingRequestChannel(handle.Value);

            var startTime = Stopwatch.GetTimestamp();

            await SendXmlPayloadAsync(handle.Value, xmlPayload, cancellationToken);

            LogMethodSent(logger, methodName, handle.Value);

            // If the connection is lost/closed while this is pending, the ListenTask completes
            // this channel's writer, which makes ReadAsync throw ChannelClosedException instead of hanging forever.
            var xml = await channel.Reader.ReadAsync(cancellationToken);

            var elapsed = Stopwatch.GetElapsedTime(startTime);

            LogMethodResponseReceived(logger, methodName, handle.Value, elapsed.TotalMilliseconds);

            return xml;
        }
        catch (ChannelClosedException ex)
        {
            throw new XmlRpcClientException($"Connection was closed while waiting for a response to {methodName}.", ex);
        }
        finally
        {
            if (handle.HasValue)
            {
                pendingRequests.Remove(handle.Value, out _);
            }

            if (semaphoreAcquired)
            {
                v1CallSemaphore?.Release();
            }
        }
    }

    // Checks whether the payload's actual root element is <methodCall> (a server-initiated callback),
    // as opposed to <methodResponse>/<fault> (a reply to our request). Anchored to the root element
    // rather than a raw substring search, since response data (e.g. string values) is not guaranteed
    // to be escaped and could otherwise coincidentally contain the literal text "<methodCall>".
    private static bool IsMethodCallPayload(string payload)
    {
        var declarationEnd = payload.IndexOf("?>", StringComparison.Ordinal);
        var rootStart = declarationEnd >= 0 ? declarationEnd + 2 : 0;
        return payload.AsSpan(rootStart).TrimStart().StartsWith("<methodCall>", StringComparison.Ordinal);
    }

    private Channel<string> GetOrCreatePendingRequestChannel(uint handle)
    {
        return pendingRequests.GetOrAdd(handle, _ => Channel.CreateBounded<string>(1));
    }

    private static object ParseXmlRpcMethodResponse(string xml)
    {
        var r = new MiniXmlReader(xml);

        _ = r.SkipProcessingInstruction();
        _ = r.SkipStartElement("methodResponse");

        var parameters = ReadXmlRpcParams(xml, ref r);
        return parameters.Length == 1 ? parameters[0] : parameters;
    }

    private static object[] ReadXmlRpcParams(string xml, ref MiniXmlReader r)
    {
        if (!r.SkipStartElement("params"))
        {
            if (!r.SkipStartElement("fault"))
            {
                throw new XmlRpcClientException(xml);
            }

            if (ReadXmlRpcValue(ref r) is not Dictionary<string, object> faultDict)
            {
                throw new XmlRpcClientException("Fault is not dictionary, cannot gather details");
            }

            if (faultDict.TryGetValue("faultString", out var faultString))
            {
                throw new XmlRpcFaultException(faultString?.ToString() ?? "Unknown fault");
            }

            throw new XmlRpcClientException("Cannot gather fault details (faultString not found)");
        }

        var parameters = new List<object>();

        while (r.SkipStartElement("param"))
        {
            parameters.Add(ReadXmlRpcValue(ref r));
            _ = r.SkipEndElement("param");
        }

        return parameters.ToArray();
    }

    private static object ReadXmlRpcValue(ref MiniXmlReader r)
    {
        _ = r.SkipStartElement("value");

        var type = r.ReadStartElement();

        object value = type switch
        {
            "i4" or "int" => int.Parse(r.ReadContent()),
            "i8" => long.Parse(r.ReadContent(), CultureInfo.InvariantCulture),
            "string" => WebUtility.HtmlDecode(r.ReadContentAsString()),
            "boolean" => r.ReadContentAsBoolean(),
            "double" => double.Parse(r.ReadContent(), NumberStyles.Number, CultureInfo.InvariantCulture),
            "struct" => ReadXmlRpcStruct(ref r),
            "array" => ReadXmlRpcArray(ref r),
            "base64" => Convert.FromBase64String(r.ReadContentAsString()),
            _ => throw new XmlRpcClientException($"Unsupported type: {type}"),
        };

        _ = r.SkipEndElement();
        _ = r.SkipEndElement("value");

        return value;
    }

    private static Dictionary<string, object> ReadXmlRpcStruct(ref MiniXmlReader r)
    {
        var dict = new Dictionary<string, object>();

        while (r.SkipStartElement("member"))
        {
            _ = r.SkipStartElement("name");
            var memberName = r.ReadContentAsString();
            _ = r.SkipEndElement("name");
            var memberValue = ReadXmlRpcValue(ref r);

            dict.Add(memberName, memberValue);

            _ = r.SkipEndElement("member");
        }

        return dict;
    }

    private static List<object> ReadXmlRpcArray(ref MiniXmlReader r)
    {
        var list = new List<object>();

        _ = r.SkipStartElement("data");

        while (!r.SkipEndElement("data"))
        {
            list.Add(ReadXmlRpcValue(ref r));
        }

        return list;
    }

    public static string GenerateXmlPayload(string methodName, object[] methodParams)
    {
        var sb = new StringBuilder("<?xml version=\"1.0\"?><methodCall><methodName>");
        sb.Append(SecurityElement.Escape(methodName));
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
        sb.Append("<param>");
        AppendXmlRpcValue(sb, param);
        sb.Append("</param>");
    }

    private static void AppendXmlRpcValue<T>(StringBuilder sb, T value)
    {
        sb.Append("<value>");

        switch (value)
        {
            case int:
            case uint:
            case ushort:
            case short:
            case byte:
            case sbyte:
                sb.Append("<int>");
                sb.Append(value);
                sb.Append("</int>");
                break;
            case long l:
                sb.Append("<i8>");
                sb.Append(l.ToString(CultureInfo.InvariantCulture));
                sb.Append("</i8>");
                break;
            case ulong ul:
                sb.Append("<i8>");
                sb.Append(ul.ToString(CultureInfo.InvariantCulture));
                sb.Append("</i8>");
                break;
            case Enum enumValue:
                sb.Append("<int>");
                sb.Append(Convert.ToInt64(enumValue, CultureInfo.InvariantCulture));
                sb.Append("</int>");
                break;
            case double doub:
                sb.Append("<double>");
                sb.Append(doub.ToString(CultureInfo.InvariantCulture));
                sb.Append("</double>");
                break;
            case float flo:
                sb.Append("<double>");
                sb.Append(flo.ToString(CultureInfo.InvariantCulture));
                sb.Append("</double>");
                break;
            case decimal dec:
                sb.Append("<double>");
                sb.Append(dec.ToString(CultureInfo.InvariantCulture));
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
                sb.Append(SecurityElement.Escape(str));
                break;
            case IEnumerable<KeyValuePair<string, object>> dict:
                sb.Append("<struct>");
                foreach (var member in dict)
                {
                    sb.Append("<member><name>");
                    sb.Append(SecurityElement.Escape(member.Key));
                    sb.Append("</name>");
                    AppendXmlRpcValue(sb, member.Value);
                    sb.Append("</member>");
                }
                sb.Append("</struct>");
                break;
            case IEnumerable enumerable:
                sb.Append("<array><data>");
                foreach (var item in enumerable)
                {
                    AppendXmlRpcValue(sb, item);
                }
                sb.Append("</data></array>");
                break;
            default:
                sb.Append(SecurityElement.Escape(value?.ToString() ?? string.Empty));
                break;
        }

        sb.Append("</value>");
    }

    private async Task SendXmlPayloadAsync(uint handle, string xmlPayload, CancellationToken cancellationToken)
    {
        var xmlPayloadByteCount = Encoding.UTF8.GetByteCount(xmlPayload);

        if (version >= 2)
        {
            const int headerSize = sizeof(uint) + sizeof(uint);

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

            // Write handle as uint32 at offset 4
            if (!BitConverter.TryWriteBytes(buffer.AsSpan().Slice(4), handle))
            {
                throw new XmlRpcClientException("Failed to write handle to buffer");
            }

            await stream.WriteAsync(buffer, cancellationToken);
        }
        else
        {
            const int headerSize = sizeof(uint);

            // uint32 xmlPayloadByteCount (4 bytes)
            // bytes xmlPayload (length of xmlPayloadByteCount)
            // GBXRemote 1 has no handle field
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

            await stream.WriteAsync(buffer, cancellationToken);
        }
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

    /// <summary>
    /// Waits until the connection is closed, whether because the client was disposed,
    /// the remote host closed it, or an error occurred while listening for messages.
    /// </summary>
    /// <param name="cancellationToken">A token that, when canceled, stops waiting without closing the connection.</param>
    public async Task WaitForCloseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await ListenTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken != cancellationToken)
        {
            // The listen loop stopped because the client was disposed - this is a normal closure.
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        tcp.Dispose();
        cts.Dispose();
        v1CallSemaphore?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        cts.Cancel();

        try
        {
            await Task.WhenAll(ListenTask, CallbackTask);
        }
        catch (OperationCanceledException)
        {
            // Expected: the listen/callback loops observe the cancellation and stop.
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "XML-RPC background task ended unexpectedly during dispose.");
        }

        tcp.Dispose();
        cts.Dispose();
        v1CallSemaphore?.Dispose();

        GC.SuppressFinalize(this);
    }
}
