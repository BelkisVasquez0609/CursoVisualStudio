using Beneficiarios360.Api.DTOs;
using Beneficiarios360.Api.Services;
using Beneficiarios360.Api.Validation;

namespace Beneficiarios360.Api.Endpoints;

public static class BeneficiarioEndpoints
{
    public static IEndpointRouteBuilder MapBeneficiarios(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group =app.MapGroup( "/api/beneficiarios") .WithTags("Beneficiarios");

        group.MapGet("/", GetAllAsync)
                           .WithName("GetBeneficiarios")
                           .WithSummary("Obtiene los beneficiarios");

        group.MapGet("/{id:int}", GetByIdAsync)
                                .WithName("GetBeneficiarioById")
                                .WithSummary("Obtiene un beneficiario por ID");

        group.MapGet("/documento/{documento}", GetByDocumentoAsync)
                                                .WithName("GetBeneficiarioByDocumento")
                                                .WithSummary("Busca un beneficiario por documento");

        group.MapPost("/", CreateAsync)
                           .AddEndpointFilter<ValidationFilter<CrearBeneficiarioRequest>>()//Importante
                           .WithName("CreateBeneficiario")
                           .WithSummary("Registra un nuevo beneficiario")
                           .Produces<BeneficiarioDto>(StatusCodes.Status201Created)
                           .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                           .Produces( StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", UpdateAsync)
                                .AddEndpointFilter<ValidationFilter<ActualizarBeneficiarioRequest>>()
                                .WithName("UpdateBeneficiario")
                                .WithSummary("Actualiza un beneficiario")
                                .Produces(StatusCodes.Status204NoContent)
                                .ProducesValidationProblem(StatusCodes.Status400BadRequest)
                                .Produces(StatusCodes.Status404NotFound);

                                    return app;
                                }

       //TO-DO: Agregar endpoint DELETE (Inactivar) junto con su nombre y Summary

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
        BeneficiarioDto? item = await service.GetByDocumentoAsync(documento, ct);

        return item is null
            ? Results.NotFound()
            : Results.Ok(item);
    }

    private static async Task<IResult> CreateAsync(CrearBeneficiarioRequest request, IBeneficiarioService service, CancellationToken ct)
    {
        CreateBeneficiarioResult result = await service.CreateAsync(request, ct);

        if (result.Duplicate)
        {
            return Results.Conflict(
                new
                {
                    title = "Documento duplicado",
                    status = StatusCodes.Status409Conflict,
                    detail = result.Error,
                    errorCode = "BENEFICIARIO_DOCUMENTO_DUPLICADO"
                });
        }

        if (!result.Success)
        {
            return Results.Problem(
                title: "No se pudo registrar el beneficiario.",
                detail: result.Error,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Created($"/api/beneficiarios/" + $"{result.Beneficiario!.Id}", result.Beneficiario);
    }


    private static async Task<IResult> UpdateAsync( int id, ActualizarBeneficiarioRequest request, IBeneficiarioService service, CancellationToken ct)
    {
        bool updated =
            await service.UpdateAsync(id,request, ct);

        return updated ? Results.NoContent() : Results.NotFound();
    }

    //TO-DO: Endpoint DELETE
}