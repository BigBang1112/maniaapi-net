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

namespace ManiaAPI.GBXRemote;

public partial class GBXRemoteClient : IDisposable, IAsyncDisposable
{
    private const string HandshakeV1 = "GBXRemote 1";
    private const string HandshakeV2 = "GBXRemote 2";

    // GBXRemote 1 messages carry no handle, so a fixed placeholder is used to key pending requests
    private const uint NoHandle = 0;

    // Far above any real response, but stops a corrupt length prefix from allocating gigabytes
    private const int MaxPayloadSize = 64 * 1024 * 1024;

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

    // NetworkStream supports one writer at a time, so concurrent calls take turns sending their message.
    // Only the write is guarded: GBXRemote 2 calls still wait for their responses concurrently.
    private readonly SemaphoreSlim writeSemaphore = new(1, 1);

    private readonly TcpClient tcp;
    private readonly int version;
    private readonly ILogger<GBXRemoteClient> logger;

    private readonly NetworkStream stream;

    private readonly CancellationTokenSource cts = new();

    public bool IsWaitingForMessages { get; private set; }

    private Task ListenTask { get; }
    private Task CallbackTask { get; }

    public int Version => version;

    public event GBXRemoteCallback? Callback;

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

    [LoggerMessage(EventId = 8, Level = LogLevel.Error, Message = "Failed to read XML callback (0x{Handle:x8}), skipping...")]
    private static partial void LogInvalidCallback(ILogger logger, Exception exception, uint handle);

    private GBXRemoteClient(TcpClient tcp, int version, ILogger<GBXRemoteClient> logger)
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
            catch (OperationCanceledException) {}
            catch (Exception ex)
            {
                logger.LogError(ex, "Listen loop terminated unexpectedly.");
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
    /// Connects to the XML-RPC port of a game server.
    /// </summary>
    /// <exception cref="SocketException">The connection couldn't be established.</exception>
    /// <exception cref="GBXRemoteClientException">Header is invalid.</exception>
    public static async ValueTask<GBXRemoteClient> ConnectAsync(
        string ip,
        int port = 5000,
        ILogger<GBXRemoteClient>? logger = null,
        CancellationToken cancellationToken = default)
    {
        return await ConnectCoreAsync(tcp => tcp.ConnectAsync(ip, port, cancellationToken), logger, cancellationToken);
    }

    /// <summary>
    /// Connects to the XML-RPC port of a game server.
    /// </summary>
    /// <exception cref="SocketException">The connection couldn't be established.</exception>
    /// <exception cref="GBXRemoteClientException">Header is invalid.</exception>
    public static async ValueTask<GBXRemoteClient> ConnectAsync(
        IPAddress ip,
        int port = 5000,
        ILogger<GBXRemoteClient>? logger = null,
        CancellationToken cancellationToken = default)
    {
        return await ConnectCoreAsync(tcp => tcp.ConnectAsync(ip, port, cancellationToken), logger, cancellationToken);
    }

    /// <summary>
    /// Connects to the XML-RPC port of a game server.
    /// </summary>
    /// <exception cref="SocketException">The connection couldn't be established.</exception>
    /// <exception cref="GBXRemoteClientException">Header is invalid.</exception>
    public static async ValueTask<GBXRemoteClient> ConnectAsync(
        IPEndPoint endpoint,
        ILogger<GBXRemoteClient>? logger = null,
        CancellationToken cancellationToken = default)
    {
        return await ConnectCoreAsync(tcp => tcp.ConnectAsync(endpoint, cancellationToken), logger, cancellationToken);
    }

    private static async ValueTask<GBXRemoteClient> ConnectCoreAsync(
        Func<TcpClient, ValueTask> connect,
        ILogger<GBXRemoteClient>? logger,
        CancellationToken cancellationToken)
    {
        var tcp = new TcpClient();

        try
        {
            await connect(tcp);
            var version = await ValidateHeaderOrThrowAsync(tcp.GetStream(), cancellationToken);
            return new GBXRemoteClient(tcp, version, logger ?? NullLogger<GBXRemoteClient>.Instance);
        }
        catch
        {
            tcp.Dispose();
            throw;
        }
    }

    private static async Task<int> ValidateHeaderOrThrowAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var lengthBuffer = new byte[4];
        await stream.ReadExactlyAsync(lengthBuffer, cancellationToken);
        
        if (lengthBuffer[1] != 0 || lengthBuffer[2] != 0 || lengthBuffer[3] != 0)
        {
            throw new GBXRemoteClientException("GBXRemote header has invalid length");
        }

        var length = lengthBuffer[0];
        var headerBuffer = new byte[length];
        await stream.ReadExactlyAsync(headerBuffer, cancellationToken);

        var header = Encoding.ASCII.GetString(headerBuffer);

        return header switch
        {
            HandshakeV1 => 1,
            HandshakeV2 => 2,
            _ => throw new GBXRemoteClientException($"GBXRemote header is invalid: {header}"),
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

            if (payloadSize < 0 || payloadSize > MaxPayloadSize)
            {
                // The framing can't be trusted anymore, so the connection can't continue
                throw new GBXRemoteClientException($"GBXRemote message has invalid size ({payloadSize} bytes, maximum is {MaxPayloadSize} bytes)");
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
                // Looked up once, so a channel removed by a caller that gave up isn't added back
                if (!pendingRequests.TryGetValue(handle, out var channel))
                {
                    LogUnknownHandle(logger, handle);
                    continue;
                }

                LogReceivedXmlResponse(logger, handle, payload);

                // A channel takes the one response to its call, and a duplicate response must not block this loop
                channel.Writer.TryWrite(payload);
            }
        }
    }

