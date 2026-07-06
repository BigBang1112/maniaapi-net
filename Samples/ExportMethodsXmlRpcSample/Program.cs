using ManiaAPI.XmlRpc;
using Microsoft.Extensions.Logging;

var logger = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
    });
    builder.SetMinimumLevel(LogLevel.Warning);
}).CreateLogger<XmlRpcClient>();

using var xmlRpc = await XmlRpcClient.ConnectAsync("127.0.0.1", logger: logger);

var methods = await xmlRpc.SystemListMethodsAsync();

// Note: this is a rather slow approach to achieve the goal
// for real scenarios, you would want to call SystemMulticallAsync
// with system.methodSignature and system.methodHelp for all methods listed
foreach (var method in methods)
{
    var signatures = await xmlRpc.SystemMethodSignatureAsync(method);

    foreach (var signature in signatures)
    {
        Console.WriteLine($"{signature.First()} {method}({string.Join(", ", signature.Skip(1))})");
    }

    Console.WriteLine(await xmlRpc.SystemMethodHelpAsync(method));

    Console.WriteLine();
}