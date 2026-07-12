using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct MapItemFields
{
    public bool MapId { get; init; }
    public bool MapUid { get; init; }
    public bool OnlineMapId { get; init; }
    public bool Name { get; init; }
    public bool GbxMapName { get; init; }
    public bool UploadedAt { get; init; }
    public bool UpdatedAt { get; init; }
    public bool ActivityAt { get; init; }
    public global::ManiaAPI.TMX.UserFields Uploader { get; init; }
    public bool Authors { get; init; }
    public bool Type { get; init; }
    public bool MapType { get; init; }
    public bool Environment { get; init; }
    public bool Vehicle { get; init; }
    public bool VehicleName { get; init; }
    public bool Mood { get; init; }
    public bool MoodFull { get; init; }
    public bool Style { get; init; }
    public bool Routes { get; init; }
    public bool Difficulty { get; init; }
    public global::ManiaAPI.TMX.MapMedalsFields Medals { get; init; }
    public bool CustomLength { get; init; }
    public bool Length { get; init; }
    public bool AwardCount { get; init; }
    public bool CommentCount { get; init; }
    public bool DownloadCount { get; init; }
    public bool ReplayCount { get; init; }
    public bool ReplayType { get; init; }
    public bool ReplayWRID { get; init; }
    public bool TrackValue { get; init; }
    public bool TitlePack { get; init; }
    public global::ManiaAPI.TMX.MapFeatureFields Feature { get; init; }
    public bool HasThumbnail { get; init; }
    public bool HasImages { get; init; }
    public bool IsPublic { get; init; }
    public bool IsListed { get; init; }
    public bool ServerSizeExceeded { get; init; }
    public bool AuthorComments { get; init; }
    public bool Tags { get; init; }
    public bool Images { get; init; }
    public global::ManiaAPI.TMX.MappackInfoFields Mappack { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly MapItemFields All = new()
    {
        MapId = true,
        MapUid = true,
        OnlineMapId = true,
        Name = true,
        GbxMapName = true,
        UploadedAt = true,
        UpdatedAt = true,
        ActivityAt = true,
        Uploader = global::ManiaAPI.TMX.UserFields.All,
        Authors = true,
        Type = true,
        MapType = true,
        Environment = true,
        Vehicle = true,
        VehicleName = true,
        Mood = true,
        MoodFull = true,
        Style = true,
        Routes = true,
        Difficulty = true,
        Medals = global::ManiaAPI.TMX.MapMedalsFields.All,
        CustomLength = true,
        Length = true,
        AwardCount = true,
        CommentCount = true,
        DownloadCount = true,
        ReplayCount = true,
        ReplayType = true,
        ReplayWRID = true,
        TrackValue = true,
        TitlePack = true,
        Feature = global::ManiaAPI.TMX.MapFeatureFields.All,
        HasThumbnail = true,
        HasImages = true,
        IsPublic = true,
        IsListed = true,
        ServerSizeExceeded = true,
        AuthorComments = true,
        Tags = true,
        Images = true,
        Mappack = global::ManiaAPI.TMX.MappackInfoFields.All,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (MapId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.MapId));
            first = false;
        }

        if (MapUid)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.MapUid));
            first = false;
        }

        if (OnlineMapId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.OnlineMapId));
            first = false;
        }

        if (Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Name));
            first = false;
        }

        if (GbxMapName)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.GbxMapName));
            first = false;
        }

        if (UploadedAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.UploadedAt));
            first = false;
        }

        if (UpdatedAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.UpdatedAt));
            first = false;
        }

        if (ActivityAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.ActivityAt));
            first = false;
        }

        if (Uploader.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Uploader));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (Uploader.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Uploader));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
            first = false;
        }

        if (Authors)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Authors));
            first = false;
        }

        if (Type)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Type));
            first = false;
        }

        if (MapType)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.MapType));
            first = false;
        }

        if (Environment)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Environment));
            first = false;
        }

        if (Vehicle)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Vehicle));
            first = false;
        }

        if (VehicleName)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.VehicleName));
            first = false;
        }

        if (Mood)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Mood));
            first = false;
        }

        if (MoodFull)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.MoodFull));
            first = false;
        }

        if (Style)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Style));
            first = false;
        }

        if (Routes)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Routes));
            first = false;
        }

        if (Difficulty)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Difficulty));
            first = false;
        }

        if (Medals.Author)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Medals));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.MapMedals.Author));
            first = false;
        }

        if (Medals.Gold)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Medals));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.MapMedals.Gold));
            first = false;
        }

        if (Medals.Silver)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Medals));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.MapMedals.Silver));
            first = false;
        }

        if (Medals.Bronze)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Medals));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.MapMedals.Bronze));
            first = false;
        }

        if (CustomLength)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.CustomLength));
            first = false;
        }

        if (Length)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Length));
            first = false;
        }

        if (AwardCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.AwardCount));
            first = false;
        }

        if (CommentCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.CommentCount));
            first = false;
        }

        if (DownloadCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.DownloadCount));
            first = false;
        }

        if (ReplayCount)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.ReplayCount));
            first = false;
        }

        if (ReplayType)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.ReplayType));
            first = false;
        }

        if (ReplayWRID)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.ReplayWRID));
            first = false;
        }

        if (TrackValue)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.TrackValue));
            first = false;
        }

        if (TitlePack)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.TitlePack));
            first = false;
        }

        if (Feature.Comment)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Feature));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.MapFeature.Comment));
            first = false;
        }

        if (Feature.Pinned)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Feature));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.MapFeature.Pinned));
            first = false;
        }

        if (HasThumbnail)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.HasThumbnail));
            first = false;
        }

        if (HasImages)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.HasImages));
            first = false;
        }

        if (IsPublic)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.IsPublic));
            first = false;
        }

        if (IsListed)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.IsListed));
            first = false;
        }

        if (ServerSizeExceeded)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.ServerSizeExceeded));
            first = false;
        }

        if (AuthorComments)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.AuthorComments));
            first = false;
        }

        if (Tags)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Tags));
            first = false;
        }

        if (Images)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Images));
            first = false;
        }

        if (Mappack.MappackId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Mappack));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.MappackInfo.MappackId));
            first = false;
        }

        if (Mappack.MapStatus)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Mappack));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.MappackInfo.MapStatus));
            first = false;
        }

        if (Mappack.MapPosition)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.MapItem.Mappack));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.MappackInfo.MapPosition));
            first = false;
        }

        if (AdditionalFields is not null)
        {
            foreach (var additionalField in AdditionalFields)
            {
                if (!first) sb.Append("%2C");
                sb.Append(additionalField);
                first = false;
            }
        }

        return !first;
    }
}

public partial record MapItem
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
