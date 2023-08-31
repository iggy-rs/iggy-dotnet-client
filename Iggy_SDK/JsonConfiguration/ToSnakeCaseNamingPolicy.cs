using System.Text.Json;
using Iggy_SDK.Extensions;

namespace Iggy_SDK.JsonConfiguration;

internal sealed class ToSnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToSnakeCase();
}