using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Iggy_SDK.JsonConfiguration;

public sealed class AuthResponseConverter : JsonConverter<AuthResponse>
{

    public override AuthResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        
        var userId = root.GetProperty(nameof(AuthResponse.UserId).ToSnakeCase()).GetInt32(); 
        var tokens = root.GetProperty(nameof(AuthResponse.Tokens).ToSnakeCase());

        var accessTokenProperty = tokens.GetProperty(nameof(Tokens.AccessToken).ToSnakeCase());
        var accessToken = accessTokenProperty.GetProperty(nameof(TokenInfo.Token).ToSnakeCase()).GetString();
        var accessTokenExpiry = accessTokenProperty.GetProperty(nameof(TokenInfo.Expiry).ToSnakeCase()).GetInt64();
        
        var refreshTokenProperty = tokens.GetProperty(nameof(Tokens.RefreshToken).ToSnakeCase());
        var refreshToken = refreshTokenProperty.GetProperty(nameof(TokenInfo.Token).ToSnakeCase()).GetString();
        var refreshTokenExpiry = refreshTokenProperty.GetProperty(nameof(TokenInfo.Expiry).ToSnakeCase()).GetInt64();
        
        return new AuthResponse
        {
            UserId = userId,
            Tokens = new Tokens
            {
                AccessToken = new TokenInfo
                {
                    Token = accessToken!,
                    Expiry = DateTimeOffset.FromUnixTimeSeconds(accessTokenExpiry).LocalDateTime,
                },
                RefreshToken = new TokenInfo
                {
                    Token = refreshToken!,
                    Expiry = DateTimeOffset.FromUnixTimeSeconds(refreshTokenExpiry).LocalDateTime,
                }
            }
        };
    }
    public override void Write(Utf8JsonWriter writer, AuthResponse value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}