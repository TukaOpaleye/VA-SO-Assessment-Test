namespace RockTrack.Core.Validation.CategoryRules
{
    // Strategy: one implementation per category. Adding a category = adding a class.
    public interface ICategoryRule
    {
        RockCategory Category { get; }
        IReadOnlyDictionary<string, string[]> Validate(CategoryRuleContext context);
    }
}
