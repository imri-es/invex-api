using System.Text.Json.Serialization;

namespace invex_api.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Newbie,
    Writer,
    Creator,
    Admin,
}
