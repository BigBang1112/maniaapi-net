using System.Runtime.InteropServices;
using System.Text;
using System.Net;

namespace ManiaAPI.TMX;

public partial class MX
{
    /// <remarks>Be careful using <c>default</c> value. It also disables all fields.</remarks>
    [StructLayout(LayoutKind.Auto)]
    public readonly partial record struct SearchMappacksParameters
    {
        public global::ManiaAPI.TMX.MappackItemFields Fields { get; init; }

        public SearchMappacksParameters()
        {
            Fields = global::ManiaAPI.TMX.MappackItemFields.All;
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

            if (Order2.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("order2=");
                sb.Append(Order2.Value);
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

            if (!string.IsNullOrEmpty(Name))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("name=");
                sb.Append(WebUtility.UrlEncode(Name));
                first = false;
            }

            if (!string.IsNullOrEmpty(Manager))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("manager=");
                sb.Append(WebUtility.UrlEncode(Manager));
                first = false;
            }

            if (ManagerUserId.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("manageruserid=");
                sb.Append(ManagerUserId.Value);
                first = false;
            }

            if (MapId.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("mapid=");
                sb.Append(MapId.Value);
                first = false;
            }

            if (Tag is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("tag=");
                sb.Append(string.Join("%2C", Tag));
                first = false;
            }

            if (ETag is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("etag=");
                sb.Append(string.Join("%2C", ETag));
                first = false;
            }

            if (TagInclusive.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("taginclusive=");
                sb.Append(TagInclusive.Value ? '1' : '0');
                first = false;
            }

            if (CreatedAfter.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("createdafter=");
                sb.Append(CreatedAfter.Value);
                first = false;
            }

            if (CreatedBefore.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("createdbefore=");
                sb.Append(CreatedBefore.Value);
                first = false;
            }

            if (PrimaryType.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("primarytype=");
                sb.Append(PrimaryType.Value);
                first = false;
            }

            if (InLatestAuthor.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inlatestauthor=");
                sb.Append(InLatestAuthor.Value ? '1' : '0');
                first = false;
            }

            if (InScreenshot.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inscreenshot=");
                sb.Append(InScreenshot.Value ? '1' : '0');
                first = false;
            }

            if (InSupporter.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("insupporter=");
                sb.Append(InSupporter.Value ? '1' : '0');
                first = false;
            }

            if (InFavorite.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("infavorite=");
                sb.Append(InFavorite.Value ? '1' : '0');
                first = false;
            }

            if (InFeatured.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("infeatured=");
                sb.Append(InFeatured.Value ? '1' : '0');
                first = false;
            }

            if (InDownloads.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("indownloads=");
                sb.Append(InDownloads.Value ? '1' : '0');
                first = false;
            }

            if (InHasRecords.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inhasrecords=");
                sb.Append(InHasRecords.Value ? '1' : '0');
                first = false;
            }

            if (InHasRecord.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inhasrecord=");
                sb.Append(InHasRecord.Value ? '1' : '0');
                first = false;
            }

            if (LbBy.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("lbby=");
                sb.Append(LbBy.Value);
                first = false;
            }

            if (!string.IsNullOrEmpty(MappackSecret))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("mappacksecret=");
                sb.Append(WebUtility.UrlEncode(MappackSecret));
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
