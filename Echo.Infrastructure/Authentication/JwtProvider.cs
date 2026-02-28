using Echo.Application.Interfaces;
using Echo.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Echo.Infrastructure.Authentication;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        // 1. Берем данные из appsettings.json
        var secretKey = _configuration["JwtOptions:SecretKey"];
        var issuer = _configuration["JwtOptions:Issuer"];
        var audience = _configuration["JwtOptions:Audience"];

        // 2. Упаковываем данные юзера в Claims
        var claims = new[]
        {
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name, user.Username)
        };

        // 3. Создаем ключ подписи
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // 4. Собираем сам токен (время жизни 1 час)
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            null,
            DateTime.UtcNow.AddHours(1),
            credentials);

        // 5. Превращаем в строку
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}