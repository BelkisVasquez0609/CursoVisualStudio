using Beneficiarios360.Api.Data;
using Beneficiarios360.Api.Endpoints;
using Beneficiarios360.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Configuración de OpenAPI
builder.Services.AddOpenApi();

// Manejo estandarizado de errores
builder.Services.AddProblemDetails();

// DbContext: una instancia por solicitud
builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        string connectionString =
            builder.Configuration
                .GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException(
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI en JSON
    app.MapOpenApi();

    // Interfaz visual Swagger
    app.UseSwaggerUI(
        options =>
        {
            options.SwaggerEndpoint(
                "/openapi/v1.json",
                "Beneficiarios360 API v1");

            options.DocumentTitle =
                "Beneficiarios360 API";

            options.RoutePrefix =
                "swagger";

            options.DisplayRequestDuration();

            options.EnableTryItOutByDefault();
        });
}

// Redirige la raíz hacia Swagger
app.MapGet(
    "/",
    () => Results.Redirect("/swagger"));

// Verificar que la aplicación funciona
app.MapGet(
        "/health",
        () => Results.Ok(
            new
            {
                status = "ok",
                environment =
                    app.Environment.EnvironmentName,
                utc = DateTime.UtcNow
            }))
    .WithName("Health")
    .WithTags("Diagnóstico")
    .WithSummary(
        "Verifica que la API esté disponible");

// Endpoints de beneficiarios
app.MapBeneficiarios();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
