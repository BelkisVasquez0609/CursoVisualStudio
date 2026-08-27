using Beneficiarios360.Api.DTOs;
using Beneficiarios360.Api.Services;

namespace Beneficiarios360.Api.Endpoints;

public static class BeneficiarioEndpoints
{
    public static IEndpointRouteBuilder MapBeneficiarios(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group =app.MapGroup( "/api/beneficiarios") .WithTags("Beneficiarios");

        group.MapGet("/", GetAllAsync);

        group.MapGet("/{id:int}", GetByIdAsync);

        group.MapGet( "/documento/{documento}", GetByDocumentoAsync);

        group.MapPost("/", CreateAsync);

        group.MapPut("/{id:int}", UpdateAsync);

        group.MapDelete("/{id:int}", DeactivateAsync);

        return app;
    }

    private static async Task<IResult> GetAllAsync(string? search, bool? activo,IBeneficiarioService service,CancellationToken ct)
    {
        var result =
            await service.GetAllAsync(
                search,
                activo,
                ct);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetByIdAsync( int id, IBeneficiarioService service, CancellationToken ct)
    {
        BeneficiarioDto? item = await service.GetByIdAsync(id, ct);

        return item is null
            ? Results.NotFound()
            : Results.Ok(item);
    }

    private static async Task<IResult>GetByDocumentoAsync(string documento, IBeneficiarioService service,  CancellationToken ct)
    {
        BeneficiarioDto? item = await service.GetByDocumentoAsync(
                                    documento,
                                    ct);

        return item is null
            ? Results.NotFound()
            : Results.Ok(item);
    }

    private static async Task<IResult> CreateAsync( CrearBeneficiarioRequest request,  IBeneficiarioService service, CancellationToken ct)
    {
        CreateBeneficiarioResult result =
            await service.CreateAsync(
                request,
                ct);

        if (result.Duplicate)
        {
            return Results.Conflict(
                new
                {
                    message = result.Error
                });
        }

        if (!result.Success)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["beneficiario"] =
                        new[] { result.Error! }
                });
        }

        return Results.Created(
            $"/api/beneficiarios/{result.Beneficiario!.Id}",
            result.Beneficiario);
    }

    private static async Task<IResult> UpdateAsync( int id, ActualizarBeneficiarioRequest request, IBeneficiarioService service, CancellationToken ct)
    {
        bool updated =
            await service.UpdateAsync(
                id,
                request,
                ct);

        return updated
            ? Results.NoContent()
            : Results.NotFound();
    }

    private static async Task<IResult> DeactivateAsync(int id, IBeneficiarioService service, CancellationToken ct)
    {
        DeactivateBeneficiarioResult result = await service.DeactivateAsync(id, ct);

        return result switch
        {
            DeactivateBeneficiarioResult.Success => Results.NoContent(),
            DeactivateBeneficiarioResult.NotFound => Results.NotFound(),
            DeactivateBeneficiarioResult.AlreadyInactive => Results.BadRequest(new { message = "El beneficiario ya se encuentra inactivo." }),
            _ => Results.StatusCode(500)
        };
    }
}