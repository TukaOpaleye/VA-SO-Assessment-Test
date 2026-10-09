namespace RockTrack.Core.Validation.CategoryRules
{
    // Everything a category rule may look at. Adding a field here doesn't change any rule's signature.
    public sealed record CategoryRuleContext(string Title, DateOnly DueDate, string? Note, DateOnly Today);
}
