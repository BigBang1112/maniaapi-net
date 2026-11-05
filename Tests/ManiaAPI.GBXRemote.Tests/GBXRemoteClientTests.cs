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

        await Assert.ThrowsAsync<ClientException>(() => clientTask.WaitAsync(Timeout));
    }

    [Test]
    public async Task ConnectAsync_UnknownHeader_ClosesConnection()
    {
        using var server = new FakeGbxRemoteServer();
        var clientTask = GBXRemoteClient.ConnectAsync(server.EndPoint).AsTask();
        using var connection = await server.AcceptAsync(UnknownHeader);

        await Assert.ThrowsAsync<ClientException>(() => clientTask.WaitAsync(Timeout));

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
    public async Task CallAsync_Fault_ThrowsWithFaultString()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var call = session.Client.CallAsync("Kick", "nobody");
        var (handle, _) = await session.Connection.ReadAsync();

        await session.Connection.FaultAsync(handle, -1000, "Login unknown.");

        var ex = await Assert.ThrowsAsync<FaultException>(() => call.WaitAsync(Timeout));
        await Assert.That(ex!.Message).IsEqualTo("Login unknown.");
    }

    [Test]
    public async Task CallAsync_ConnectionClosed_Throws()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var call = session.Client.CallAsync("GetVersion");
        await session.Connection.ReadAsync();

        session.Connection.Dispose();

        await Assert.ThrowsAsync<ClientException>(() => call.WaitAsync(Timeout));
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
