using ManiaAPI.GBXRemote;
using Microsoft.Extensions.Logging;

var logger = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
    });
    builder.SetMinimumLevel(LogLevel.Warning);
}).CreateLogger<Client>();

using var client = await Client.ConnectAsync("127.0.0.1", logger: logger);

var methods = await client.SystemListMethodsAsync();

// Note: this is a rather slow approach to achieve the goal
// for real scenarios, you would want to call SystemMulticallAsync
// with system.methodSignature and system.methodHelp for all methods listed
foreach (var method in methods)
{
    var signatures = await client.SystemMethodSignatureAsync(method);

    foreach (var signature in signatures)
    {
        Console.WriteLine($"{signature.First()} {method}({string.Join(", ", signature.Skip(1))})");
    }

    Console.WriteLine(await client.SystemMethodHelpAsync(method));

    Console.WriteLine();
}