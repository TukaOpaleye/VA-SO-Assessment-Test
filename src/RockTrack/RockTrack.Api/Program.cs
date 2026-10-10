using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RockTrack.Api.Authentication;
using RockTrack.Api.Configuration;
using RockTrack.Api.Development;
using RockTrack.Api.Middleware;
using RockTrack.Core.Repositories;
using RockTrack.Core.Services;
using RockTrack.Core.Validation;
using RockTrack.Core.Validation.CategoryRules;
using RockTrack.Infrastructure.Repositories;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace RockTrack.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Bootstrap logger: startup failures are logged as JSON, before configuration is read
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(new CompactJsonFormatter())
                .CreateBootstrapLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Serilog replaces the default logging; levels, sinks and formatter come from the "Serilog" config section
                builder.Services.AddSerilog((services, configuration) => configuration
                    .ReadFrom.Configuration(builder.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext());

                // Add services to the container.

                builder.Services.AddControllers(options =>
                        // Explicit [Required] / C# required do the work; avoids a misleading "request field is required" error
                        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
                    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)))  //enum conversions
                    .ConfigureApiBehaviorOptions(options =>
                        options.InvalidModelStateResponseFactory = context =>
                        {
                            // Replace the JSON converter's technical enum messages with the allowed values
                            ReplaceErrors(context.ModelState, "$.category", "category must be one of: revenue, health, career, other.");
                            ReplaceErrors(context.ModelState, "$.status", "status must be one of: pending, completed, missed.");

                            var problemDetails = context.HttpContext.RequestServices
                                .GetRequiredService<ProblemDetailsFactory>()
                                .CreateValidationProblemDetails(context.HttpContext, context.ModelState);

                            return new BadRequestObjectResult(problemDetails) { ContentTypes = { "application/problem+json" } };
                        });

                // Every error body carries the request's correlation ID next to traceId
                builder.Services.AddProblemDetails(options =>
                    options.CustomizeProblemDetails = context =>
                    {
                        if (context.HttpContext.Items[CorrelationIdMiddleware.ItemKey] is string correlationId)
                        {
                            context.ProblemDetails.Extensions["correlationId"] = correlationId;
                        }
                    });
                builder.Services.AddSingleton(TimeProvider.System);

                builder.Services.AddOpenApi();

                // Registration of services
                builder.Services.AddSingleton<IRockRepository, InMemoryRockRepository>();

                // Category rules for  IEnumerable<ICategoryRule>
                builder.Services.AddTransient<ICategoryRule, RevenueRule>();
                builder.Services.AddTransient<ICategoryRule, HealthRule>();
                builder.Services.AddTransient<ICategoryRule, CareerRule>();
                builder.Services.AddTransient<ICategoryRule, OtherRule>();
                builder.Services.AddTransient<IRockCategoryValidator, RockCategoryValidator>();

                builder.Services.AddScoped<IRockService, RockService>();

                // API key authentication: the key comes from configuration ("ApiKeyAuthentication:ApiKey").
                builder.Services.AddAuthentication(ApiKeyAuthenticationOptions.SchemeName)
                    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.SchemeName,
                        options => builder.Configuration.GetSection("ApiKeyAuthentication").Bind(options));

                // Fail fast: the app won't start without a configured key
                builder.Services.AddOptions<ApiKeyAuthenticationOptions>(ApiKeyAuthenticationOptions.SchemeName)
                    .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "ApiKeyAuthentication:ApiKey must be configured.")
                    .ValidateOnStart();

                builder.Services.AddAuthorization();

                // Third-party profile API: typed client, settings and resilience pipeline
                builder.Services.AddProfileApiClient(builder.Configuration);

                if (builder.Environment.IsDevelopment())
                {
                    builder.Services.AddHostedService<DevSeedHostedService>();
                }

                var app = builder.Build();

                app.UseMiddleware<CorrelationIdMiddleware>(); // first: every log entry below carries the correlation ID

                // One log entry per request with status code and duration; level follows the outcome
                app.UseSerilogRequestLogging(options =>
                {
                    options.GetLevel = (httpContext, elapsed, ex) =>
                        ex is not null || httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error
                        : httpContext.Response.StatusCode >= 400 ? LogEventLevel.Warning
                        : LogEventLevel.Information;

                    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                        diagnosticContext.Set("RequestQuery", httpContext.Request.QueryString.Value);
                });

                app.UseExceptionHandler(); // catches exceptions and returns problem details
                app.UseStatusCodePages();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi().RequireAuthorization(); // protected like every other endpoint
                }

                app.UseHttpsRedirection();

                app.UseAuthentication(); // reads X-Api-Key
                app.UseAuthorization();


                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "RockTrack failed to start");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ReplaceErrors(ModelStateDictionary modelState, string key, string message)
        {
            if (modelState.TryGetValue(key, out var entry) && entry.Errors.Count > 0)
            {
                entry.Errors.Clear();
                entry.Errors.Add(message);
            }
        }
    }
}
