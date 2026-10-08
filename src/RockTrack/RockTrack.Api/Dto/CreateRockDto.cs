using System.ComponentModel.DataAnnotations;
using RockTrack.Api.Validation;
using RockTrack.Core;

namespace RockTrack.Api.Dto
{
    public sealed record CreateRockDto
    {
        [Required(ErrorMessage = "title must not be empty or whitespace.")]
        public required string Title { get; init; }

        public required RockCategory Category { get; init; }

        [NotInPast(ErrorMessage = "dueDate must not be in the past.")]
        public required DateOnly DueDate { get; init; }

        public string? Note { get; init; }
    }
}
