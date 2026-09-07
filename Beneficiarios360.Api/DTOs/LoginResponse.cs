namespace Beneficiarios360.Api.DTOs
{
    public sealed record LoginResponse(
     string AccessToken,
     string TokenType,
     DateTime ExpiresUtc,
     int ExpiresIn,
     int UserId,
     string UserName,
     string FullName,
     string Role);
}
