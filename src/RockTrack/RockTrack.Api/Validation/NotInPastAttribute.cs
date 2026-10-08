using System.ComponentModel.DataAnnotations;

namespace RockTrack.Api.Validation
{
    // Fails when a DateOnly is before today (UTC). Today itself is valid.
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class NotInPastAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var timeProvider = validationContext.GetService<TimeProvider>() ?? TimeProvider.System;
            var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

            return value is DateOnly date && date < today
                ? new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must not be in the past.",
                    [validationContext.MemberName!])
                : ValidationResult.Success;
        }
    }
}
