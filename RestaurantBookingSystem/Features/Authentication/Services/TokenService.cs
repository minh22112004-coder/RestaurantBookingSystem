using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Features.Authentication.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(
        User user,
        IList<string> roles)
    {
        string jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Missing Jwt:Key configuration."
            );

        string issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "Missing Jwt:Issuer configuration."
            );

        string audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "Missing Jwt:Audience configuration."
            );

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()
            ),

            new Claim(
                ClaimTypes.Name,
                user.Username
            ),

            new Claim(
                ClaimTypes.Email,
                user.Email ?? string.Empty
            )
        };

        foreach (string role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role
                )
            );
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        );

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: GetExpirationTime(),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public DateTime GetExpirationTime()
    {
        int expirationMinutes =
            _configuration.GetValue<int>(
                "Jwt:ExpirationMinutes"
            );

        if (expirationMinutes <= 0)
        {
            expirationMinutes = 120;
        }

        return DateTime.UtcNow.AddMinutes(
            expirationMinutes
        );
    }
}
