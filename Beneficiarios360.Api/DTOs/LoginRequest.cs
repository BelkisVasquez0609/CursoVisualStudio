using System.ComponentModel.DataAnnotations;

namespace Beneficiarios360.Api.DTOs;

public sealed class LoginRequest
{
    [Required(ErrorMessage =  "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3,  ErrorMessage ="El usuario debe tener entre 3 y 50 caracteres.")]
    public string UserName { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength( 100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; init; } = string.Empty;
}