using Microsoft.Extensions.Logging;
using RockTrack.Core.Services;

namespace RockTrack.Core.Profiles
{
    public sealed class MemberProfileService : IMemberProfileService
    {
        private readonly IRockService _rockService;
        private readonly IMemberProfileClient _profileClient;
        private readonly ILogger<MemberProfileService> _logger;

        public MemberProfileService(IRockService rockService, IMemberProfileClient profileClient,
            ILogger<MemberProfileService> logger)
        {
            _rockService = rockService;
            _profileClient = profileClient;
            _logger = logger;
        }

        public async Task<EnrichedProfileResult> GetEnrichedProfileAsync(int memberId, CancellationToken cancellationToken)
        {
            // In-memory lookup: no I/O to cancel, so no token
            var rocks = await _rockService.GetRocksAsync(memberId, null);

            try
            {
                // null = no profile (external 404): still a successful enrichment
                var profile = await _profileClient.GetProfileAsync(memberId, cancellationToken);
                return new EnrichedProfileResult(rocks, profile, EnrichmentAvailable: true);
            }
            // Any failure after the retries falls back to Rocks only (never a 500).
            // Not when the caller cancelled: a timeout also throws a cancellation-type exception,
            // so the check is on the caller's token, not the exception type.
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Profile enrichment unavailable for member {MemberId}; returning Rocks only", memberId);
                return new EnrichedProfileResult(rocks, Profile: null, EnrichmentAvailable: false);
            }
        }
    }
}
