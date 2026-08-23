namespace Beneficiarios360.Api.DTOs
{
    public sealed record ActualizarBeneficiarioRequest(
    string Nombres,
    string Apellidos,
    bool Activo);
}
