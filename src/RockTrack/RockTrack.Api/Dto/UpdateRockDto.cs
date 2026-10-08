using RockTrack.Core;

namespace RockTrack.Api.Dto
{
    public sealed record UpdateRockDto
    {
        public required RockStatus Status { get; init; }
    }
}
