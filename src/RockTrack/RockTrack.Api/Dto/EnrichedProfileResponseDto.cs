using RockTrack.Core.Profiles;

namespace RockTrack.Api.Dto
{
    public sealed record EnrichedProfileResponseDto
    {
        public required int MemberId { get; init; }
        public required IReadOnlyList<RockResponseDto> Rocks { get; init; }
        public MemberProfileDto? Profile { get; init; }
        public required bool EnrichmentAvailable { get; init; }

        public static EnrichedProfileResponseDto FromResult(int memberId, EnrichedProfileResult result) => new()
        {
            MemberId = memberId,
            Rocks = result.Rocks.Select(RockResponseDto.FromRock).ToList(),
            Profile = result.Profile is null ? null : MemberProfileDto.FromProfile(result.Profile),
            EnrichmentAvailable = result.EnrichmentAvailable
        };
    }

    public sealed record MemberProfileDto
    {
        public required string Name { get; init; }
        public required string Username { get; init; }
        public required string Email { get; init; }

        public static MemberProfileDto FromProfile(MemberProfile profile) => new()
        {
            Name = profile.Name,
            Username = profile.Username,
            Email = profile.Email
        };
    }
}
