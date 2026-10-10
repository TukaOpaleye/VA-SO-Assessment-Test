using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using RockTrack.Core.Profiles;
using RockTrack.Infrastructure.Profiles;

namespace RockTrack.Api.Configuration
{
    public static class ProfileApiServiceCollectionExtensions
    {
        private static readonly TimeSpan MinPollyTimeout = TimeSpan.FromMilliseconds(10);

        // Third-party profile API: settings, startup checks, typed client and resilience pipeline
        public static IServiceCollection AddProfileApiClient(this IServiceCollection services, IConfiguration configuration)
        {
            // Defaults live in ProfileApiOptions; an optional "ProfileApi" section overrides them
            services.AddOptions<ProfileApiOptions>()
                .Bind(configuration.GetSection(ProfileApiOptions.SectionName))
                .Validate(options => Uri.TryCreate(options.BaseAddress, UriKind.Absolute, out _),
                    "ProfileApi:BaseAddress must be an absolute URL.")
                .Validate(options => options.RetryCount >= 3, "ProfileApi:RetryCount must be at least 3.")
                .Validate(options => options.AttemptTimeout > TimeSpan.Zero && options.BaseDelay > TimeSpan.Zero
                                     && options.TotalTimeout > TimeSpan.Zero && options.HttpClientTimeout > TimeSpan.Zero,
                    "ProfileApi timeouts and BaseDelay must be greater than zero.")
                // Polly only accepts timeouts of 10 ms to 24 h; it checks when the client is first created,
                // so a smaller value would be a 500 at request time instead of a startup failure
                .Validate(options => options.AttemptTimeout >= MinPollyTimeout && options.TotalTimeout >= MinPollyTimeout,
                    "ProfileApi:AttemptTimeout and TotalTimeout must be at least 10 ms.")
                .Validate(options => options.TotalTimeout > options.AttemptTimeout,
                    "ProfileApi:TotalTimeout must be greater than AttemptTimeout.")
                .Validate(options => options.HttpClientTimeout > options.TotalTimeout,
                    "ProfileApi:HttpClientTimeout must be greater than TotalTimeout.")
                .ValidateOnStart();

            // Typed client (not HttpClient injected directly)
            services.AddHttpClient<IMemberProfileClient, JsonPlaceholderProfileClient>((serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<ProfileApiOptions>>().Value;
                    client.BaseAddress = new Uri(options.BaseAddress);
                    client.Timeout = options.HttpClientTimeout; // covers all retries, so only a safety net
                })
                // Order: total timeout (outer) -> retry -> per-attempt timeout (inner), so a slow attempt is retried
                .AddResilienceHandler("profile-api", (pipeline, context) =>
                {
                    var options = context.ServiceProvider.GetRequiredService<IOptions<ProfileApiOptions>>().Value;
                    var logger = context.ServiceProvider.GetRequiredService<ILogger<JsonPlaceholderProfileClient>>();

                    pipeline
                        .AddTimeout(options.TotalTimeout)
                        // Default ShouldHandle: 5xx, 408, 429, HttpRequestException, attempt timeouts. Not 404.
                        .AddRetry(new HttpRetryStrategyOptions
                        {
                            MaxRetryAttempts = options.RetryCount,
                            BackoffType = DelayBackoffType.Exponential,
                            UseJitter = true,
                            Delay = options.BaseDelay,
                            OnRetry = args =>
                            {
                                var reason = args.Outcome.Exception is { } exception
                                    ? $"{exception.GetType().Name}: {exception.Message}"
                                    : $"{(int)args.Outcome.Result!.StatusCode} {args.Outcome.Result.StatusCode}";

                                // AttemptNumber is zero-based; +1 gives retry 1, 2, 3
                                logger.LogWarning("Profile API retry {AttemptNumber} in {RetryDelay} ms: {Reason}",
                                    args.AttemptNumber + 1, args.RetryDelay.TotalMilliseconds, reason);
                                return ValueTask.CompletedTask;
                            }
                        })
                        .AddTimeout(options.AttemptTimeout);
                });

            return services;
        }
    }
}
