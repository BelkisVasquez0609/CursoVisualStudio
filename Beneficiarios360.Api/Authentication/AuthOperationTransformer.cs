using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Beneficiarios360.Api.Authentication;

internal sealed class AuthOperationTransformer :  IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        IList<object> metadata = context.Description.ActionDescriptor.EndpointMetadata;

        bool allowAnonymous = metadata.OfType<IAllowAnonymous>().Any();

        bool requiresAuthorization = metadata.OfType<IAuthorizeData>().Any();

        if (allowAnonymous || !requiresAuthorization)
        {
            return Task.CompletedTask;
        }

        operation.Security ??=  new List<OpenApiSecurityRequirement>();

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        context.Document)
                ] = []
            });

        return Task.CompletedTask;
    }
}