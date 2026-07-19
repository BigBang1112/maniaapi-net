using System.Text;

namespace ManiaAPI.TMX;

public readonly record struct TrackItemFields
{
    public bool TrackId { get; init; }
    public bool TrackName { get; init; }
    public bool UId { get; init; }
    public bool AuthorTime { get; init; }
    public bool AuthorScore { get; init; }
    public bool GoldTarget { get; init; }
    public bool SilverTarget { get; init; }
    public bool BronzeTarget { get; init; }
    public global::ManiaAPI.TMX.UserFields Uploader { get; init; }
    public bool UploadedAt { get; init; }
    public bool UpdatedAt { get; init; }
    public bool ActivityAt { get; init; }
    public bool PrimaryType { get; init; }
    public bool TrackValue { get; init; }
    public bool AuthorComments { get; init; }
    public bool Style { get; init; }
    public bool Routes { get; init; }
    public bool Difficulty { get; init; }
    public bool Environment { get; init; }
    public bool Car { get; init; }
    public bool Mood { get; init; }
    public bool Awards { get; init; }
    public bool Comments { get; init; }
    public bool ReplayType { get; init; }
    public bool HasThumbnail { get; init; }
    public bool AuthorBeaten { get; init; }
    public bool AuthorBeatable { get; init; }
    public global::ManiaAPI.TMX.WRReplayFields WRReplay { get; init; }
    public global::ManiaAPI.TMX.UserReplayFields UserReplay { get; init; }
    public bool Authors { get; init; }
    public bool Tags { get; init; }
    public bool Images { get; init; }
    public IEnumerable<string>? AdditionalFields { get; init; }

    public static readonly TrackItemFields All = new()
    {
        TrackId = true,
        TrackName = true,
        UId = true,
        AuthorTime = true,
        AuthorScore = true,
        GoldTarget = true,
        SilverTarget = true,
        BronzeTarget = true,
        Uploader = global::ManiaAPI.TMX.UserFields.All,
        UploadedAt = true,
        UpdatedAt = true,
        ActivityAt = true,
        PrimaryType = true,
        TrackValue = true,
        AuthorComments = true,
        Style = true,
        Routes = true,
        Difficulty = true,
        Environment = true,
        Car = true,
        Mood = true,
        Awards = true,
        Comments = true,
        ReplayType = true,
        HasThumbnail = true,
        AuthorBeaten = true,
        AuthorBeatable = true,
        WRReplay = global::ManiaAPI.TMX.WRReplayFields.All,
        UserReplay = global::ManiaAPI.TMX.UserReplayFields.All,
        Authors = true,
        Tags = true,
        Images = true,
    };

    internal bool Append(StringBuilder sb)
    {
        var first = true;

        if (TrackId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.TrackId));
            first = false;
        }

        if (TrackName)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.TrackName));
            first = false;
        }

        if (UId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.UId));
            first = false;
        }

        if (AuthorTime)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.AuthorTime));
            first = false;
        }

        if (AuthorScore)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.AuthorScore));
            first = false;
        }

        if (GoldTarget)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.GoldTarget));
            first = false;
        }

        if (SilverTarget)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.SilverTarget));
            first = false;
        }

        if (BronzeTarget)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.BronzeTarget));
            first = false;
        }

        if (Uploader.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Uploader));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (Uploader.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Uploader));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
            first = false;
        }

        if (UploadedAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.UploadedAt));
            first = false;
        }

        if (UpdatedAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.UpdatedAt));
            first = false;
        }

        if (ActivityAt)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.ActivityAt));
            first = false;
        }

        if (PrimaryType)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.PrimaryType));
            first = false;
        }

        if (TrackValue)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.TrackValue));
            first = false;
        }

        if (AuthorComments)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.AuthorComments));
            first = false;
        }

        if (Style)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Style));
            first = false;
        }

        if (Routes)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Routes));
            first = false;
        }

        if (Difficulty)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Difficulty));
            first = false;
        }

        if (Environment)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Environment));
            first = false;
        }

        if (Car)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Car));
            first = false;
        }

        if (Mood)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Mood));
            first = false;
        }

        if (Awards)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Awards));
            first = false;
        }

        if (Comments)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Comments));
            first = false;
        }

        if (ReplayType)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.ReplayType));
            first = false;
        }

        if (HasThumbnail)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.HasThumbnail));
            first = false;
        }

        if (AuthorBeaten)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.AuthorBeaten));
            first = false;
        }

        if (AuthorBeatable)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.AuthorBeatable));
            first = false;
        }

        if (WRReplay.User.UserId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.WRReplay));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.WRReplay.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.UserId));
            first = false;
        }

        if (WRReplay.User.Name)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.WRReplay));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.WRReplay.User));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.User.Name));
            first = false;
        }

        if (WRReplay.ReplayTime)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.WRReplay));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.WRReplay.ReplayTime));
            first = false;
        }

        if (WRReplay.ReplayScore)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.WRReplay));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.WRReplay.ReplayScore));
            first = false;
        }

        if (WRReplay.ReplayId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.WRReplay));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.WRReplay.ReplayId));
            first = false;
        }

        if (UserReplay.ReplayId)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.UserReplay));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.UserReplay.ReplayId));
            first = false;
        }

        if (UserReplay.ReplayTime)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.UserReplay));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.UserReplay.ReplayTime));
            first = false;
        }

        if (UserReplay.ReplayScore)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.UserReplay));
            sb.Append('.');
            sb.Append(nameof(global::ManiaAPI.TMX.UserReplay.ReplayScore));
            first = false;
        }

        if (Authors)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Authors));
            first = false;
        }

        if (Tags)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Tags));
            first = false;
        }

        if (Images)
        {
            if (!first) sb.Append("%2C");
            sb.Append(nameof(global::ManiaAPI.TMX.TrackItem.Images));
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

public partial record TrackItem
{
    /// <summary>
    /// Values of fields requested via <c>AdditionalFields</c> that are not represented by a strongly-typed property.
    /// </summary>
    [global::System.Text.Json.Serialization.JsonExtensionData]
    public global::System.Collections.Generic.IDictionary<string, global::System.Text.Json.JsonElement>? AdditionalFields { get; set; }
}
