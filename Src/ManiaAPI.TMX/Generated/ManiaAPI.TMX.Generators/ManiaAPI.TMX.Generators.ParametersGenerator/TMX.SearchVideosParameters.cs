using System.Runtime.InteropServices;
using System.Text;
using System.Net;

namespace ManiaAPI.TMX;

public partial class TMX
{
    /// <remarks>Be careful using <c>default</c> value. It also disables all fields.</remarks>
    [StructLayout(LayoutKind.Auto)]
    public readonly partial record struct SearchVideosParameters
    {
        public global::ManiaAPI.TMX.VideoItemFields Fields { get; init; }

        public SearchVideosParameters()
        {
            Fields = global::ManiaAPI.TMX.VideoItemFields.All with { MapCount = default };
        }

        internal bool AppendQueryString(StringBuilder sb, bool appendFields = true)
        {
            var first = true;

            if (Order1.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("order1=");
                sb.Append(Order1.Value);
                first = false;
            }

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

            if (Id is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("id=");
                sb.Append(string.Join("%2C", Id));
                first = false;
            }

            if (NId is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("nid=");
                sb.Append(string.Join("%2C", NId));
                first = false;
            }

            if (TrackId is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("trackid=");
                sb.Append(string.Join("%2C", TrackId));
                first = false;
            }

            if (!string.IsNullOrEmpty(Title))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("title=");
                sb.Append(WebUtility.UrlEncode(Title));
                first = false;
            }

            if (!string.IsNullOrEmpty(Author))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("author=");
                sb.Append(WebUtility.UrlEncode(Author));
                first = false;
            }

            if (!string.IsNullOrEmpty(Poster))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("poster=");
                sb.Append(WebUtility.UrlEncode(Poster));
                first = false;
            }

            if (!string.IsNullOrEmpty(Creator))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("creator=");
                sb.Append(WebUtility.UrlEncode(Creator));
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
