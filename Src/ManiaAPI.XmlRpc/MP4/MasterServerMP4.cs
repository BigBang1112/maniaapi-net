using MinimalXmlReader;
using System.Text;

namespace ManiaAPI.XmlRpc.MP4;

public class MasterServerMP4 : MasterServer
{
    protected async Task<string> SendAsync(StringContent content, CancellationToken cancellationToken)
    {
        using var response = await Client.PostAsync("http://relay02.v04.maniaplanet.com/game/request.php", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeaderboardItem>> GetMapLeaderBoardAsync(string titleId, string mapUid, int count = 10, int offset = 0, string zone = "World", string context = "", CancellationToken cancellationToken = default)
    {
        var content = new StringContent(@$"
<root>
    <game>
        <version>3.3.0</version>
        <build>2019-11-19_18_50</build>
        <title>{titleId}</title>
    </game>
    <request>
        <name>GetMapLeaderBoard</name>
        <params>
            <m>{mapUid}</m>
            <n>{count}</n>
            <f>{offset}</f>
            <z>{zone}</z>
            <c></c>
            <t></t>
            <s>MapRecord</s>
        </params>
    </request>
</root>", Encoding.UTF8, "text/xml");
        var responseStr = await SendAsync(content, cancellationToken);
        var xml = new MiniXmlReader(responseStr);
        xml.SkipProcessingInstruction();
        var requestElement = xml.ReadStartElement();
        var responseElement = xml.ReadStartElement();
        var requestNameElement = xml.ReadStartElement();
        var requestName = xml.ReadContentAsString();

        if (requestName != "GetMapLeaderBoard")
        {
            throw new Exception("Invalid response");
        }

        xml.SkipEndElement();
        var contentElement = xml.ReadStartElement();

        var items = new List<LeaderboardItem>();

        while (xml.ReadStartElement("i", out _))
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
