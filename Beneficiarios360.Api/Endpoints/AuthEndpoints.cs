using Beneficiarios360.Api.DTOs;
using Beneficiarios360.Api.Entities;
using Beneficiarios360.Api.Services;
using Beneficiarios360.Api.Validation;

namespace Beneficiarios360.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/auth").WithTags("Autenticación").AllowAnonymous();


        group.MapPost("/login", Login)
                                .AddEndpointFilter<ValidationFilter<LoginRequest>>()
                                .WithName("Login")
                                .WithSummary("Inicia sesión y genera un token JWT")
                                .WithDescription("Valida las credenciales y devuelve un token Bearer.")
                                .Produces<LoginResponse>(StatusCodes.Status200OK)
                                .ProducesValidationProblem( StatusCodes.Status400BadRequest)
                                .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static IResult Login(LoginRequest request, IUserService userService, IJwtService jwtService, ILoggerFactory loggerFactory)
    {
        ILogger logger = loggerFactory.CreateLogger("Authentication");

        AppUser? user = userService.Authenticate(request.UserName, request.Password);

        if (user is null)
        {
            logger.LogWarning( "Inicio de sesión rechazado para {UserName}",request.UserName);

            return Results.Problem(statusCode:StatusCodes.Status401Unauthorized,

                title: "Credenciales incorrectas",

                detail: "El usuario o la contraseña no son correctos.",

                extensions:
                    new Dictionary<string, object?>
                    {
                        ["errorCode"] ="AUTH_INVALID_CREDENTIALS"
                    });
        }

        LoginResponse response =jwtService.GenerateToken(user);

        logger.LogInformation(
            "Inicio de sesión correcto para {UserName} con rol {Role}",
            user.UserName,
            user.Role);

        return Results.Ok(response);
    }
}