    public async IAsyncEnumerable<GBXRemoteCallbackMessage> StreamCallbacksAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
        
        var streamChannel = Channel.CreateUnbounded<GBXRemoteCallbackMessage>();

        Task HandleCallback(string methodName, object[] parameters, CancellationToken token)
        {
            streamChannel.Writer.TryWrite(new GBXRemoteCallbackMessage(methodName, parameters));
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
            string methodName;
            object[] parameters;

            try
            {
                parameters = ParseXmlRpcMethodCall(xml, out methodName);
            }
            catch (Exception ex)
            {
                // One unreadable callback must not stop the ones after it
                LogInvalidCallback(logger, ex, handle);
                continue;
            }

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
                    await ((GBXRemoteCallback)invocation).Invoke(methodName, parameters, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Global callback threw an exception for {MethodName}.", methodName);
                }
            }));
        }
    }

    /// <summary>
    /// Calls a method on the game server and returns the raw XML response, faults included.
    /// </summary>
    /// <exception cref="GBXRemoteClientException">The connection was closed before the response arrived.</exception>
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

    /// <summary>
    /// Calls a method on the game server and reads its response.
    /// </summary>
    /// <returns>The single returned value, or an array when the method returns several.</returns>
    /// <exception cref="GBXRemoteFaultException">The server answered with a fault, like wrong credentials or an unknown login.</exception>
    /// <exception cref="GBXRemoteClientException">The connection was closed before the response arrived, or the response couldn't be read.</exception>
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
        Channel<string>? channel = null;
        var awaitingResponse = false;

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
            channel = GetOrCreatePendingRequestChannel(handle.Value);

            // The ListenTask cancels cts once the connection is lost/closed. Watching it covers calls made after that,
            // whose channel the ListenTask never completes, and whose write can still succeed into the socket buffer.
            using var connectionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

            var startTime = Stopwatch.GetTimestamp();

            await SendXmlPayloadAsync(handle.Value, xmlPayload, connectionCts.Token);
            awaitingResponse = true;

            LogMethodSent(logger, methodName, handle.Value);

            // If the connection is lost/closed while this is pending, the ListenTask completes
            // this channel's writer, which makes ReadAsync throw ChannelClosedException instead of hanging forever.
            var xml = await channel.Reader.ReadAsync(connectionCts.Token);
            awaitingResponse = false;

            var elapsed = Stopwatch.GetElapsedTime(startTime);

            LogMethodResponseReceived(logger, methodName, handle.Value, elapsed.TotalMilliseconds);

            return xml;
        }
        catch (ChannelClosedException ex)
        {
            throw new GBXRemoteClientException($"Connection was closed before a response to {methodName} arrived.", ex);
        }
        catch (OperationCanceledException ex) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new GBXRemoteClientException($"Connection was closed before a response to {methodName} arrived.", ex);
        }
        finally
        {
            // GBXRemote 1 responses have no handle, so the response to a call given up on would be taken as the response
            // to the next call. The call stays registered until that response arrives and is thrown away.
            if (semaphoreAcquired && awaitingResponse)
            {
                _ = DiscardLateResponseAsync(channel!);
            }
            else
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
    }

    /// <summary>
    /// Waits for the response to a GBXRemote 1 call that was given up on, then lets the next call go.
    /// </summary>
    private async Task DiscardLateResponseAsync(Channel<string> channel)
    {
        try
        {
            // The ListenTask completes the channel or cancels cts once the connection is closed
            await channel.Reader.ReadAsync(cts.Token);
        }
        catch (Exception ex) when (ex is ChannelClosedException or OperationCanceledException)
        {
        }
        finally
        {
            pendingRequests.Remove(NoHandle, out _);
            v1CallSemaphore!.Release();
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

    private static object[] ParseXmlRpcMethodCall(string xml, out string methodName)
    {
        var r = new MiniXmlReader(xml);

        _ = r.SkipProcessingInstruction();
        _ = r.SkipStartElement("methodCall");
        _ = r.SkipStartElement("methodName");

        methodName = r.ReadContentAsString();

        _ = r.SkipEndElement();

        return ReadXmlRpcParams(xml, ref r);
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
                throw new GBXRemoteClientException(xml);
            }

            if (ReadXmlRpcValue(ref r) is not Dictionary<string, object> faultDict)
            {
                throw new GBXRemoteClientException("Fault is not dictionary, cannot gather details");
            }

            throw new GBXRemoteFaultException(
                faultDict.TryGetValue("faultCode", out var faultCode) ? faultCode as int? : null,
                faultDict.TryGetValue("faultString", out var faultString) ? faultString?.ToString() : null);
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
            "i4" or "int" => int.Parse(r.ReadContent(), CultureInfo.InvariantCulture),
            "i8" => long.Parse(r.ReadContent(), CultureInfo.InvariantCulture),
            "string" => WebUtility.HtmlDecode(r.ReadContentAsString()),
            "boolean" => r.ReadContentAsBoolean(),
            "double" => double.Parse(r.ReadContent(), NumberStyles.Number, CultureInfo.InvariantCulture),
            "struct" => ReadXmlRpcStruct(ref r),
            "array" => ReadXmlRpcArray(ref r),
            "base64" => Convert.FromBase64String(r.ReadContentAsString()),
            _ => throw new GBXRemoteClientException($"Unsupported type: {type}"),
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
                // Not StringBuilder.Append(int), which formats with the current culture (like U+2212 as the minus sign)
                sb.Append("<int>");
                sb.Append(((IFormattable)value).ToString(null, CultureInfo.InvariantCulture));
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
                sb.Append(Convert.ToInt64(enumValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture));
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
                throw new GBXRemoteClientException($"Invalid string buffering (expected {xmlPayloadByteCount} bytes, got {bufferedXmlPayloadByteCount} bytes)");
            }

            if (!BitConverter.TryWriteBytes(buffer, xmlPayloadByteCount))
            {
                throw new GBXRemoteClientException("Failed to write XML payload byte count to buffer");
            }

            // Write handle as uint32 at offset 4
            if (!BitConverter.TryWriteBytes(buffer.AsSpan().Slice(4), handle))
            {
                throw new GBXRemoteClientException("Failed to write handle to buffer");
            }

            await WriteAsync(buffer, cancellationToken);
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
                throw new GBXRemoteClientException($"Invalid string buffering (expected {xmlPayloadByteCount} bytes, got {bufferedXmlPayloadByteCount} bytes)");
            }

            if (!BitConverter.TryWriteBytes(buffer, xmlPayloadByteCount))
            {
                throw new GBXRemoteClientException("Failed to write XML payload byte count to buffer");
            }

            await WriteAsync(buffer, cancellationToken);
        }
    }

    private async Task WriteAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        await writeSemaphore.WaitAsync(cancellationToken);

        try
        {
            await stream.WriteAsync(buffer, cancellationToken);
        }
        finally
        {
            writeSemaphore.Release();
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

    // cts and the semaphores aren't disposed. The background loops, calls in progress, and discarding of late GBXRemote 1
    // responses can still use them after disposal returns. They create no timer or wait handle, so they hold nothing to release.

    public void Dispose()
    {
        cts.Cancel();
        tcp.Dispose();
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
            logger.LogWarning(ex, "GBXRemote background task ended unexpectedly during dispose.");
        }

        tcp.Dispose();

        GC.SuppressFinalize(this);
    }
}
