using RockTrack.Core.Validation.CategoryRules;

namespace RockTrack.Core.Validation
{
    // Strategy dispatcher: picks the rule for the rock's category. Knows no category by name.
    public sealed class RockCategoryValidator : IRockCategoryValidator
    {
        private readonly IReadOnlyDictionary<RockCategory, ICategoryRule> _rules;

        public RockCategoryValidator(IEnumerable<ICategoryRule> rules)
        {
            // Throws if two rules claim the same category
            _rules = rules.ToDictionary(rule => rule.Category);
        }

        public IReadOnlyDictionary<string, string[]> Validate(
            string title, RockCategory category, DateOnly dueDate, string? note, DateOnly today)
        {
            if (!_rules.TryGetValue(category, out var rule))
            {
                throw new InvalidOperationException($"No validation rule is registered for category '{category}'.");
            }

            return rule.Validate(new CategoryRuleContext(title, dueDate, note, today));
        }
    }
}
