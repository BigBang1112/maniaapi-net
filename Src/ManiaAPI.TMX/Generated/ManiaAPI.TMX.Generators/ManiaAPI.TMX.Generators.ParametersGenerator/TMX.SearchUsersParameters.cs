using System.Runtime.InteropServices;
using System.Text;
using System.Net;

namespace ManiaAPI.TMX;

public partial class TMX
{
    /// <remarks>Be careful using <c>default</c> value. It also disables all fields.</remarks>
    [StructLayout(LayoutKind.Auto)]
    public readonly partial record struct SearchUsersParameters
    {
        public global::ManiaAPI.TMX.UserItemFields Fields { get; init; }

        public SearchUsersParameters()
        {
            Fields = global::ManiaAPI.TMX.UserItemFields.All;
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

            if (!string.IsNullOrEmpty(Name))
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("name=");
                sb.Append(WebUtility.UrlEncode(Name));
                first = false;
            }

            if (MxId is not null)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("mxid=");
                sb.Append(string.Join("%2C", MxId));
                first = false;
            }

            if (TracksMin.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("tracksmin=");
                sb.Append(TracksMin.Value);
                first = false;
            }

            if (TracksMax.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("tracksmax=");
                sb.Append(TracksMax.Value);
                first = false;
            }

            if (AwardsMin.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("awardsmin=");
                sb.Append(AwardsMin.Value);
                first = false;
            }

            if (AwardsMax.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("awardsmax=");
                sb.Append(AwardsMax.Value);
                first = false;
            }

            if (AwardsGivenMin.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("awardsgivenmin=");
                sb.Append(AwardsGivenMin.Value);
                first = false;
            }

            if (AwardsGivenMax.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("awardsgivenmax=");
                sb.Append(AwardsGivenMax.Value);
                first = false;
            }

            if (RegisteredAfter.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("registeredafter=");
                sb.Append(RegisteredAfter.Value);
                first = false;
            }

            if (RegisteredBefore.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("registeredbefore=");
                sb.Append(RegisteredBefore.Value);
                first = false;
            }

            if (InSupporters.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("insupporters=");
                sb.Append(InSupporters.Value ? '1' : '0');
                first = false;
            }

            if (InModerators.HasValue)
            {
                if (first) sb.Append('?');
                else sb.Append('&');
                sb.Append("inmoderators=");
                sb.Append(InModerators.Value ? '1' : '0');
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
