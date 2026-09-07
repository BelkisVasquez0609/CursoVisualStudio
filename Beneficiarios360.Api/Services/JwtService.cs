using Beneficiarios360.Api.Configuration;
using Beneficiarios360.Api.DTOs;
using Beneficiarios360.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Beneficiarios360.Api.Services;

public sealed class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public LoginResponse GenerateToken(AppUser user)
    {
        DateTime issuedUtc =  DateTime.UtcNow;

        DateTime expiresUtc =  issuedUtc.AddMinutes( _settings.ExpirationMinutes);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

            new Claim(JwtRegisteredClaimNames.UniqueName,  user.UserName),

            new Claim(ClaimTypes.NameIdentifier,  user.Id.ToString()),

            new Claim(ClaimTypes.Name, user.FullName),

            new Claim( ClaimTypes.Role, user.Role),

            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(issuedUtc).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        ];

        byte[] secretKey = Encoding.UTF8.GetBytes(_settings.SecretKey);

        var signingKey = new SymmetricSecurityKey(secretKey);

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: issuedUtc,
                expires:expiresUtc,
                signingCredentials: credentials);

        string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResponse(
            AccessToken: accessToken,
            TokenType: "Bearer",
            ExpiresUtc: expiresUtc,
            ExpiresIn:(int)TimeSpan.FromMinutes(_settings.ExpirationMinutes).TotalSeconds,
            UserId: user.Id,
            UserName: user.UserName,
            FullName: user.FullName,
            Role: user.Role);
    }
}