using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RockTrack.Api.Development;
using RockTrack.Core.Repositories;
using RockTrack.Core.Services;
using RockTrack.Core.Validation;
using RockTrack.Infrastructure.Repositories;

namespace RockTrack.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
            builder.Services.AddProblemDetails();
            builder.Services.AddSingleton(TimeProvider.System);

            builder.Services.AddOpenApi();

            // Registration of services
            builder.Services.AddSingleton<IRockRepository, InMemoryRockRepository>();
            builder.Services.AddTransient<IRockCategoryValidator, RockCategoryValidator>(); // as Validator is stateless
            builder.Services.AddScoped<IRockService, RockService>();

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddHostedService<DevSeedHostedService>();
            }

            var app = builder.Build();

            app.UseExceptionHandler(); //first in pipeline to catch exceptions and return problem details
            app.UseStatusCodePages();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
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
