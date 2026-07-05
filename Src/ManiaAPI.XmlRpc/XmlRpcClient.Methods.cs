namespace ManiaAPI.XmlRpc;

public partial class XmlRpcClient
{
    public async Task<IEnumerable<string>> SystemListMethodsAsync(CancellationToken cancellationToken = default)
    {
        var result = await CallAsync("system.listMethods", cancellationToken);

        if (result is not IEnumerable<object> methods)
        {
            throw new XmlRpcClientException("Invalid response from system.listMethods.");
        }

        return methods.OfType<string>();
    }

    public async Task<IEnumerable<IEnumerable<string>>> SystemMethodSignatureAsync(string methodName, CancellationToken cancellationToken = default)
    {
        var result = await CallAsync("system.methodSignature", [methodName], cancellationToken);

        if (result is not IEnumerable<object> signatures)
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

        if (result is not string help)
        {
            throw new XmlRpcClientException($"Invalid response from system.methodHelp for method '{methodName}'.");
        }

        return help;
    }

    public async Task<IEnumerable<XmlRpcMulticallResult>> SystemMulticallAsync(IEnumerable<XmlRpcMulticall> calls, CancellationToken cancellationToken = default)
    {
        var multicallParams = calls
            .Select(call => new Dictionary<string, object?>
            {
                ["methodName"] = call.MethodName,
                ["params"] = call.Parameters
            })
            .ToArray();

        var result = await CallAsync("system.multicall", [multicallParams], cancellationToken);

        if (result is not IEnumerable<object> results)
        {
            throw new XmlRpcClientException("Invalid response from system.multicall.");
        }

        return results.Select(ParseMulticallResult);
    }

    private static XmlRpcMulticallResult ParseMulticallResult(object item)
    {
        if (item is IEnumerable<object?> success)
        {
            return new XmlRpcMulticallResult(success.FirstOrDefault(), FaultCode: null, FaultString: null);
        }

        if (item is IDictionary<string, object?> fault)
        {
            return new XmlRpcMulticallResult(
                Value: null,
                FaultCode: fault.TryGetValue("faultCode", out var code) ? code as int? : null,
                FaultString: fault.TryGetValue("faultString", out var faultString) ? faultString as string : null);
        }

        throw new XmlRpcClientException("Invalid item in system.multicall response.");
    }
}
