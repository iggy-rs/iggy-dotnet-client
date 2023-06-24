using System.Text.Json;
using Iggy_SDK.Extensions;

namespace Iggy_SDK.SerializationConfiguration;

public sealed class ToSnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToSnakeCase();
}