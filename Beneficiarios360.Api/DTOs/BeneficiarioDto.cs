namespace Beneficiarios360.Api.DTOs
{
    public sealed record BeneficiarioDto(
    int Id,
    string Documento,
    string NombreCompleto,
    bool Activo,
    DateTime CreadoUtc);
}
