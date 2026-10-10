using System.Diagnostics.CodeAnalysis;

namespace RockTrack.Core.Services
{
    public enum UpdateRockStatusOutcome
    {
        Updated,
        NotFound,
        InvalidTransition
    }

    // Outcome of a status change. Rock is set for Updated and InvalidTransition, null for NotFound.
    public sealed class UpdateRockStatusResult
    {
        private UpdateRockStatusResult(UpdateRockStatusOutcome outcome, Rock? rock)
        {
            Outcome = outcome;
            Rock = rock;
        }

        public UpdateRockStatusOutcome Outcome { get; }
        public Rock? Rock { get; }

        [MemberNotNullWhen(true, nameof(Rock))]
        public bool IsSuccess => Outcome == UpdateRockStatusOutcome.Updated;

        [MemberNotNullWhen(true, nameof(Rock))]
        public bool IsInvalidTransition => Outcome == UpdateRockStatusOutcome.InvalidTransition;

        public static UpdateRockStatusResult Updated(Rock rock) => new(UpdateRockStatusOutcome.Updated, rock);
        public static UpdateRockStatusResult NotFound() => new(UpdateRockStatusOutcome.NotFound, null);
        public static UpdateRockStatusResult InvalidTransition(Rock rock) => new(UpdateRockStatusOutcome.InvalidTransition, rock);
    }
}
