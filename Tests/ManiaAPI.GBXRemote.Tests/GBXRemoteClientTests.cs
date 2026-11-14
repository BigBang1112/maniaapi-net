using System.Globalization;
using System.Threading.Channels;

namespace ManiaAPI.GBXRemote.Tests;

public class GBXRemoteClientTests
{
    private const string UnknownHeader = "GBXRemote 3";
    private const string PlayerConnect = "ManiaPlanet.PlayerConnect";

    private static readonly TimeSpan Timeout = FakeGbxRemoteServer.Timeout;

    [Test]
    [Arguments("GBXRemote 2", 2)]
    [Arguments("GBXRemote 1", 1)]
    public async Task ConnectAsync_KnownHeader_ReadsProtocolVersion(string header, int expectedVersion)
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync(header);

        var version = session.Client.Version;

        await Assert.That(version).IsEqualTo(expectedVersion);
    }

    [Test]
    public async Task ConnectAsync_UnknownHeader_Throws()
    {
        using var server = new FakeGbxRemoteServer();
        var clientTask = GBXRemoteClient.ConnectAsync(server.EndPoint).AsTask();

        using var connection = await server.AcceptAsync(UnknownHeader);

        await Assert.ThrowsAsync<GBXRemoteClientException>(() => clientTask.WaitAsync(Timeout));
    }

    [Test]
    public async Task ConnectAsync_UnknownHeader_ClosesConnection()
    {
        using var server = new FakeGbxRemoteServer();
        var clientTask = GBXRemoteClient.ConnectAsync(server.EndPoint).AsTask();
        using var connection = await server.AcceptAsync(UnknownHeader);

        await Assert.ThrowsAsync<GBXRemoteClientException>(() => clientTask.WaitAsync(Timeout));

        await connection.WaitForCloseAsync();
    }

    [Test]
    public async Task ConnectAsync_CancelledDuringHandshake_ThrowsOperationCanceled()
    {
        using var server = new FakeGbxRemoteServer();
        using var cts = new CancellationTokenSource();
        var clientTask = GBXRemoteClient.ConnectAsync(server.EndPoint, cancellationToken: cts.Token).AsTask();
        using var connection = await server.AcceptAsync(header: null);

        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => clientTask.WaitAsync(Timeout));
    }

    [Test]
    public async Task ConnectAsync_CancelledDuringHandshake_ClosesConnection()
    {
        using var server = new FakeGbxRemoteServer();
        using var cts = new CancellationTokenSource();
        var clientTask = GBXRemoteClient.ConnectAsync(server.EndPoint, cancellationToken: cts.Token).AsTask();
        using var connection = await server.AcceptAsync(header: null);

        await cts.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(() => clientTask.WaitAsync(Timeout));

        await connection.WaitForCloseAsync();
    }

    [Test]
    public async Task CallAsync_SendsMethodCallXml()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();

        _ = session.Client.CallAsync<int>("SetTimeAttackLimit", 300000);
        var (_, xml) = await session.Connection.ReadAsync();

        await Assert.That(xml).IsEqualTo("""<?xml version="1.0"?><methodCall><methodName>SetTimeAttackLimit</methodName><params><param><value><int>300000</int></value></param></params></methodCall>""");
    }

    [Test]
    public async Task CallAsync_SendsHandleWithHighBitSet()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();

        _ = session.Client.CallAsync("GetVersion");
        var (handle, _) = await session.Connection.ReadAsync();

        await Assert.That(handle).IsGreaterThanOrEqualTo(0x80000000u);
    }

    [Test]
    public async Task CallAsync_ReturnsParsedResult()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var call = session.Client.CallAsync<int>("GetTimeAttackLimit");
        var (handle, _) = await session.Connection.ReadAsync();

        await session.Connection.RespondAsync(handle, "<int>42</int>");

        await Assert.That(await call.WaitAsync(Timeout)).IsEqualTo(42);
    }

    [Test]
    public async Task CallAsync_SecondCall_UsesNextHandle()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var firstHandle = await CallAndRespondAsync(session, "<int>1</int>");

        var secondHandle = await CallAndRespondAsync(session, "<int>2</int>");

        await Assert.That(secondHandle).IsEqualTo(firstHandle + 1);
    }

    [Test]
    public async Task CallAsync_ConcurrentCalls_MatchesResponsesByHandle()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var (connection, client) = (session.Connection, session.Client);
        var first = client.CallAsync<string>("GetServerName");
        var (firstHandle, _) = await connection.ReadAsync();
        var second = client.CallAsync<string>("GetServerComment");
        var (secondHandle, _) = await connection.ReadAsync();

        // Answer in reverse order.
        await connection.RespondAsync(secondHandle, "<string>comment</string>");
        await connection.RespondAsync(firstHandle, "<string>name</string>");

        await Assert.That(await first.WaitAsync(Timeout)).IsEqualTo("name");
        await Assert.That(await second.WaitAsync(Timeout)).IsEqualTo("comment");
    }

    [Test]
    public async Task CallAsync_ConcurrentLargeCalls_SendsEachMessageWhole()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var (connection, client) = (session.Connection, session.Client);

        // Large enough to need several socket writes each, so unsynchronized writes would interleave
        const int callCount = 8;
        const int length = 4 * 1024 * 1024;

        var calls = Enumerable.Range(0, callCount)
            .Select(i => Task.Run(() => client.CallAsync("Echo", [new string((char)('a' + i), length)])))
            .ToArray();

        var seen = new HashSet<char>();

        for (var i = 0; i < callCount; i++)
        {
            var (handle, xml) = await connection.ReadAsync();

            var value = xml[(xml.IndexOf("<value>", StringComparison.Ordinal) + "<value>".Length)..xml.IndexOf("</value>", StringComparison.Ordinal)];
            await Assert.That(value.Length).IsEqualTo(length);
            await Assert.That(value.All(c => c == value[0])).IsTrue();
            await Assert.That(seen.Add(value[0])).IsTrue();

            await connection.RespondAsync(handle, "<boolean>1</boolean>");
        }

        await Task.WhenAll(calls).WaitAsync(Timeout);
    }

    [Test]
    public async Task CallAsync_StructResponse_ParsesEveryValueType()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var call = session.Client.CallAsync<Dictionary<string, object>>("GetSystemInfo");
        var (handle, _) = await session.Connection.ReadAsync();

        await session.Connection.RespondAsync(handle, """
            <struct>
              <member><name>Name</name><value><string>Tom &amp; Jerry</string></value></member>
              <member><name>Port</name><value><i4>2350</i4></value></member>
              <member><name>Uptime</name><value><i8>9000000000</i8></value></member>
              <member><name>Ratio</name><value><double>0.5</double></value></member>
              <member><name>IsServer</name><value><boolean>1</boolean></value></member>
              <member><name>Data</name><value><base64>AQID</base64></value></member>
              <member><name>Players</name><value><array><data><value><string>a</string></value><value><int>1</int></value></data></array></value></member>
            </struct>
            """);
        var info = await call.WaitAsync(Timeout);

        await Assert.That(info["Name"]).IsEqualTo("Tom & Jerry");
        await Assert.That(info["Port"]).IsEqualTo(2350);
        await Assert.That(info["Uptime"]).IsEqualTo(9_000_000_000L);
        await Assert.That(info["Ratio"]).IsEqualTo(0.5);
        await Assert.That((bool)info["IsServer"]).IsTrue();
        await Assert.That((byte[])info["Data"]).IsEquivalentTo(new byte[] { 1, 2, 3 });
        await Assert.That((List<object>)info["Players"]).IsEquivalentTo(new object[] { "a", 1 });
    }

    [Test]
    public async Task CallAsync_Fault_ThrowsWithFaultCodeAndString()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var call = session.Client.CallAsync("Kick", "nobody");
        var (handle, _) = await session.Connection.ReadAsync();

        await session.Connection.FaultAsync(handle, -1000, "Login unknown.");

        var ex = await Assert.ThrowsAsync<GBXRemoteFaultException>(() => call.WaitAsync(Timeout));
        await Assert.That(ex!.Message).IsEqualTo("Login unknown.");
        await Assert.That(ex.FaultCode).IsEqualTo(-1000);
        await Assert.That(ex.FaultString).IsEqualTo("Login unknown.");
    }

    [Test]
    public async Task CallAsync_FaultWithoutFaultString_ThrowsWithFaultCode()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var call = session.Client.CallAsync("Kick", "nobody");
        var (handle, _) = await session.Connection.ReadAsync();

        await session.Connection.SendAsync(handle, """
            <?xml version="1.0" encoding="UTF-8"?><methodResponse><fault><value><struct>
            <member><name>faultCode</name><value><int>-1000</int></value></member>
            </struct></value></fault></methodResponse>
            """);

        var ex = await Assert.ThrowsAsync<GBXRemoteFaultException>(() => call.WaitAsync(Timeout));
        await Assert.That(ex!.FaultCode).IsEqualTo(-1000);
        await Assert.That(ex.FaultString).IsNull();
        await Assert.That(ex.Message).IsEqualTo("Fault -1000");
    }

    [Test]
    public async Task CallAsync_ConnectionClosed_Throws()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var call = session.Client.CallAsync("GetVersion");
        await session.Connection.ReadAsync();

        session.Connection.Dispose();

        await Assert.ThrowsAsync<GBXRemoteClientException>(() => call.WaitAsync(Timeout));
    }

    [Test]
    public async Task CallAsync_AfterServerClosedConnection_Throws()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        session.Connection.Dispose();
        await session.Client.WaitForCloseAsync().WaitAsync(Timeout);

        await Assert.ThrowsAsync<GBXRemoteClientException>(() => session.Client.CallAsync("GetVersion").WaitAsync(Timeout));
    }

    [Test]
    [Arguments(-1)]
    [Arguments(64 * 1024 * 1024 + 1)] // One byte over the client's limit
    public async Task CallAsync_ServerSendsInvalidMessageSize_Throws(int payloadSize)
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var call = session.Client.CallAsync("GetVersion");
        var (handle, _) = await session.Connection.ReadAsync();

        await session.Connection.SendHeaderAsync(payloadSize, handle);

        var ex = await Assert.ThrowsAsync<GBXRemoteClientException>(() => call.WaitAsync(Timeout));

        // The call names the error that lost the connection as its cause
        await Assert.That(ex!.InnerException).IsTypeOf<GBXRemoteClientException>();
        await Assert.That(ex.InnerException!.Message).Contains("invalid size");
    }

    [Test]
    public async Task WaitForCloseAsync_ServerSendsInvalidMessageSize_ThrowsWithCause()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();

        await session.Connection.SendHeaderAsync(-1, 0x80000000);

        var ex = await Assert.ThrowsAsync<GBXRemoteClientException>(() => session.Client.WaitForCloseAsync().WaitAsync(Timeout));

        await Assert.That(ex!.InnerException).IsTypeOf<GBXRemoteClientException>();
        await Assert.That(ex.InnerException!.Message).Contains("invalid size");
    }

    [Test]
    public async Task WaitForCloseAsync_ClientDisposed_Completes()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();

        session.Client.Dispose();

        await session.Client.WaitForCloseAsync().WaitAsync(Timeout);
    }

    [Test]
    public async Task CallAsync_NegativeIntInHebrewCulture_ParsesInvariant()
    {
        var culture = CultureInfo.CurrentCulture;
        // Hebrew uses U+200E before the minus sign, so parsing a plain "-5" with it fails
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("he-IL");

        try
        {
            await using var session = await FakeGbxRemoteServer.ConnectAsync();
            var call = session.Client.CallAsync<int>("GetNumber");
            var (handle, _) = await session.Connection.ReadAsync();

            await session.Connection.RespondAsync(handle, "<int>-5</int>");

            await Assert.That(await call.WaitAsync(Timeout)).IsEqualTo(-5);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Test]
    public async Task CallAsync_Version1_SendsFrameWithoutHandle()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync("GBXRemote 1");

        _ = session.Client.CallAsync<string>("GetServerName");
        var (_, xml) = await session.Connection.ReadAsync();

        await Assert.That(xml).Contains("<methodName>GetServerName</methodName>");
    }

    [Test]
    public async Task CallAsync_Version1_ReadsResponseWithoutHandle()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync("GBXRemote 1");
        var call = session.Client.CallAsync<string>("GetServerName");
        await session.Connection.ReadAsync();

        await session.Connection.RespondAsync(0, "<string>name</string>");

        await Assert.That(await call.WaitAsync(Timeout)).IsEqualTo("name");
    }

    [Test]
    public async Task CallAsync_Version1_CallAfterCancelledCall_GetsItsOwnResponse()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync("GBXRemote 1");
        using var cancellation = new CancellationTokenSource();
        var cancelled = session.Client.CallAsync<string>("GetServerName", [], cancellation.Token);
        await session.Connection.ReadAsync();
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => cancelled.WaitAsync(Timeout));

        var next = session.Client.CallAsync<string>("GetVersion");
        // The response to the cancelled call arrives late, before the response to the next call
        await session.Connection.RespondAsync(0, "<string>late</string>");
        await session.Connection.ReadAsync();
        await session.Connection.RespondAsync(0, "<string>own</string>");

        await Assert.That(await next.WaitAsync(Timeout)).IsEqualTo("own");
    }

    [Test]
    public async Task CallAsync_AfterDispose_ThrowsGBXRemoteClientException()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();

        session.Client.Dispose();

        await Assert.ThrowsAsync<GBXRemoteClientException>(() => session.Client.CallAsync("GetVersion").WaitAsync(Timeout));
    }

    [Test]
    public async Task WaitForCloseAsync_ServerCloses_Completes()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();

        session.Connection.Dispose();

        await session.Client.WaitForCloseAsync().WaitAsync(Timeout);
    }

    [Test]
    public async Task On_ServerSendsCallback_InvokesHandlerWithParameters()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var received = new TaskCompletionSource<object[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        session.Client.On(PlayerConnect, (parameters, _) =>
        {
            received.TrySetResult(parameters);
            return Task.CompletedTask;
        });

        await session.Connection.SendCallbackAsync(PlayerConnect,
            "<param><value><string>bigbang1112</string></value></param><param><value><boolean>0</boolean></value></param>");

        await Assert.That(await received.Task.WaitAsync(Timeout)).IsEquivalentTo(new object[] { "bigbang1112", false });
    }

    [Test]
    public async Task On_ServerSendsUnreadableCallback_StillInvokesHandlerForNextCallback()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var received = new TaskCompletionSource<object[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        session.Client.On(PlayerConnect, (parameters, _) =>
        {
            received.TrySetResult(parameters);
            return Task.CompletedTask;
        });

        // dateTime.iso8601 isn't a supported type, so this callback can't be read
        await session.Connection.SendCallbackAsync(PlayerConnect,
            "<param><value><dateTime.iso8601>20260101T00:00:00</dateTime.iso8601></value></param>");
        await session.Connection.SendCallbackAsync(PlayerConnect,
            "<param><value><string>bigbang1112</string></value></param>");

        await Assert.That(await received.Task.WaitAsync(Timeout)).IsEquivalentTo(new object[] { "bigbang1112" });
    }

    [Test]
    public async Task Callback_ServerSendsCallback_RaisesEventWithMethodName()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        session.Client.Callback += (methodName, _, _) =>
        {
            received.TrySetResult(methodName);
            return Task.CompletedTask;
        };

        await session.Connection.SendCallbackAsync(PlayerConnect, "<param><value><string>bigbang1112</string></value></param>");

        await Assert.That(await received.Task.WaitAsync(Timeout)).IsEqualTo(PlayerConnect);
    }

    [Test]
    public async Task StreamCallbacksAsync_ServerSendsCallback_YieldsCallback()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();

        var callback = await StreamFirstCallbackAsync(session, "TrackMania.PlayerFinish", "<param><value><int>45000</int></value></param>");

        await Assert.That(callback.MethodName).IsEqualTo("TrackMania.PlayerFinish");
        await Assert.That(callback.MethodParams).IsEquivalentTo(new object[] { 45000 });
    }

    [Test]
    public async Task SystemMulticallAsync_SendsSystemMulticall()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();

        _ = session.Client.SystemMulticallAsync([new GBXRemoteCall("GetVersion"), new GBXRemoteCall("Kick", "nobody")]);
        var (_, xml) = await session.Connection.ReadAsync();

        await Assert.That(xml).Contains("<methodName>system.multicall</methodName>");
    }

    [Test]
    public async Task SystemMulticallAsync_ReturnsSuccessesAndFaults()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var call = session.Client.SystemMulticallAsync([new GBXRemoteCall("GetVersion"), new GBXRemoteCall("Kick", "nobody")]);
        var (handle, _) = await session.Connection.ReadAsync();

        await session.Connection.RespondAsync(handle, """
            <array><data>
              <value><array><data><value><string>ok</string></value></data></array></value>
              <value><struct>
                <member><name>faultCode</name><value><int>-1000</int></value></member>
                <member><name>faultString</name><value><string>Login unknown.</string></value></member>
              </struct></value>
            </data></array>
            """);
        var results = (await call.WaitAsync(Timeout)).ToList();

        await Assert.That(results).IsEquivalentTo([
            new GBXRemoteCallResult("ok", null, null),
            new GBXRemoteCallResult(null, -1000, "Login unknown.")]);
    }

    /// <summary>
    /// Makes a call and answers it, returning the handle the client used.
    /// </summary>
    private static async Task<uint> CallAndRespondAsync(FakeGbxRemoteSession session, string valueXml)
    {
        var call = session.Client.CallAsync("GetVersion");
        var (handle, _) = await session.Connection.ReadAsync();
        await session.Connection.RespondAsync(handle, valueXml);
        await call.WaitAsync(Timeout);
        return handle;
    }

    /// <summary>
    /// Streams callbacks and returns the first one. The stream subscribes asynchronously,
    /// so the callback is sent again until it arrives.
    /// </summary>
    private static async Task<GBXRemoteCallbackMessage> StreamFirstCallbackAsync(FakeGbxRemoteSession session, string methodName, string paramsXml)
    {
        using var cts = new CancellationTokenSource(Timeout);
        var received = Channel.CreateUnbounded<GBXRemoteCallbackMessage>();
        _ = Task.Run(async () =>
        {
            await foreach (var message in session.Client.StreamCallbacksAsync(cts.Token))
            {
                await received.Writer.WriteAsync(message);
            }
        });

        try
        {
            while (true)
            {
                await session.Connection.SendCallbackAsync(methodName, paramsXml);

                using var wait = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
                wait.CancelAfter(TimeSpan.FromMilliseconds(100));

                try
                {
                    return await received.Reader.ReadAsync(wait.Token);
                }
                catch (OperationCanceledException) when (!cts.IsCancellationRequested)
                {
                    // Not subscribed yet, send it again.
                }
            }
        }
        finally
        {
            await cts.CancelAsync();
        }
    }
}
