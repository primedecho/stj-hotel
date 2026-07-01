using HotelSearch.Api.Configuration;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HotelSearch.Api.OpenApi;

internal static class OpenApiDocumentConfiguration
{
    public static void Configure(OpenApiOptions options)
    {
        options.AddDocumentTransformer(TransformDocumentAsync);
        options.AddOperationTransformer(TransformOperationAsync);
    }

    private static Task TransformDocumentAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Hotel Search API",
            Version = "v1",
            Description =
                "REST API for hotel CRUD and natural-language search (Lemax take-home PoC). " +
                "Write operations may require the X-Api-Key header when ApiKey:WriteKey is configured."
        };

        document.Servers =
        [
            new OpenApiServer { Url = "http://localhost:5103", Description = "Local dotnet run" },
            new OpenApiServer { Url = "http://localhost:8080", Description = "Docker Compose" }
        ];

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[ApiKeyOptions.HeaderName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = ApiKeyOptions.HeaderName,
            Description = "Optional API key for POST/PUT/DELETE when ApiKey:WriteKey is configured on the server."
        };

        return Task.CompletedTask;
    }

    private static Task TransformOperationAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var path = context.Description.RelativePath?.TrimEnd('/');
        var method = context.Description.HttpMethod;

        if (operation.RequestBody?.Content?.TryGetValue("application/json", out var mediaType) == true)
        {
            var example = OpenApiRequestExamples.ForOperation(path, method);
            if (example is not null)
            {
                mediaType.Example = example;
            }
        }

        if (RequiresApiKey(path, method))
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(ApiKeyOptions.HeaderName, context.Document)] = []
            });
        }

        return Task.CompletedTask;
    }

    private static bool RequiresApiKey(string? path, string? method) =>
        method is "POST" or "PUT" or "DELETE"
        && path is "api/hotels" or "api/hotels/{id}";
}
