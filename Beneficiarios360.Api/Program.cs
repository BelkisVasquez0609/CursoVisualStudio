using Beneficiarios360.Api.Configuration;
using Beneficiarios360.Api.Data;
using Beneficiarios360.Api.Endpoints;
using Beneficiarios360.Api.Entities;
using Beneficiarios360.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Configuración de OpenAPI
builder.Services.AddOpenApi(
    options =>
    {
        options.AddDocumentTransformer(
            (
                document,
                context,
                cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();

                document.Components.SecuritySchemes = new Dictionary<string,IOpenApiSecurityScheme>
                    {
                        ["Bearer"] =
                            new OpenApiSecurityScheme
                            {
                                Type =SecuritySchemeType.Http,

                                Scheme ="bearer",

                                BearerFormat ="JWT",

                                In =ParameterLocation.Header,

                                Description ="Pegue aquí el token JWT."
                            }
                    };

                foreach (var path in document.Paths)
                {
                    bool publicEndpoint =path.Key.StartsWith("/api/auth",StringComparison.OrdinalIgnoreCase)|| path.Key.StartsWith("/health", StringComparison.OrdinalIgnoreCase) || path.Key == "/";

                    if (publicEndpoint)
                    {
                        continue;
                    }

                    foreach (var operation in path.Value.Operations)
                    {
                        operation.Value.Security ??= new List<OpenApiSecurityRequirement>();

                        operation.Value.Security.Add(
                            new OpenApiSecurityRequirement
                            {
                                [
                                    new OpenApiSecuritySchemeReference(
                                        "Bearer",
                                        document)
                                ] = []
                            });
                    }
                }

                return Task.CompletedTask;
            });
    });

// Manejo estandarizado de errores
builder.Services.AddProblemDetails();

string sqlServerConnection =
    builder.Configuration
        .GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException(
        "No se configuró " +
        "ConnectionStrings:SqlServer.");

builder.Services.AddOptions<JwtSettings>()
                .Bind(
                    builder.Configuration.GetSection(
                        JwtSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

JwtSettings jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName)
                                               .Get<JwtSettings>()
                                                ?? throw new InvalidOperationException("No se configuró la sección Jwt.");


//Registro de autenticación.Esto registra el componente que interpreta y valida tokens Bearer.
builder.Services
    .AddAuthentication(
        options =>
        {
            options.DefaultScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;

            options.DefaultAuthenticateScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;

            options.DefaultForbidScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;
        })
    .AddJwtBearer(
        options =>
        {
            // Para el laboratorio local con HTTP.
            options.RequireHttpsMetadata =
                false;

            options.SaveToken =
                true;

            options.MapInboundClaims =
                false;

            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    // Validar quién generó el token.
                    ValidateIssuer =
                        true,

                    ValidIssuer =
                        jwtSettings.Issuer,

                    // Validar para quién se generó.
                    ValidateAudience =
                        true,

                    ValidAudience =
                        jwtSettings.Audience,

                    // Validar la firma.
                    ValidateIssuerSigningKey =
                        true,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                jwtSettings.SecretKey)),

                    // Validar la expiración.
                    ValidateLifetime =
                        true,

                    RequireExpirationTime =
                        true,

                    RequireSignedTokens =
                        true,

                    ClockSkew =
                        TimeSpan.FromSeconds(30),

                    NameClaimType =
                        ClaimTypes.Name,

                    RoleClaimType =
                        ClaimTypes.Role
                };

            options.Events =
                new JwtBearerEvents
                {
                    OnAuthenticationFailed =
                        context =>
                        {
                            Console.WriteLine(
                                "TOKEN RECHAZADO:");

                            Console.WriteLine(
                                context.Exception
                                    .Message);

                            return Task.CompletedTask;
                        },

                    OnTokenValidated =
                        context =>
                        {
                            Console.WriteLine(
                                "TOKEN ACEPTADO");

                            return Task.CompletedTask;
                        }
                };
        });

//Una política agrupa condiciones de autorización.
builder.Services.AddAuthorization(
    options =>
    {
        options.AddPolicy("AdminOnly",
            policy =>
            {
                policy.RequireAuthenticatedUser();

                policy.RequireRole("Administrador");
            });
    });
builder.Services.AddHealthChecks()
    // Verifica que la API esté encendida.
    .AddCheck("self", () => HealthCheckResult.Healthy("La API está funcionando."),
                            tags: ["live"])

    // Verifica la conexión con SQL Server.
    .AddSqlServer(connectionString:sqlServerConnection,
                  name: "sql-server", //Es el nombre de la comprobación.
                  failureStatus: HealthStatus.Unhealthy, //Si SQL Server no responde, el estado será:
                  tags: ["ready"]);//Esta etiqueta permite incluir la comprobación en /health/ready.

// DbContext: una instancia por solicitud
builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        string connectionString =
            builder.Configuration
                .GetConnectionString("SqlServer") ?? throw new InvalidOperationException(
                                                     "No se configuró ConnectionStrings:SqlServer.");

        options.UseSqlServer(
            connectionString);
    });

// Servicio de negocio: una instancia por solicitud
builder.Services.AddScoped<IBeneficiarioService, BeneficiarioService>();

builder.Services.AddSingleton<PasswordHasher<AppUser>>();
builder.Services.AddSingleton<IUserService,UserService>();
builder.Services.AddSingleton<IJwtService,JwtService>();

var app = builder.Build();

// Pipeline
app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();
//app.UseHttpsRedirection();

// Mientras se utiliza solo HTTP:
// app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(
        options =>
        {
            options.SwaggerEndpoint( "/openapi/v1.json", "Beneficiarios360 API v1");

            options.DocumentTitle = "Beneficiarios360 API";

            options.RoutePrefix = "swagger";

            options.EnableTryItOutByDefault();

            options.DisplayRequestDuration();
        });
}

// Redirige la raíz hacia Swagger
app.MapGet("/",() => Results.Redirect("/swagger")).AllowAnonymous().ExcludeFromDescription();

//NEW
//Este endpoint solamente responde:
//“¿La aplicación está encendida y puede responder?”

//No comprueba:
//SQL Server.
//Servicios externos.
//Espacio en disco.
//Conexión con otras APIs.

app.MapHealthChecks("/health/live",
    new HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("live"),

        ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                await context.Response
                    .WriteAsJsonAsync(
                        new
                        {
                            status = report.Status.ToString(),

                            message ="La API está funcionando.",

                            utc = DateTime.UtcNow
                        });
            }
    });

//// ¿La aplicación está lista para trabajar?
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate =
            healthCheck =>
                healthCheck.Tags.Contains(
                    "ready"),

        ResponseWriter =
            async (context, report) =>
            {
                context.Response.ContentType =
                    "application/json";

                await context.Response
                    .WriteAsJsonAsync(
                        new
                        {
                            status = report.Status.ToString(),
                            message = report.Status == HealthStatus.Healthy ? "La API está lista para trabajar." : "La API no está lista para trabajar.",
                            duration = report.TotalDuration.TotalMilliseconds,
                            checks =
                                report.Entries.Select(
                                    entry =>
                                        new
                                        {
                                            name = entry.Key,
                                            status = entry.Value.Status.ToString(),
                                            description = entry.Value.Description,
                                            duration = entry.Value.Duration.TotalMilliseconds,
                                            error = entry.Value.Exception?.Message
                                        }),

                            utc =
                                DateTime.UtcNow
                        });
            }
    });
// Endpoints de beneficiarios
app.MapAuthEndpoints();
app.MapBeneficiarios();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
