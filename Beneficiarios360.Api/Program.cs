using Beneficiarios360.Api.Data;
using Beneficiarios360.Api.Endpoints;
using Beneficiarios360.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Configuración de OpenAPI
builder.Services.AddOpenApi();

// Manejo estandarizado de errores
builder.Services.AddProblemDetails();

//NEW
string sqlServerConnection =
    builder.Configuration
        .GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException(
        "No se configuró " +
        "ConnectionStrings:SqlServer.");

builder.Services
    .AddHealthChecks()

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

var app = builder.Build();

// Pipeline
app.UseExceptionHandler();
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
app.MapGet("/", () => Results.Redirect("/swagger"));

//NEW
//Este endpoint solamente responde:
    //“¿La aplicación está encendida y puede responder?”

//No comprueba:
    //SQL Server.
    //Servicios externos.
    //Espacio en disco.
    //Conexión con otras APIs.

app.MapHealthChecks("live",
    new HealthCheckOptions
    {
        Predicate =
            healthCheck =>
                healthCheck.Tags.Contains("live"),

        ResponseWriter =
            async (context, report) =>
            {
                context.Response.ContentType =
                    "application/json";

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        status =
                            report.Status.ToString(),

                        message =
                            "La API está funcionando.",

                        utc =
                            DateTime.UtcNow
                    });
            }
    });

//// ¿La aplicación está lista para trabajar?
app.MapHealthChecks(
    "ready",
    new HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("ready"),

        ResponseWriter =
            async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var response =
                    new
                    {
                        status = report.Status.ToString(),
                        message = report.Status == HealthStatus.Healthy ? "La API está lista para trabajar." : "La API no está lista para trabajar.",
                        duration = report.TotalDuration.TotalMilliseconds,
                        checks = report.Entries.Select(
                                 entry =>
                                    new
                                    {
                                        name = entry.Key,
                                        status = entry.Value.Status.ToString(),
                                        description = entry.Value.Description,
                                        duration = entry.Value.Duration.TotalMilliseconds,
                                        error =entry.Value.Exception?.Message
                                    }),

                        utc = DateTime.UtcNow
                    };

                await context.Response.WriteAsJsonAsync(response);
            }
    });
// Endpoints de beneficiarios

app.MapBeneficiarios();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
