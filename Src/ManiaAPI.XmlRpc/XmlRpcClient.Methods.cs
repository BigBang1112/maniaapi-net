namespace ManiaAPI.XmlRpc;

public partial class XmlRpcClient
{
    public async Task<IEnumerable<string>> SystemListMethodsAsync(CancellationToken cancellationToken = default)
    {
        var methods = await CallAsync<IEnumerable<object>>("system.listMethods", cancellationToken);

        return methods.OfType<string>();
    }

    public async Task<IEnumerable<IEnumerable<string>>> SystemMethodSignatureAsync(string methodName, CancellationToken cancellationToken = default)
    {
        var signatures = await CallAsync<IEnumerable<object>>("system.methodSignature", [methodName], cancellationToken);

        return signatures
            .OfType<IEnumerable<object>>()
            .Select(signature => signature.OfType<string>());
    }

    public async Task<string> SystemMethodHelpAsync(string methodName, CancellationToken cancellationToken = default)
    {
        return await CallAsync<string>("system.methodHelp", [methodName], cancellationToken);
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

        var results = await CallAsync<IEnumerable<object>>("system.multicall", [multicallParams], cancellationToken);

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
