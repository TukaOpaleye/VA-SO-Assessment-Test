using System.Text.Json;
using System.Text.Json.Serialization;
using RockTrack.Core.Repositories;
using RockTrack.Core.Services;
using RockTrack.Infrastructure.Repositories;

namespace RockTrack.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)));  //enum conversions
            builder.Services.AddProblemDetails();

            builder.Services.AddOpenApi();

            // Registration of services
            builder.Services.AddSingleton<IRockRepository, InMemoryRockRepository>();
            builder.Services.AddScoped<IRockService, RockService>();

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
    }
}
