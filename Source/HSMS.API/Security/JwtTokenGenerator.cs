using HSMS.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HSMS.API
{
    public static class JwtTokenGenerator
    {
        public static string Generate(
            IConfiguration config,
            Guid userId,
            IEnumerable<AppPermission> permissions)
        {
            var claims = new List<Claim>
        {
            new("user_id", userId.ToString())
        };

            claims.AddRange(
                permissions.Select(p =>
                    new Claim("permission", p.ToString()))
            );

            var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
