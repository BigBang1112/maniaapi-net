using MinimalXmlReader;
using System.Diagnostics;
using System.Text;

namespace ManiaAPI.XmlRpc.MP4;

public class MasterServerMP4 : MasterServer
{
    protected async Task<string> SendAsync(string titleId, string requestName, string parameters, CancellationToken cancellationToken)
    {
        var content = new StringContent(@$"
<root>
    <game>
        <version>3.3.0</version>
        <build>2019-11-19_18_50</build>
        <title>{titleId}</title>
    </game>
    <request>
        <name>{requestName}</name>
        <params>{parameters}</params>
    </request>
</root>", Encoding.UTF8, "text/xml");

        using var response = await Client.PostAsync("http://relay02.v04.maniaplanet.com/game/request.php", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeaderboardItem>> GetCampaignLeaderBoardAsync(
        string titleId,
        int count = 10,
        int offset = 0,
        string zone = "World",
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetCampaignLeaderBoard";
        var responseStr = await SendAsync(titleId, RequestName, @$"
            <f>{offset}</f>
            <n>{count}</n>
            <c>{titleId}</c>
            <m></m>
            <t></t>
            <z>{zone}</z>
            <s>SkillPoint</s>", cancellationToken);
        var xml = new MiniXmlReader(responseStr);
        xml.SkipProcessingInstruction();
        if (!MemoryExtensions.Equals(xml.ReadStartElement(), "r", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("<r> (request) not found");
        }

        var items = new List<LeaderboardItem>();

        while (xml.TryReadStartElement(out var responseElement))
        {
            switch (responseElement)
            {
                case "r":
                    while (xml.TryReadStartElement(out var contentElement))
                    {
                        switch (contentElement)
                        {
                            case "n":
                                var requestName = xml.ReadContentAsString();
                                if (requestName != RequestName)
                                {
                                    //throw new Exception("Invalid response"); invalid xml will respond with empty request name
                                }
                                break;
                            case "c":
                                while (xml.TryReadStartElement("i"))
                                {
                                    var rank = 0;
                                    var login = string.Empty;
                                    var nickname = string.Empty;
                                    var score = 0u;
                                    var fileName = string.Empty;
                                    var downloadUrl = string.Empty;

                                    while (xml.TryReadStartElement(out var itemElement))
                                    {
                                        switch (itemElement)
                                        {
                                            case "r":
                                                rank = int.Parse(xml.ReadContent());
                                                break;
                                            case "l":
                                                login = xml.ReadContentAsString();
                                                break;
                                            case "n":
                                                nickname = xml.ReadContentAsString();
                                                break;
                                            case "s":
                                                score = uint.Parse(xml.ReadContent());
                                                break;
                                            case "f":
                                                fileName = xml.ReadContentAsString();
                                                break;
                                            case "u":
                                                downloadUrl = xml.ReadContentAsString();
                                                break;
                                            default:
                                                xml.ReadContent();
                                                break;
                                        }

                                        Debug.Assert(xml.SkipEndElement());
                                    }

                                    items.Add(new LeaderboardItem(rank, login, nickname, score, fileName, downloadUrl));

                                    Debug.Assert(xml.SkipEndElement()); // i
                                }
                                break;
                            case "e":
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

                        Debug.Assert(xml.SkipEndElement()); // n, c or e
                    }

                    break;
                case "e":
                    var executionTime = xml.ReadContentAsString();
                    break;
            }

            Debug.Assert(xml.SkipEndElement());
        }

        return items;
    }

    public async Task<IEnumerable<LeaderboardItem>> GetMapLeaderBoardAsync(
        string titleId, 
        string mapUid, 
        int count = 10, 
        int offset = 0, 
        string zone = "World",
        string context = "",
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetMapLeaderBoard";

        var responseStr = await SendAsync(titleId, RequestName, @$"
            <m>{mapUid}</m>
            <n>{count}</n>
            <f>{offset}</f>
            <z>{zone}</z>
            <c></c>
            <t></t>
            <s>MapRecord</s>", cancellationToken);

        var xml = new MiniXmlReader(responseStr);
        xml.SkipProcessingInstruction();

        var requestElement = xml.ReadStartElement();
        var responseElement = xml.ReadStartElement();
        var requestNameElement = xml.ReadStartElement();
        var requestName = xml.ReadContentAsString();

        if (requestName != RequestName)
        {
            throw new Exception("Invalid response");
        }

        xml.SkipEndElement();
        var contentElement = xml.ReadStartElement();

        var items = new List<LeaderboardItem>();

        while (xml.TryReadStartElement("i"))
        {
            var rankElement = xml.ReadStartElement();
            var rank = int.Parse(xml.ReadContent());
            xml.SkipEndElement();

            var loginElement = xml.ReadStartElement();
            var login = xml.ReadContentAsString();
            xml.SkipEndElement();

            var nicknameElement = xml.ReadStartElement();
            var nickname = xml.ReadContentAsString();
            xml.SkipEndElement();

            var scoreElement = xml.ReadStartElement();
            var score = uint.Parse(xml.ReadContent());
            xml.SkipEndElement();

            var fileNameElement = xml.ReadStartElement();
            var fileName = xml.ReadContentAsString();
            xml.SkipEndElement();

            var downloadUrlElement = xml.ReadStartElement();
            var downloadUrl = xml.ReadContentAsString();
            xml.SkipEndElement();

            items.Add(new LeaderboardItem(rank, login, nickname, score, fileName, downloadUrl));

            xml.SkipEndElement();
        }

        return items;
    }
}
