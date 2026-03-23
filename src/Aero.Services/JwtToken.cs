namespace Aero.Services;

// todo - add some more properties
public sealed class JwtToken(JwtSecurityToken token)
{
    public DateTime ValidTo => token.ValidTo;
    public string Value => new JwtSecurityTokenHandler().WriteToken(token);
}