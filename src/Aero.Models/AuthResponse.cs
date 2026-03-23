namespace Aero.Models;

public class AuthResponse(string accessToken, string refreshToken, DateTimeOffset expiration)
{
    public string accessToken { get; set; } = accessToken;
    public string refreshToken { get; set; } = refreshToken;
    public DateTimeOffset Expiration { get; set; } = expiration;
}