namespace Beneficiarios360.Api.DTOs
{
    public sealed record CrearBeneficiarioRequest(
     string Documento,
     string Nombres,
     string Apellidos);
}
