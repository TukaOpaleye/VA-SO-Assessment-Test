namespace RockTrack.Core.Validation
{
    // Initial version: one if/else branch per category. To be refactored to the strategy pattern.
    public sealed class RockCategoryValidator : IRockCategoryValidator
    {
        public IReadOnlyDictionary<string, string[]> Validate(
            string title, RockCategory category, DateOnly dueDate, string? note, DateOnly today)
        {
            var errors = new Dictionary<string, string[]>();

            if (category == RockCategory.Health)
            {
                if (title.Trim().Length < 10)
                {
                    errors["title"] = ["title must be at least 10 characters for health rocks."];
                }
            }
            else if (category == RockCategory.Career)
            {
                // Required: whitespace-only doesn't explain anything
                if (string.IsNullOrWhiteSpace(note))
                {
                    errors["note"] = ["note is required for career rocks, explaining why this matters."];
                }
            }

            return errors;
        }
    }
}
