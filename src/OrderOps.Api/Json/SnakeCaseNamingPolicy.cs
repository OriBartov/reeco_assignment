using System.Text.Json;

namespace OrderOps.Api.Json;

public sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public static SnakeCaseNamingPolicy Instance { get; } = new();

    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var buffer = new char[name.Length + 16];
        var pos = 0;

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    buffer[pos++] = '_';
                }

                buffer[pos++] = char.ToLowerInvariant(c);
            }
            else
            {
                buffer[pos++] = c;
            }
        }

        return new string(buffer, 0, pos);
    }
}
