using System.Text.Json.Serialization;

namespace NettoRetail.Application.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LockableEntitiesEnum
{
    Role = 0
}
