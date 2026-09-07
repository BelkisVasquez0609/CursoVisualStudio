using System.ComponentModel.DataAnnotations;

namespace Beneficiarios360.Api.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    [MinLength( 32, ErrorMessage = "La clave JWT debe tener al menos 32 caracteres.")]
    public string SecretKey { get; init; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "La expiración debe estar entre 1 y 1440 minutos.")]
    public int ExpirationMinutes { get; init; } =60;
}