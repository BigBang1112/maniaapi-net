using System.Runtime.InteropServices;
using System.Text;
using System.Net;

namespace ManiaAPI.TMX;

public partial class MX
{
    /// <remarks>Be careful using <c>default</c> value. It also disables all fields.</remarks>
    [StructLayout(LayoutKind.Auto)]
    public readonly partial record struct SearchMapsParameters
    {
        public global::ManiaAPI.TMX.MapItemFields Fields { get; init; }

        public SearchMapsParameters()
        {
            Fields = global::ManiaAPI.TMX.MapItemFields.All;
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

            if (Uid is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("uid=");
                sb.Append(string.Join("%2C", Uid));
                first = false;
            }

            if (Random.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("random=");
                sb.Append(Random.Value);
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

            if (MappackId.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("mappackid=");
                sb.Append(MappackId.Value);
                first = false;
            }

            if (Status is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("status=");
                sb.Append(string.Join("%2C", Status));
                first = false;
            }

            if (AwardedBy.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("awardedby=");
                sb.Append(AwardedBy.Value);
                first = false;
            }

            if (CommentedBy.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("commentedby=");
                sb.Append(CommentedBy.Value);
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

            if (PrimaryType.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("primarytype=");
                sb.Append(PrimaryType.Value);
                first = false;
            }

            if (!string.IsNullOrEmpty(MapType))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("maptype=");
                sb.Append(WebUtility.UrlEncode(MapType));
                first = false;
            }

            if (LbType.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("lbtype=");
                sb.Append(LbType.Value);
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

            if (!string.IsNullOrEmpty(TitlePack))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("titlepack=");
                sb.Append(WebUtility.UrlEncode(TitlePack));
                first = false;
            }

            if (!string.IsNullOrEmpty(CustomMapType))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("custommaptype=");
                sb.Append(WebUtility.UrlEncode(CustomMapType));
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

            if (LengthMin.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("lengthmin=");
                sb.Append(LengthMin.Value);
                first = false;
            }

            if (LengthMax.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("lengthmax=");
                sb.Append(LengthMax.Value);
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

            if (ExEnvironment is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("exenvironment=");
                sb.Append(string.Join("%2C", ExEnvironment));
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

            if (ExVehicle is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("exvehicle=");
                sb.Append(string.Join("%2C", ExVehicle));
                first = false;
            }

            if (!string.IsNullOrEmpty(Mod))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("mod=");
                sb.Append(WebUtility.UrlEncode(Mod));
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

            if (InBeta.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inbeta=");
                sb.Append(InBeta.Value ? '1' : '0');
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

            if (InPlayLater.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inplaylater=");
                sb.Append(InPlayLater.Value ? '1' : '0');
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

            if (InDownloads.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("indownloads=");
                sb.Append(InDownloads.Value ? '1' : '0');
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

            if (InReplays.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inreplays=");
                sb.Append(InReplays.Value ? '1' : '0');
                first = false;
            }

            if (InOnlineRecords.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inonlinerecords=");
                sb.Append(InOnlineRecords.Value ? '1' : '0');
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

            if (InHasReplay.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inhasreplay=");
                sb.Append(InHasReplay.Value ? '1' : '0');
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

            if (InTotd.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("intotd=");
                sb.Append(InTotd.Value ? '1' : '0');
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

            if (InCollaborative.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("incollaborative=");
                sb.Append(InCollaborative.Value ? '1' : '0');
                first = false;
            }

            if (!string.IsNullOrEmpty(Secret))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("secret=");
                sb.Append(WebUtility.UrlEncode(Secret));
                first = false;
            }

            if (MappackSecret.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("mappacksecret=");
                sb.Append(MappackSecret.Value);
                first = false;
            }

            if (!string.IsNullOrEmpty(DriverLogin))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("driverlogin=");
                sb.Append(WebUtility.UrlEncode(DriverLogin));
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
