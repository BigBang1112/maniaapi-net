using ManiaAPI.TMX.Attributes;
using System.Reflection;
using System.Text;

namespace ManiaAPI.TMX;

public abstract record RequestParameters
{
    public string ToQuery()
    {
        var properties = GetType().GetProperties()
            .Where(x => Attribute.IsDefined(x, typeof(QueryAttribute)))
            .ToList();

        var builder = new StringBuilder("?");

        var counter = 0;

        foreach (var property in properties)
        {
            var query = property.GetCustomAttribute<QueryAttribute>()!;

            var value = property.GetValue(this);

            if (value is null)
            {
                continue;
            }

            if (counter > 0)
            {
                builder.Append('&');
            }

            builder.Append(query.Name);
            builder.Append('=');

            if (value is Enum)
            {
                builder.Append((int)value);
            }
            else
            {
                builder.Append(value);
            }

            counter++;
        }

        return builder.ToString();
    }
}
