namespace RockTrack.Core.Profiles
{
    // Port to the external profile API; implemented in Infrastructure.
    public interface IMemberProfileClient
    {
        // Returns null when the member has no profile (external 404).
        // Throws when the call still fails after all retries, or when cancellationToken is cancelled.
        Task<MemberProfile?> GetProfileAsync(int memberId, CancellationToken cancellationToken);
    }
}
