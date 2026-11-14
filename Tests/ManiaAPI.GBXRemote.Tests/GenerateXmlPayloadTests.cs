using System.Globalization;

namespace ManiaAPI.GBXRemote.Tests;

public class GenerateXmlPayloadTests
{
    private enum Mode { Script = 0, Rounds = 1, TimeAttack = 2 }
    private enum Direction { Backward = -1, Forward = 1 }

    [Test]
    public async Task GenerateXmlPayload_NoParameters_ProducesEmptyParams()
    {
        var xml = GBXRemoteClient.GenerateXmlPayload("GetVersion", []);

        await Assert.That(xml).IsEqualTo("""<?xml version="1.0"?><methodCall><methodName>GetVersion</methodName><params></params></methodCall>""");
    }

    [Test]
    public async Task GenerateXmlPayload_SpecialCharactersInMethodName_EscapesThem()
    {
        var xml = GBXRemoteClient.GenerateXmlPayload("A<B>", []);

        await Assert.That(xml).Contains("<methodName>A&lt;B&gt;</methodName>");
    }

    [Test]
    [MethodDataSource(nameof(ScalarCases))]
    public async Task GenerateXmlPayload_Scalar_UsesXmlRpcType(object value, string expectedXml)
    {
        var xml = ValueXml(value);

        await Assert.That(xml).IsEqualTo(expectedXml);
    }

    public static IEnumerable<Func<(object, string)>> ScalarCases()
    {
        yield return () => (42, "<value><int>42</int></value>");
        yield return () => ((byte)7, "<value><int>7</int></value>");
        yield return () => (9_000_000_000L, "<value><i8>9000000000</i8></value>");
        yield return () => (true, "<value><boolean>1</boolean></value>");
        yield return () => (false, "<value><boolean>0</boolean></value>");
        yield return () => (0.5, "<value><double>0.5</double></value>");
        yield return () => (1.25f, "<value><double>1.25</double></value>");
        yield return () => (Mode.TimeAttack, "<value><int>2</int></value>");
        yield return () => (new byte[] { 1, 2, 3 }, "<value><base64>AQID</base64></value>");
        yield return () => ("Tom & <Jerry>", "<value>Tom &amp; &lt;Jerry&gt;</value>");
    }

    [Test]
    public async Task GenerateXmlPayload_DoubleInCommaDecimalCulture_UsesInvariantCulture()
    {
        var xml = WithCulture("cs-CZ", () => ValueXml(0.5));

        await Assert.That(xml).IsEqualTo("<value><double>0.5</double></value>");
    }

    [Test]
    [MethodDataSource(nameof(NegativeIntegerCases))]
    public async Task GenerateXmlPayload_NegativeIntegerInSwedishCulture_UsesAsciiMinus(object value, string expectedXml)
    {
        // Swedish formats the minus sign as U+2212, which the server doesn't accept
        var xml = WithCulture("sv-SE", () => ValueXml(value));

        await Assert.That(xml).IsEqualTo(expectedXml);
    }

    public static IEnumerable<Func<(object, string)>> NegativeIntegerCases()
    {
        yield return () => (-5, "<value><int>-5</int></value>");
        yield return () => ((short)-5, "<value><int>-5</int></value>");
        yield return () => (-9_000_000_000L, "<value><i8>-9000000000</i8></value>");
        yield return () => (Direction.Backward, "<value><int>-1</int></value>");
    }

    [Test]
    public async Task GenerateXmlPayload_Dictionary_ProducesStruct()
    {
        var value = new Dictionary<string, object> { ["Name"] = "a&b", ["Count"] = 1 };

        var xml = ValueXml(value);

        await Assert.That(xml).IsEqualTo(
            "<value><struct><member><name>Name</name><value>a&amp;b</value></member><member><name>Count</name><value><int>1</int></value></member></struct></value>");
    }

    [Test]
    public async Task GenerateXmlPayload_Enumerable_ProducesArray()
    {
        var xml = ValueXml(new object[] { 1, "x", true });

        await Assert.That(xml).IsEqualTo(
            "<value><array><data><value><int>1</int></value><value>x</value><value><boolean>1</boolean></value></data></array></value>");
    }

    /// <summary>
    /// Generates a payload with one parameter and returns only the XML of that parameter.
    /// </summary>
    private static string ValueXml(object value)
    {
        const string prefix = """<?xml version="1.0"?><methodCall><methodName>Method</methodName><params><param>""";
        const string suffix = "</param></params></methodCall>";

        var xml = GBXRemoteClient.GenerateXmlPayload("Method", [value]);
        return xml[prefix.Length..^suffix.Length];
    }

    private static T WithCulture<T>(string cultureName, Func<T> action)
    {
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

        try
        {
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }
}
