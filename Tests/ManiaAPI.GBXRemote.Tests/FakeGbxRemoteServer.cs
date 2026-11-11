using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;

namespace ManiaAPI.GBXRemote.Tests;

/// <summary>
/// A loopback TCP server that speaks the GBXRemote framing, so the tests control every byte the client receives.
/// </summary>
internal sealed class FakeGbxRemoteServer : IDisposable
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private readonly TcpListener listener;

    public IPEndPoint EndPoint => (IPEndPoint)listener.LocalEndpoint;

    public FakeGbxRemoteServer()
    {
        listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
    }

    /// <summary>
    /// Accepts the next client and sends it the handshake header. A null header sends nothing.
    /// </summary>
    public async Task<FakeGbxRemoteConnection> AcceptAsync(string? header = "GBXRemote 2")
    {
        var tcp = await listener.AcceptTcpClientAsync().WaitAsync(Timeout);

        if (header is null)
        {
            return new FakeGbxRemoteConnection(tcp, version: 2);
        }

        var stream = tcp.GetStream();

        var headerBytes = Encoding.ASCII.GetBytes(header);
        var lengthBytes = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(lengthBytes, headerBytes.Length);
        await stream.WriteAsync(lengthBytes);
        await stream.WriteAsync(headerBytes);

        return new FakeGbxRemoteConnection(tcp, version: header == "GBXRemote 1" ? 1 : 2);
    }

    /// <summary>
    /// Starts a server, connects a client to it, and completes the handshake.
    /// </summary>
    public static async Task<FakeGbxRemoteSession> ConnectAsync(string header = "GBXRemote 2")
    {
        var server = new FakeGbxRemoteServer();
        var clientTask = GBXRemoteClient.ConnectAsync(server.EndPoint).AsTask();
        var connection = await server.AcceptAsync(header);
        var client = await clientTask.WaitAsync(Timeout);
        return new FakeGbxRemoteSession(server, connection, client);
    }

    public void Dispose() => listener.Stop();
}

internal sealed record FakeGbxRemoteSession(FakeGbxRemoteServer Server, FakeGbxRemoteConnection Connection, GBXRemoteClient Client) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await Client.DisposeAsync();
        Connection.Dispose();
        Server.Dispose();
    }
}

internal sealed class FakeGbxRemoteConnection : IDisposable
{
    private readonly TcpClient tcp;
    private readonly NetworkStream stream;
    private readonly int version;

    public FakeGbxRemoteConnection(TcpClient tcp, int version)
    {
        this.tcp = tcp;
        this.version = version;
        stream = tcp.GetStream();
    }

    /// <summary>
    /// Reads one message sent by the client. GBXRemote 1 messages have no handle, so it is reported as 0.
    /// </summary>
    public async Task<(uint Handle, string Xml)> ReadAsync()
    {
        var header = new byte[version >= 2 ? 8 : 4];
        await stream.ReadExactlyAsync(header).AsTask().WaitAsync(FakeGbxRemoteServer.Timeout);

        var length = BinaryPrimitives.ReadInt32LittleEndian(header);
        var handle = version >= 2 ? BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4)) : 0;

        var payload = new byte[length];
        await stream.ReadExactlyAsync(payload).AsTask().WaitAsync(FakeGbxRemoteServer.Timeout);

        return (handle, Encoding.UTF8.GetString(payload));
    }

    public async Task SendAsync(uint handle, string xml)
    {
        var payload = Encoding.UTF8.GetBytes(xml);
        var header = new byte[version >= 2 ? 8 : 4];
        BinaryPrimitives.WriteInt32LittleEndian(header, payload.Length);

        if (version >= 2)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4), handle);
        }

        await stream.WriteAsync(header);
        await stream.WriteAsync(payload);
    }

    /// <summary>
    /// Sends only a message header that announces the given payload size, without any payload.
    /// </summary>
    public async Task SendHeaderAsync(int payloadSize, uint handle)
    {
        var header = new byte[version >= 2 ? 8 : 4];
        BinaryPrimitives.WriteInt32LittleEndian(header, payloadSize);

        if (version >= 2)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4), handle);
        }

        await stream.WriteAsync(header);
    }

    /// <summary>
    /// Answers a call with a single return value, given as the inner XML of &lt;value&gt;.
    /// </summary>
    public Task RespondAsync(uint handle, string valueXml)
    {
        return SendAsync(handle, $"""<?xml version="1.0" encoding="UTF-8"?><methodResponse><params><param><value>{valueXml}</value></param></params></methodResponse>""");
    }

    public Task FaultAsync(uint handle, int code, string message)
    {
        return SendAsync(handle, $"""
            <?xml version="1.0" encoding="UTF-8"?><methodResponse><fault><value><struct>
            <member><name>faultCode</name><value><int>{code}</int></value></member>
            <member><name>faultString</name><value><string>{message}</string></value></member>
            </struct></value></fault></methodResponse>
            """);
    }

    /// <summary>
    /// Sends a server callback. Callbacks use handles below 0x80000000.
    /// </summary>
    public Task SendCallbackAsync(string methodName, string paramsXml, uint handle = 1)
    {
        return SendAsync(handle, $"""<?xml version="1.0" encoding="UTF-8"?><methodCall><methodName>{methodName}</methodName><params>{paramsXml}</params></methodCall>""");
    }

    /// <summary>
    /// Completes once the client closes its end of the connection.
    /// </summary>
    public async Task WaitForCloseAsync()
    {
        var buffer = new byte[1];
        var read = await stream.ReadAsync(buffer).AsTask().WaitAsync(FakeGbxRemoteServer.Timeout);

        if (read != 0)
        {
            throw new InvalidOperationException("Expected the client to close the connection, but it sent data.");
        }
    }

    public void Dispose() => tcp.Dispose();
}
