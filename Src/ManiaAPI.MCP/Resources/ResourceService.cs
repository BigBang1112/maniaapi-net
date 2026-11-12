using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Tools;
using ModelContextProtocol.Protocol;

namespace ManiaAPI.MCP.Resources;

public sealed class ResourceService(OperationEngine engine, GatewayTools gateway)
{
    public ListResourceTemplatesResult List()
    {
        var scope = gateway.Scope();
        var templates = engine.Available(scope)
            .Where(operation => operation.ReadOnly && operation.ReturnsBinary)
            .Select(operation => new ResourceTemplate
            {
                Name = operation.Id,
                Description = operation.Summary + " Allowed exchange sites: " + string.Join(", ", scope.Sites(operation)),
                UriTemplate = "maniaapi://operation/" + operation.Id + "{?arguments}"
            })
            .ToList();

        return new ListResourceTemplatesResult
        {
            ResourceTemplates = templates,
            TimeToLive = TimeSpan.FromHours(24),
            CacheScope = CacheScope.Public
        };
    }

    public async ValueTask<ReadResourceResult> ReadAsync(string uri, IServiceProvider services, CancellationToken cancellationToken)
    {
        var (operation, arguments) = ResourceUri.Parse(uri);
        var capture = new BinaryCapture();

        await engine.InvokeAsync(operation, arguments, gateway.Scope(), services,
            cancellationToken: cancellationToken, capture: capture);

        if (capture.Bytes is null)
        {
            throw new ArgumentException("This operation does not return a downloadable resource.");
        }

        return new ReadResourceResult
        {
            Contents =
            [
                BlobResourceContents.FromBytes(capture.Bytes, uri, capture.ContentType)
            ],
            TimeToLive = TimeSpan.FromMinutes(5),
            CacheScope = CacheScope.Private
        };
    }
}
