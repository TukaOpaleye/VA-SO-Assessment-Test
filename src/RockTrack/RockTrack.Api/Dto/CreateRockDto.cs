using RockTrack.Core;

namespace RockTrack.Api.Dto
{
    public sealed record CreateRockDto
    {
        public required string Title { get; init; }
        public required RockCategory Category { get; init; }
        public required DateOnly DueDate { get; init; }
        public string? Note { get; init; }
    }
}
