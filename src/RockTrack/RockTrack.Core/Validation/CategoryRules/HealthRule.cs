namespace RockTrack.Core.Validation.CategoryRules
{
    // Title must be at least 10 characters (trimmed, matching how Rock stores it)
    public sealed class HealthRule : ICategoryRule
    {
        public RockCategory Category => RockCategory.Health;

        public IReadOnlyDictionary<string, string[]> Validate(CategoryRuleContext context)
        {
            var errors = new Dictionary<string, string[]>();

            if (context.Title.Trim().Length < 10)
            {
                errors["title"] = ["title must be at least 10 characters for health rocks."];
            }

            return errors;
        }
    }
}
