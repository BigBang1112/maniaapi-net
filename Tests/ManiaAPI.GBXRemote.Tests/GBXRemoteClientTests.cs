using System.Threading.Channels;

namespace ManiaAPI.GBXRemote.Tests;

public class GBXRemoteClientTests
{
    private static readonly TimeSpan Timeout = FakeGbxRemoteServer.Timeout;

    [Test]
    [Arguments("GBXRemote 2", 2)]
    [Arguments("GBXRemote 1", 1)]
    public async Task ConnectAsync_ReadsProtocolVersionFromHeader(string header, int expectedVersion)
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync(header);

        await Assert.That(session.Client.Version).IsEqualTo(expectedVersion);
    }

    [Test]
    public async Task ConnectAsync_UnknownHeader_Throws()
    {
        using var server = new FakeGbxRemoteServer();
        var clientTask = GBXRemoteClient.ConnectAsync(server.EndPoint).AsTask();
        using var connection = await server.AcceptAsync("GBXRemote 3");

        await Assert.ThrowsAsync<ClientException>(() => clientTask.WaitAsync(Timeout));
    }

    [Test]
    public async Task CallAsync_SendsMethodCallWithResponseHandleAndParsesResult()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var (connection, client) = (session.Connection, session.Client);

        var call = client.CallAsync<int>("SetTimeAttackLimit", 300000);
        var (handle, xml) = await connection.ReadAsync();
        await connection.RespondAsync(handle, "<int>42</int>");

        await Assert.That(await call.WaitAsync(Timeout)).IsEqualTo(42);
        await Assert.That(handle).IsGreaterThanOrEqualTo(0x80000000u);
        await Assert.That(xml).IsEqualTo("""<?xml version="1.0"?><methodCall><methodName>SetTimeAttackLimit</methodName><params><param><value><int>300000</int></value></param></params></methodCall>""");
    }

    [Test]
    public async Task CallAsync_EachCallGetsNextHandle()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var (connection, client) = (session.Connection, session.Client);

        var first = client.CallAsync("GetVersion");
        var (firstHandle, _) = await connection.ReadAsync();
        await connection.RespondAsync(firstHandle, "<int>1</int>");
        await first.WaitAsync(Timeout);

        var second = client.CallAsync("GetVersion");
        var (secondHandle, _) = await connection.ReadAsync();
        await connection.RespondAsync(secondHandle, "<int>2</int>");
        await second.WaitAsync(Timeout);

        await Assert.That(secondHandle).IsEqualTo(firstHandle + 1);
    }

    [Test]
    public async Task CallAsync_ConcurrentCalls_AreMatchedByHandle()
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
    public async Task CallAsync_ParsesNestedValues()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var (connection, client) = (session.Connection, session.Client);

        var call = client.CallAsync<Dictionary<string, object>>("GetSystemInfo");
        var (handle, _) = await connection.ReadAsync();
        await connection.RespondAsync(handle, """
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
        var (connection, client) = (session.Connection, session.Client);

        var call = client.CallAsync("Kick", "nobody");
        var (handle, _) = await connection.ReadAsync();
        await connection.FaultAsync(handle, -1000, "Login unknown.");

        var ex = await Assert.ThrowsAsync<FaultException>(() => call.WaitAsync(Timeout));

        await Assert.That(ex!.Message).IsEqualTo("Login unknown.");
    }

    [Test]
    public async Task CallAsync_ConnectionClosed_ThrowsInsteadOfHanging()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var (connection, client) = (session.Connection, session.Client);

        var call = client.CallAsync("GetVersion");
        await connection.ReadAsync();
        connection.Dispose();

        await Assert.ThrowsAsync<ClientException>(() => call.WaitAsync(Timeout));
    }

    [Test]
    public async Task WaitForCloseAsync_CompletesWhenServerCloses()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var (connection, client) = (session.Connection, session.Client);

        connection.Dispose();

        await client.WaitForCloseAsync().WaitAsync(Timeout);
    }

    [Test]
    public async Task CallAsync_Version1_UsesFramingWithoutHandle()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync("GBXRemote 1");
        var (connection, client) = (session.Connection, session.Client);

        var call = client.CallAsync<string>("GetServerName");
        var (_, xml) = await connection.ReadAsync();
        await connection.RespondAsync(0, "<string>name</string>");

        await Assert.That(xml).Contains("<methodName>GetServerName</methodName>");
        await Assert.That(await call.WaitAsync(Timeout)).IsEqualTo("name");
    }

    [Test]
    public async Task Callbacks_ReachRouteHandlerEventAndStream()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var (connection, client) = (session.Connection, session.Client);

        var routed = new TaskCompletionSource<object[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        var evented = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        client.On("ManiaPlanet.PlayerConnect", (parameters, _) =>
        {
            routed.TrySetResult(parameters);
            return Task.CompletedTask;
        });

        client.Callback += (methodName, _, _) =>
        {
            evented.TrySetResult(methodName);
            return Task.CompletedTask;
        };

        await connection.SendCallbackAsync("ManiaPlanet.PlayerConnect",
            "<param><value><string>bigbang1112</string></value></param><param><value><boolean>0</boolean></value></param>");

        var parameters = await routed.Task.WaitAsync(Timeout);
        await Assert.That(parameters).IsEquivalentTo(new object[] { "bigbang1112", false });
        await Assert.That(await evented.Task.WaitAsync(Timeout)).IsEqualTo("ManiaPlanet.PlayerConnect");
    }

    [Test]
    public async Task StreamCallbacksAsync_YieldsCallbacks()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var (connection, client) = (session.Connection, session.Client);

        using var cts = new CancellationTokenSource(Timeout);
        var received = Channel.CreateUnbounded<GBXRemoteCallbackMessage>();
        var streaming = Task.Run(async () =>
        {
            await foreach (var message in client.StreamCallbacksAsync(cts.Token))
            {
                await received.Writer.WriteAsync(message);
            }
        });

        // The stream subscribes asynchronously, so keep sending until the first callback arrives.
        GBXRemoteCallbackMessage? first = null;
        while (first is null)
        {
            await connection.SendCallbackAsync("TrackMania.PlayerFinish", "<param><value><int>45000</int></value></param>");
            using var wait = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            try { first = await received.Reader.ReadAsync(wait.Token); } catch (OperationCanceledException) { cts.Token.ThrowIfCancellationRequested(); }
        }

        cts.Cancel();
        await Assert.That(first.MethodName).IsEqualTo("TrackMania.PlayerFinish");
        await Assert.That(first.MethodParams).IsEquivalentTo(new object[] { 45000 });
    }

    [Test]
    public async Task SystemMulticallAsync_ParsesSuccessesAndFaults()
    {
        await using var session = await FakeGbxRemoteServer.ConnectAsync();
        var (connection, client) = (session.Connection, session.Client);

        var call = client.SystemMulticallAsync([new GBXRemoteCall("GetVersion"), new GBXRemoteCall("Kick", "nobody")]);
        var (handle, xml) = await connection.ReadAsync();
        await connection.RespondAsync(handle, """
            <array><data>
              <value><array><data><value><string>ok</string></value></data></array></value>
              <value><struct>
                <member><name>faultCode</name><value><int>-1000</int></value></member>
                <member><name>faultString</name><value><string>Login unknown.</string></value></member>
              </struct></value>
            </data></array>
            """);

        var results = (await call.WaitAsync(Timeout)).ToList();

        await Assert.That(xml).Contains("<methodName>system.multicall</methodName>");
        await Assert.That(results[0]).IsEqualTo(new GBXRemoteCallResult("ok", null, null));
        await Assert.That(results[1].IsFault).IsTrue();
        await Assert.That(results[1].FaultCode).IsEqualTo(-1000);
        await Assert.That(results[1].FaultString).IsEqualTo("Login unknown.");
    }
}
