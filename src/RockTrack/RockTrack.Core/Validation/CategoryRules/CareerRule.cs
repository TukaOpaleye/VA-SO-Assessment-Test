namespace RockTrack.Core.Validation.CategoryRules
{
    // A note explaining why this matters is required; whitespace-only doesn't count
    public sealed class CareerRule : ICategoryRule
    {
        public RockCategory Category => RockCategory.Career;

        public IReadOnlyDictionary<string, string[]> Validate(CategoryRuleContext context)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(context.Note))
            {
                errors["note"] = ["note is required for career rocks, explaining why this matters."];
            }

            return errors;
        }
    }
}
