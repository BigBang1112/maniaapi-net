using System.Runtime.InteropServices;
using System.Text;
using System.Net;

namespace ManiaAPI.TMX;

public partial class MX
{
    /// <remarks>Be careful using <c>default</c> value. It also disables all fields.</remarks>
    [StructLayout(LayoutKind.Auto)]
    public readonly partial record struct SearchReplaysParameters
    {
        public global::ManiaAPI.TMX.ReplayItemMXFields Fields { get; init; }

        public SearchReplaysParameters()
        {
            Fields = global::ManiaAPI.TMX.ReplayItemMXFields.All;
        }

        internal bool AppendQueryString(StringBuilder sb, bool appendFields = true)
        {
            var first = true;

            if (first) sb.Append('?');
            else sb.Append('&');
            sb.Append("mapid=");
            sb.Append(MapId);
            first = false;

            if (Count.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("count=");
                sb.Append(Count.Value);
                first = false;
            }

            if (UserId.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("userid=");
                sb.Append(UserId.Value);
                first = false;
            }

            if (Best.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("best=");
                sb.Append(Best.Value ? '1' : '0');
                first = false;
            }

            if (appendFields)
            {
                if (first) sb.Append("?fields=");
                else sb.Append("&fields=");
                Fields.Append(sb);
                first = false;
            }

            return !first;
        }
    }
}
