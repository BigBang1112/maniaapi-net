namespace ManiaAPI.XmlRpc;

public partial class XmlRpcClient
{
    public async Task<IEnumerable<string>> SystemListMethodsAsync(CancellationToken cancellationToken = default)
    {
        var result = await CallAsync("system.listMethods", cancellationToken);

        if (result is not [IEnumerable<object> methods])
        {
            throw new XmlRpcClientException("Invalid response from system.listMethods.");
        }

        return methods.OfType<string>();
    }

    public async Task<IEnumerable<IEnumerable<string>>> SystemMethodSignatureAsync(string methodName, CancellationToken cancellationToken = default)
    {
        var result = await CallAsync("system.methodSignature", [methodName], cancellationToken);

        if (result is not [IEnumerable<object> signatures])
        {
            throw new XmlRpcClientException($"Invalid response from system.methodSignature for method '{methodName}'.");
        }

        return signatures
            .OfType<IEnumerable<object>>()
            .Select(signature => signature.OfType<string>());
    }

    public async Task<string> SystemMethodHelpAsync(string methodName, CancellationToken cancellationToken = default)
    {
        var result = await CallAsync("system.methodHelp", [methodName], cancellationToken);

        if (result is not [string help])
        {
            throw new XmlRpcClientException($"Invalid response from system.methodHelp for method '{methodName}'.");
        }

        return help;
    }
}
