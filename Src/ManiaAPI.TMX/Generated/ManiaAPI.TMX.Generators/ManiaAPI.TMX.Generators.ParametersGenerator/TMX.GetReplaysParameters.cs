using System.Runtime.InteropServices;
using System.Text;
using System.Net;

namespace ManiaAPI.TMX;

public partial class TMX
{
    /// <remarks>Be careful using <c>default</c> value. It also disables all fields.</remarks>
    [StructLayout(LayoutKind.Auto)]
    public readonly partial record struct GetReplaysParameters
    {
        public global::ManiaAPI.TMX.ReplayItemFields Fields { get; init; }

        public GetReplaysParameters()
        {
            Fields = global::ManiaAPI.TMX.ReplayItemFields.All;
        }

        internal bool AppendQueryString(StringBuilder sb, bool appendFields = true)
        {
            var first = true;

            if (first) sb.Append('?');
            else sb.Append('&');
            sb.Append("trackid=");
            sb.Append(TrackId);
            first = false;

            if (Count.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("count=");
                sb.Append(Count.Value);
                first = false;
            }

            if (After.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("after=");
                sb.Append(After.Value);
                first = false;
            }

            if (Before.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("before=");
                sb.Append(Before.Value);
                first = false;
            }

            if (From.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("from=");
                sb.Append(From.Value);
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

            if (UserId.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("userid=");
                sb.Append(UserId.Value);
                first = false;
            }

            if (AdditionalParameters is not null)
            {
                foreach (var additionalParameter in AdditionalParameters)
                {
                    if (first) sb.Append('?');
                    else sb.Append('&');
                    sb.Append(additionalParameter.Key);
                    sb.Append('=');
                    sb.Append(WebUtility.UrlEncode(additionalParameter.Value));
                    first = false;
                }
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
