using System.Globalization;

namespace RockTrack.Core.Validation
{
    // Initial version: one if/else branch per category. To be refactored to the strategy pattern.
    public sealed class RockCategoryValidator : IRockCategoryValidator
    {
        public IReadOnlyDictionary<string, string[]> Validate(
            string title, RockCategory category, DateOnly dueDate, string? note, DateOnly today)
        {
            var errors = new Dictionary<string, string[]>();

            if (category == RockCategory.Revenue)
            {
                var (quarterStart, quarterEnd) = GetQuarter(today);

                if (dueDate < quarterStart || dueDate > quarterEnd)
                {
                    errors["dueDate"] = [$"dueDate must fall within the current quarter " +
                        $"({Format(quarterStart)} to {Format(quarterEnd)}) for revenue rocks."];
                }
            }
            else if (category == RockCategory.Health)
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

        // Calendar quarters: Jan–Mar, Apr–Jun, Jul–Sep, Oct–Dec
        private static (DateOnly Start, DateOnly End) GetQuarter(DateOnly date)
        {
            var firstMonth = (date.Month - 1) / 3 * 3 + 1;   // 1, 4, 7 or 10
            var start = new DateOnly(date.Year, firstMonth, 1);
            return (start, start.AddMonths(3).AddDays(-1));
        }

        private static string Format(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
