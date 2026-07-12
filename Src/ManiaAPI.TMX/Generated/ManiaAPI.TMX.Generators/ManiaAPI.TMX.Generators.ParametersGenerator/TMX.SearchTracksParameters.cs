using System.Runtime.InteropServices;
using System.Text;
using System.Net;

namespace ManiaAPI.TMX;

public partial class TMX
{
    /// <remarks>Be careful using <c>default</c> value. It also disables all fields.</remarks>
    [StructLayout(LayoutKind.Auto)]
    public readonly partial record struct SearchTracksParameters
    {
        public global::ManiaAPI.TMX.TrackItemFields Fields { get; init; }

        public SearchTracksParameters()
        {
            Fields = global::ManiaAPI.TMX.TrackItemFields.All;
        }

        internal bool AppendQueryString(StringBuilder sb, bool appendFields = true)
        {
            var first = true;

            if (Order1.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("order1=");
                sb.Append((int)Order1.Value);
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

            if (UId is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("uid=");
                sb.Append(string.Join("%2C", UId));
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

            if (!string.IsNullOrEmpty(Author))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("author=");
                sb.Append(WebUtility.UrlEncode(Author));
                first = false;
            }

            if (AuthorUserId.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("authoruserid=");
                sb.Append(AuthorUserId.Value);
                first = false;
            }

            if (VideoId.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("videoid=");
                sb.Append(VideoId.Value);
                first = false;
            }

            if (PackId.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("packid=");
                sb.Append(PackId.Value);
                first = false;
            }

            if (!string.IsNullOrEmpty(AwardedBy))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("awardedby=");
                sb.Append(WebUtility.UrlEncode(AwardedBy));
                first = false;
            }

            if (!string.IsNullOrEmpty(CommentedBy))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("commentedby=");
                sb.Append(WebUtility.UrlEncode(CommentedBy));
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
                sb.Append(TagInclusive.Value);
                first = false;
            }

            if (PrimaryType.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("primarytype=");
                sb.Append((int)PrimaryType.Value);
                first = false;
            }

            if (LbType.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("lbtype=");
                sb.Append((int)LbType.Value);
                first = false;
            }

            if (UploadedAfter.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("uploadedafter=");
                sb.Append(UploadedAfter.Value);
                first = false;
            }

            if (UploadedBefore.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("uploadedbefore=");
                sb.Append(UploadedBefore.Value);
                first = false;
            }

            if (AuthorTimeMin.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("authortimemin=");
                sb.Append(AuthorTimeMin.Value);
                first = false;
            }

            if (AuthorTimeMax.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("authortimemax=");
                sb.Append(AuthorTimeMax.Value);
                first = false;
            }

            if (Environment is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("environment=");
                sb.Append(string.Join("%2C", Environment));
                first = false;
            }

            if (Vehicle is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("vehicle=");
                sb.Append(string.Join("%2C", Vehicle));
                first = false;
            }

            if (Mood is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("mood=");
                sb.Append(string.Join("%2C", Mood));
                first = false;
            }

            if (Difficulty is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("difficulty=");
                sb.Append(string.Join("%2C", Difficulty));
                first = false;
            }

            if (Routes is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("routes=");
                sb.Append(string.Join("%2C", Routes));
                first = false;
            }

            if (AntiSpam.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("antispam=");
                sb.Append(AntiSpam.Value);
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

            if (InLatestAuthor.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inlatestauthor=");
                sb.Append(InLatestAuthor.Value ? '1' : '0');
                first = false;
            }

            if (InLatestAwardedAuthor.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inlatestawardedauthor=");
                sb.Append(InLatestAwardedAuthor.Value ? '1' : '0');
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

            if (InHasRecord.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inhasrecord=");
                sb.Append(InHasRecord.Value ? '1' : '0');
                first = false;
            }

            if (InEnvMix.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inenvmix=");
                sb.Append(InEnvMix.Value ? '1' : '0');
                first = false;
            }

            if (InUnlimiter.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inunlimiter=");
                sb.Append(InUnlimiter.Value ? '1' : '0');
                first = false;
            }

            if (Order2.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("order2=");
                sb.Append((int)Order2.Value);
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
