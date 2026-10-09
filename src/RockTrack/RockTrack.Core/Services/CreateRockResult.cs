using System.Diagnostics.CodeAnalysis;

namespace RockTrack.Core.Services
{
    // Outcome of creating a rock: either the created rock, or the validation errors. Never both.
    public sealed class CreateRockResult
    {
        private CreateRockResult(Rock? rock, IReadOnlyDictionary<string, string[]> errors)
        {
            Rock = rock;
            Errors = errors;
        }

        public Rock? Rock { get; }
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        [MemberNotNullWhen(true, nameof(Rock))]
        public bool IsSuccess => Rock is not null;

        public static CreateRockResult Success(Rock rock) =>
            new(rock, new Dictionary<string, string[]>());

        public static CreateRockResult Failure(IReadOnlyDictionary<string, string[]> errors) =>
            errors.Count > 0
                ? new(null, errors)
                : throw new ArgumentException("A failure needs at least one error.", nameof(errors));
    }
}
