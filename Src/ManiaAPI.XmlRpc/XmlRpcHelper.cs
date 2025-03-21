using MinimalXmlReader;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ManiaAPI.XmlRpc;

internal static partial class XmlRpcHelper
{
    [GeneratedRegex(@"execution time\s*:\s*(\d+\.\d+)\s*s")]
    private static partial Regex ExecutionTimeRegex();

    internal static MasterServerResponse<T> ProcessResponseResult<T>(string requestName, string responseStr, XmlRpcProcessContent<T> processContent) where T : notnull
    {
        var xml = new MiniXmlReader(responseStr);

        xml.SkipProcessingInstruction();

        if (!MemoryExtensions.Equals(xml.ReadStartElement(), "r", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("<r> (first one) not found");
        }

        var content = default(T);
        var executionTime = string.Empty;

        while (xml.TryReadStartElement(out var responseElement))
        {
            switch (responseElement)
            {
                case "r":
                    ProcessResponse(requestName, processContent, ref xml, out content);
                    break;
                case "e":
                    executionTime = xml.ReadContentAsString();
                    break;
            }

            Debug.Assert(xml.SkipEndElement());
        }

        var executionTimeSpan = TimeSpan.FromSeconds(double.Parse(ExecutionTimeRegex().Match(executionTime).Groups[1].Value, CultureInfo.InvariantCulture));

        return new MasterServerResponse<T>(content ?? throw new Exception("No response content"), executionTimeSpan);
    }

    private static void ProcessResponse<T>(string requestName, XmlRpcProcessContent<T> processContent, ref MiniXmlReader xml, out T? content) where T : notnull
    {
        content = default;

        while (xml.TryReadStartElement(out var contentElement))
        {
            switch (contentElement)
            {
                case "n":
                    var actualRequestName = xml.ReadContentAsString();
                    if (requestName != actualRequestName)
                    {
                        //throw new Exception("Invalid response"); invalid xml will respond with empty request name
                    }
                    break;
                case "c":
                    content = processContent(ref xml);
                    break;
                case "e":
                    ProcessErrorAndThrowException(xml);
                    return;
            }

            Debug.Assert(xml.SkipEndElement()); // n, c or e
        }
    }

    private static void ProcessErrorAndThrowException(MiniXmlReader xml)
    {
        var value = 0;
        var message = string.Empty;

        while (xml.TryReadStartElement(out var errorElement))
        {
            switch (errorElement)
            {
                case "v":
                    value = int.Parse(xml.ReadContent());
                    break;
                case "m":
                    message = xml.ReadContentAsString();
                    break;
                default:
                    xml.ReadContent();
                    break;
            }

            Debug.Assert(xml.SkipEndElement());
        }

        throw new Exception($"Error {value}: {message}");
    }
}
