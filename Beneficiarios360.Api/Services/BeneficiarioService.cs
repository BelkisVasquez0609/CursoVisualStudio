using Beneficiarios360.Api.Data;
using Beneficiarios360.Api.DTOs;
using Beneficiarios360.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beneficiarios360.Api.Services;

public sealed class BeneficiarioService(AppDbContext db,ILogger<BeneficiarioService> logger): IBeneficiarioService
{
    public async Task<IReadOnlyList<BeneficiarioDto>> GetAllAsync( string? search, bool? activo, CancellationToken ct)
    {
        IQueryable<Beneficiario> query = db.Beneficiarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string value = search.Trim();

            query = query.Where(x =>
                x.Documento.Contains(value) ||
                x.Nombres.Contains(value) ||
                x.Apellidos.Contains(value));
        }

        if (activo.HasValue)
        {
            query = query.Where(x => x.Activo == activo.Value);
        }

        return await query
            .OrderBy(x => x.Apellidos)
            .ThenBy(x => x.Nombres)
            .Select(x => ToDto(x))
            .ToListAsync(ct);
    }

    public Task<BeneficiarioDto?> GetByIdAsync( int id, CancellationToken ct)
    {
        return db.Beneficiarios
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .SingleOrDefaultAsync(ct);
    }

    public Task<BeneficiarioDto?> GetByDocumentoAsync( string documento,CancellationToken ct)
    {
        string value = documento.Trim();

        return db.Beneficiarios
            .AsNoTracking()
            .Where(x => x.Documento == value)
            .Select(x => ToDto(x))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<CreateBeneficiarioResult> CreateAsync(CrearBeneficiarioRequest request, CancellationToken ct)
    {
        string documento = request.Documento.Trim();

        string nombres = request.Nombres.Trim();

        string apellidos = request.Apellidos.Trim();

        if (string.IsNullOrWhiteSpace(documento) ||
            string.IsNullOrWhiteSpace(nombres) ||
            string.IsNullOrWhiteSpace(apellidos))
        {
            return new(false, false, null, "Documento, nombres y apellidos son obligatorios.");
        }

        bool duplicate =
            await db.Beneficiarios.AnyAsync(
                x => x.Documento == documento,
                ct);

        if (duplicate)
        {
            return new(false, true, null,"El documento ya está registrado.");
        }

        var entity = new Beneficiario
        {
            Documento = documento,
            Nombres = nombres,
            Apellidos = apellidos
        };

        db.Beneficiarios.Add(entity);

        await db.SaveChangesAsync(ct);

        logger.LogInformation( "Beneficiario {BeneficiarioId} registrado",entity.Id);

        return new(
            true,
            false,
            ToDto(entity),
            null);
    }

    public async Task<bool> UpdateAsync(int id, ActualizarBeneficiarioRequest request, CancellationToken ct)
    {
        Beneficiario? entity =await db.Beneficiarios.FindAsync([id],ct);

        if (entity is null)
            return false;

        entity.Nombres = request.Nombres.Trim();

        entity.Apellidos = request.Apellidos.Trim();

        entity.Activo = request.Activo;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Beneficiario {BeneficiarioId} actualizado", entity.Id);

        return true;
    }

    //TO-DO: Agregar metodo de servicio para DELETE

    private static BeneficiarioDto ToDto(Beneficiario entity)
    {
        return new BeneficiarioDto(
            entity.Id,
            entity.Documento,
            $"{entity.Nombres} {entity.Apellidos}",
            entity.Activo,
            entity.CreadoUtc);
    }
}