namespace RockTrack.Core.Validation
{
    // Category-specific rules applied when a rock is created.
    // Returns errors keyed by field name; an empty dictionary means the rock is valid.
    public interface IRockCategoryValidator
    {
        IReadOnlyDictionary<string, string[]> Validate(
            string title, RockCategory category, DateOnly dueDate, string? note, DateOnly today);
    }
}
