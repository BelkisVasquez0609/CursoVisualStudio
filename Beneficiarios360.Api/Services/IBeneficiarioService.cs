using Beneficiarios360.Api.DTOs;

namespace Beneficiarios360.Api.Services
{
    public interface IBeneficiarioService
    {
        Task<IReadOnlyList<BeneficiarioDto>>GetAllAsync(string? search, bool? activo, CancellationToken ct);

        Task<BeneficiarioDto?> GetByIdAsync( int id,CancellationToken ct);

        Task<BeneficiarioDto?> GetByDocumentoAsync(string documento, CancellationToken ct);

        Task<CreateBeneficiarioResult> CreateAsync(CrearBeneficiarioRequest request,CancellationToken ct);

        Task<bool> UpdateAsync( int id,ActualizarBeneficiarioRequest request,CancellationToken ct);
    }

    public sealed record CreateBeneficiarioResult(
        bool Success,
        bool Duplicate,
        BeneficiarioDto? Beneficiario,
        string? Error);

    public enum DeactivateBeneficiarioResult
    {
        Success,
        NotFound,
        AlreadyInactive
    }
}
