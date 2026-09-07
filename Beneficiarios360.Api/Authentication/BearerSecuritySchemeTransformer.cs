using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Beneficiarios360.Api.Authentication;

internal sealed class
    BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider  authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        IEnumerable<AuthenticationScheme> authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        bool bearerExists = authenticationSchemes.Any(scheme => scheme.Name == "Bearer");

        if (!bearerExists)
        {
            return;
        }

        var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] =
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme ="bearer",
                        In = ParameterLocation.Header,
                        BearerFormat = "JSON Web Token",
                        Description = "Introduzca el token JWT."
                    }
            };

        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes = securitySchemes;
    }
}