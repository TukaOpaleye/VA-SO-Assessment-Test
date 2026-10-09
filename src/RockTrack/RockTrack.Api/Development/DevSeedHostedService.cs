using System.Text.Json;
using Microsoft.Extensions.Options;
using RockTrack.Core;
using RockTrack.Core.Repositories;

namespace RockTrack.Api.Development
{
    // Development only: loads rocks from a JSON file into the in-memory store at startup.
    // Uses the public Rock API only - the domain has no knowledge of seeding.
    public sealed class DevSeedHostedService(
        IRockRepository repository,
        IHostEnvironment environment,
        IConfiguration configuration,
        IOptions<Microsoft.AspNetCore.Mvc.JsonOptions> jsonOptions,
        ILogger<DevSeedHostedService> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var relativePath = configuration["Seed:File"];
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            var path = Path.Combine(environment.ContentRootPath, relativePath);
            if (!File.Exists(path))
            {
                logger.LogWarning("Seed file {Path} not found; skipping seeding", path);
                return;
            }

            List<SeedRock>? items;
            try
            {
                await using var stream = File.OpenRead(path);
                items = await JsonSerializer.DeserializeAsync<List<SeedRock>>(
                    stream, jsonOptions.Value.JsonSerializerOptions, cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Seed file {Path} is invalid; skipping seeding", path);
                return;
            }

            foreach (var item in items ?? [])
            {
                var rock = new Rock(item.MemberId, item.Title, item.Category, item.DueDate, item.Note);
                if (item.Status != RockStatus.Pending)
                {
                    rock.TryTransitionTo(item.Status);
                }

                await repository.AddAsync(rock);
                logger.LogInformation("Seeded rock {RockId} for member {MemberId} ({Status})",
                    rock.Id, rock.MemberId, rock.Status);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private sealed record SeedRock(
            int MemberId,
            string Title,
            RockCategory Category,
            DateOnly DueDate,
            string? Note = null,
            RockStatus Status = RockStatus.Pending);
    }
}
