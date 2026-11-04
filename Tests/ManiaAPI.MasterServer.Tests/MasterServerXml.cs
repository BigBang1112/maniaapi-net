namespace ManiaAPI.MasterServer.Tests;

/// <summary>
/// Builds responses in the XML format of the master servers.
/// </summary>
internal static class MasterServerXml
{
    public static string Success(string requestName, string content, string executionTime = "execution time : 0.0123 s")
    {
        return $"""<?xml version="1.0" encoding="UTF-8"?><r><r><n>{requestName}</n><c>{content}</c></r><e>{executionTime}</e></r>""";
    }

    public static string Error(string requestName, int value, string message)
    {
        return $"""<?xml version="1.0" encoding="UTF-8"?><r><r><n>{requestName}</n><e><v>{value}</v><m>{message}</m></e></r></r>""";
    }

    public static string Empty(string executionTime = "execution time : 0.0123 s")
    {
        return $"""<?xml version="1.0" encoding="UTF-8"?><r><e>{executionTime}</e></r>""";
    }

    public static FakeHttpMessageHandler Handler(string xml) => FakeHttpMessageHandler.Text(xml, "text/xml");
}
