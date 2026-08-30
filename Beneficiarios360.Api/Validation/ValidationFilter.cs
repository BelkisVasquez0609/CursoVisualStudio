using System.ComponentModel.DataAnnotations;

namespace Beneficiarios360.Api.Validation;

public sealed class ValidationFilter<T> :
    IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Busca en los parámetros del endpoint
        // un objeto que sea del tipo T.
        // Esto indicaría que el filtro
        // fue aplicado con un tipo incorrecto.

        //T es el tipo de solicitud que queremos validar.
        T? request = context.Arguments.OfType<T>().FirstOrDefault() ?? throw new InvalidOperationException($"No se encontró un argumento del tipo " +
                                                 $"{typeof(T).Name} en el endpoint.");

        // Lista donde se guardarán los errores.
        var validationResults =new List<ValidationResult>();

        // Información necesaria para validar el objeto.
        var validationContext = new ValidationContext(request);

        // Ejecuta las reglas DataAnnotations.
        bool valid = Validator.TryValidateObject(request,
                                                 validationContext,
                                                 validationResults,
                                                 validateAllProperties: true);

        // Si no hay errores, ejecuta el endpoint.
        if (valid)
            return await next(context);

        // Organiza los errores por propiedad.
        Dictionary<string, string[]> errors =
            validationResults
                .SelectMany(
                    result => result.MemberNames
                            .DefaultIfEmpty("request")
                            .Select(
                                member => new
                                    {
                                        Member = ToCamelCase(member),

                                        Message =  result.ErrorMessage ?? "El valor no es válido."
                                    }))
                .GroupBy(item => item.Member)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(item => item.Message).Distinct().ToArray());

        // Detiene la solicitud y devuelve 400.
        return Results.ValidationProblem(
            errors,
            title: "La solicitud contiene errores de validación.",
            detail: "Revise los campos indicados.",
            statusCode:
                StatusCodes.Status400BadRequest);
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "request";

        if (value.Length == 1)
            return value.ToLowerInvariant(); //Equivalente en minúsculas de la cadena actual.

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}