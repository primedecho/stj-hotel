using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelSearch.Api.OpenApi;

internal sealed class SwaggerRequestExamplesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content is null
            || !operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            return;
        }

        var example = OpenApiRequestExamples.ForOperation(
            context.ApiDescription.RelativePath?.TrimEnd('/'),
            context.ApiDescription.HttpMethod);

        if (example is not null)
        {
            mediaType.Example = example;
        }
    }
}
