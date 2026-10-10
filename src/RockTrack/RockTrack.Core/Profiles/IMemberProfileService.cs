namespace RockTrack.Core.Profiles
{
    public interface IMemberProfileService
    {
        Task<EnrichedProfileResult> GetEnrichedProfileAsync(int memberId, CancellationToken cancellationToken);
    }
}
