using System.ComponentModel.DataAnnotations;

namespace Beneficiarios360.Api.DTOs;

public sealed class ActualizarBeneficiarioRequest
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 100 caracteres.")]
    public string Nombres { get; init; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength( 100, MinimumLength = 2, ErrorMessage ="Los apellidos deben tener entre 2 y 100 caracteres.")]
    public string Apellidos { get; init; } = string.Empty;

    public bool Activo { get; init; }
}