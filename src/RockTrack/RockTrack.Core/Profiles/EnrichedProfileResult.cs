namespace RockTrack.Core.Profiles
{
    // Rocks combined with profile data.
    // Profile is null when the member has no profile (EnrichmentAvailable true)
    // or when the profile API failed after all retries (EnrichmentAvailable false).
    public sealed record EnrichedProfileResult(IReadOnlyList<Rock> Rocks, MemberProfile? Profile, bool EnrichmentAvailable);
}
