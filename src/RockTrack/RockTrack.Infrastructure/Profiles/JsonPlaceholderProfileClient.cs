using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RockTrack.Core.Profiles;

namespace RockTrack.Infrastructure.Profiles
{
    // Typed client: HttpClient comes from IHttpClientFactory with the base address, timeout and
    // resilience pipeline configured in Program.cs.
    public sealed class JsonPlaceholderProfileClient : IMemberProfileClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<JsonPlaceholderProfileClient> _logger;

        public JsonPlaceholderProfileClient(HttpClient httpClient, ILogger<JsonPlaceholderProfileClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MemberProfile?> GetProfileAsync(int memberId, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync($"users/{memberId}", cancellationToken);

            // Checked before EnsureSuccessStatusCode: a 404 is an answer ("no profile"), not a failure
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No profile found for member {MemberId}", memberId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var user = await response.Content.ReadFromJsonAsync<JsonPlaceholderUser>(cancellationToken)
                ?? throw new JsonException("Profile API returned an empty body.");

            // Member ID only - profile values are never logged
            _logger.LogDebug("Profile found for member {MemberId}", memberId);

            return new MemberProfile(user.Name, user.Username, user.Email);
        }
    }
}
