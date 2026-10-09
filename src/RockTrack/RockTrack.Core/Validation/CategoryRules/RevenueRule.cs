using System.Globalization;

namespace RockTrack.Core.Validation.CategoryRules
{
    // Due date must fall within the current calendar quarter
    public sealed class RevenueRule : ICategoryRule
    {
        public RockCategory Category => RockCategory.Revenue;

        public IReadOnlyDictionary<string, string[]> Validate(CategoryRuleContext context)
        {
            var errors = new Dictionary<string, string[]>();
            var (quarterStart, quarterEnd) = GetQuarter(context.Today);

            if (context.DueDate < quarterStart || context.DueDate > quarterEnd)
            {
                errors["dueDate"] = [$"dueDate must fall within the current quarter " +
                    $"({Format(quarterStart)} to {Format(quarterEnd)}) for revenue rocks."];
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
