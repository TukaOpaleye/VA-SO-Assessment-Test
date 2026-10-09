namespace RockTrack.Core.Validation.CategoryRules
{
    // No additional rules beyond the base validation. Explicit, so every category has a rule.
    public sealed class OtherRule : ICategoryRule
    {
        public RockCategory Category => RockCategory.Other;

        public IReadOnlyDictionary<string, string[]> Validate(CategoryRuleContext context) =>
            new Dictionary<string, string[]>();
    }
}
