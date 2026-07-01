using FluentValidation;
using HotelSearch.Api.Configuration;
using HotelSearch.Api.Endpoints;
using HotelSearch.Api.Infrastructure;
using HotelSearch.Api.OpenApi;
using HotelSearch.Api.Validation;
using HotelSearch.Application;
using HotelSearch.Infrastructure;
using HotelSearch.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection(ApiKeyOptions.SectionName));
builder.Services.AddValidatorsFromAssemblyContaining<CreateHotelRequestValidator>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hotel Search API",
        Version = "v1",
        Description = "REST API for hotel CRUD and natural-language search (Lemax take-home PoC)."
    });

    options.OperationFilter<SwaggerRequestExamplesFilter>();
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml"));
});
builder.Services.AddOpenApi(OpenApiDocumentConfiguration.Configure);
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
    options.CombineLogs = true;
});
builder.Services.AddSingleton<FluentValidationFilter>();
builder.Services.AddSingleton<ApiKeyAuthorizationFilter>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/health"),
    branch => branch.UseHttpLogging());

if (app.Environment.IsDevelopment())
{
    await DatabaseInitializer.ApplyMigrationsAsync(app.Services);

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Search API v1");
        options.RoutePrefix = "swagger";
    });

    app.MapOpenApi();
}

var apiKeyOptions = app.Services.GetRequiredService<IOptions<ApiKeyOptions>>().Value;
ApiKeyStartupValidator.EnsureProductionWriteKeyConfigured(app.Environment, apiKeyOptions);
StartupLogger.LogApplicationStarted(app, apiKeyOptions);

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
})
.WithName("HealthCheck")
.WithTags("Health")
.ExcludeFromDescription();

app.MapHotelEndpoints();

app.Run();

public partial class Program;
