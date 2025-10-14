using MinimalXmlReader;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ManiaAPI.Xml;

internal static partial class XmlHelper
{
    [GeneratedRegex(@"execution time\s*:\s*(\d+\.\d+)\s*s")]
    private static partial Regex ExecutionTimeRegex();

    public static async Task<XmlResponse> SendAsync(HttpClient client, string gameXml, string? authorXml, string requestName, string parametersXml, CancellationToken cancellationToken)
    {
        var formedXml = $"<root><game>{gameXml}</game>{authorXml}<request><name>{requestName}</name><params>{parametersXml}</params></request></root>";

        Debug.WriteLine(formedXml);

        using var content = new StringContent(formedXml, Encoding.UTF8, "text/xml");

        var startTime = Stopwatch.GetTimestamp();

        var response = await client.PostAsync(default(Uri), content, cancellationToken);

        var requestTime = Stopwatch.GetElapsedTime(startTime);

        response.EnsureSuccessStatusCode();

        return new XmlResponse(await response.Content.ReadAsStringAsync(cancellationToken), new XmlResponseDetails(requestTime));
    }

    public static MasterServerResponse<T> ProcessResponseResult<T>(
        string requestName,
        XmlResponse response, 
        XmlProcessContent<T> processContent) where T : notnull
    {
        Debug.WriteLine(response.Content);

        var startTime = Stopwatch.GetTimestamp();

        var xml = new MiniXmlReader(response.Content);

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
                    ProcessResponse(requestName, processContent, ref xml, startTime, response.Details, out content);
                    break;
                case "e":
                    executionTime = xml.ReadContentAsString();
                    break;
            }

            _ = xml.SkipEndElement();
        }

        var xmlParseTime = Stopwatch.GetElapsedTime(startTime);

        var executionTimeGroup = ExecutionTimeRegex().Match(executionTime).Groups[1].Value;

        var executionTimeSpan = string.IsNullOrEmpty(executionTimeGroup)
            ? default(TimeSpan?)
            : TimeSpan.FromSeconds(double.Parse(executionTimeGroup, CultureInfo.InvariantCulture));

        return new MasterServerResponse<T>(content ?? throw new Exception("No response content"), executionTimeSpan, xmlParseTime, response.Details);
    }

    private static void ProcessResponse<T>(
        string requestName, 
        XmlProcessContent<T> processContent, 
        ref MiniXmlReader xml, 
        long startTime,
        XmlResponseDetails details,
        out T? content) where T : notnull
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
                    ProcessErrorAndThrowException(ref xml, startTime, details);
                    return;
            }

            _ = xml.SkipEndElement(); // n, c or e
        }
    }

    private static void ProcessErrorAndThrowException(ref MiniXmlReader xml, long startTime, XmlResponseDetails details)
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

            _ = xml.SkipEndElement();
        }

        throw new XmlRequestException(value, message, Stopwatch.GetElapsedTime(startTime), details);
    }

    public static RecordUnit<T> ReadRecordUnit<T>(ref MiniXmlReader xml) where T : struct
    {
        var score = 0u;
        var count = 0;

        while (xml.TryReadStartElement(out var element))
        {
            switch (element)
            {
                case "s":
                    score = uint.Parse(xml.ReadContent());
                    break;
                case "c":
                    count = int.Parse(xml.ReadContent());
                    break;
                default:
                    xml.ReadContent();
                    break;
            }

            _ = xml.SkipEndElement();
        }

        ref T scoreValue = ref Unsafe.As<uint, T>(ref score);

        return new RecordUnit<T>(scoreValue, count);
    }
}
