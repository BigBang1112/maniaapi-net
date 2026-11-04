using System.Globalization;

namespace ManiaAPI.GBXRemote.Tests;

public class GenerateXmlPayloadTests
{
    private enum Mode { Script = 0, Rounds = 1, TimeAttack = 2 }

    private static string Value(object value)
    {
        var xml = GBXRemoteClient.GenerateXmlPayload("Method", [value]);
        const string prefix = """<?xml version="1.0"?><methodCall><methodName>Method</methodName><params><param>""";
        const string suffix = "</param></params></methodCall>";
        return xml[prefix.Length..^suffix.Length];
    }

    [Test]
    public async Task NoParameters_ProducesEmptyParams()
    {
        var xml = GBXRemoteClient.GenerateXmlPayload("GetVersion", []);

        await Assert.That(xml).IsEqualTo("""<?xml version="1.0"?><methodCall><methodName>GetVersion</methodName><params></params></methodCall>""");
    }

    [Test]
    public async Task MethodName_IsEscaped()
    {
        var xml = GBXRemoteClient.GenerateXmlPayload("A<B>", []);

        await Assert.That(xml).Contains("<methodName>A&lt;B&gt;</methodName>");
    }

    [Test]
    [MethodDataSource(nameof(ScalarCases))]
    public async Task Scalars_UseXmlRpcTypes(object value, string expectedXml)
    {
        await Assert.That(Value(value)).IsEqualTo(expectedXml);
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
    public async Task Double_IgnoresCurrentCulture()
    {
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("cs-CZ");

        try
        {
            await Assert.That(Value(0.5)).IsEqualTo("<value><double>0.5</double></value>");
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Test]
    public async Task Dictionary_BecomesStruct()
    {
        var value = new Dictionary<string, object> { ["Name"] = "a&b", ["Count"] = 1 };

        await Assert.That(Value(value)).IsEqualTo(
            "<value><struct><member><name>Name</name><value>a&amp;b</value></member><member><name>Count</name><value><int>1</int></value></member></struct></value>");
    }

    [Test]
    public async Task Enumerable_BecomesArray()
    {
        await Assert.That(Value(new object[] { 1, "x", true })).IsEqualTo(
            "<value><array><data><value><int>1</int></value><value>x</value><value><boolean>1</boolean></value></data></array></value>");
    }
}
