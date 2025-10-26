namespace Masarif.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Masarif";
    public string Audience { get; set; } = "Masarif.Web";
    public string Key { get; set; } = "";
    public int AccessTokenMinutes { get; set; } = 120;
}
