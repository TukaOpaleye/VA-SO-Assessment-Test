using RockTrack.Core;

namespace RockTrack.Api.Dto
{
    public sealed record RockResponseDto
    {
        public required Guid Id { get; init; }
        public required int MemberId { get; init; }
        public required string Title { get; init; }
        public required RockCategory Category { get; init; }
        public required DateOnly DueDate { get; init; }
        public string? Note { get; init; }
        public required RockStatus Status { get; init; }

        public static RockResponseDto FromRock(Rock rock) => new()
        {
            Id = rock.Id,
            MemberId = rock.MemberId,
            Title = rock.Title,
            Category = rock.Category,
            DueDate = rock.DueDate,
            Note = rock.Note,
            Status = rock.Status
        };
    }
}
