using ManiaAPI.GBXRemote;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Net.Sockets;

namespace ManiaAPI.XmlRpc;

/// <summary>
/// Compatibility alias for <see cref="GBXRemoteClient"/>.
/// </summary>
[Obsolete("Use ManiaAPI.GBXRemote.GBXRemoteClient instead.")]
public sealed class XmlRpcClient : GBXRemoteClient
{
    private XmlRpcClient(TcpClient tcp, int version, ILogger<GBXRemoteClient> logger)
        : base(tcp, version, logger)
    {
    }

    public new static async ValueTask<XmlRpcClient> ConnectAsync(
        string ip,
        int port = 5000,
        ILogger<GBXRemoteClient>? logger = null,
        CancellationToken cancellationToken = default)
    {
        var tcp = new TcpClient();
        await tcp.ConnectAsync(ip, port, cancellationToken);
        var version = await ValidateHeaderOrThrowAsync(tcp.GetStream(), cancellationToken);
        return new XmlRpcClient(tcp, version, logger ?? NullLogger<GBXRemoteClient>.Instance);
    }

    public new static async ValueTask<XmlRpcClient> ConnectAsync(
        IPAddress ip,
        int port = 5000,
        ILogger<GBXRemoteClient>? logger = null,
        CancellationToken cancellationToken = default)
    {
        var tcp = new TcpClient();
        await tcp.ConnectAsync(ip, port, cancellationToken);
        var version = await ValidateHeaderOrThrowAsync(tcp.GetStream(), cancellationToken);
        return new XmlRpcClient(tcp, version, logger ?? NullLogger<GBXRemoteClient>.Instance);
    }

    public new static async ValueTask<XmlRpcClient> ConnectAsync(
        IPEndPoint endpoint,
        ILogger<GBXRemoteClient>? logger = null,
        CancellationToken cancellationToken = default)
    {
        var tcp = new TcpClient();
        await tcp.ConnectAsync(endpoint, cancellationToken);
        var version = await ValidateHeaderOrThrowAsync(tcp.GetStream(), cancellationToken);
        return new XmlRpcClient(tcp, version, logger ?? NullLogger<GBXRemoteClient>.Instance);
    }
}
