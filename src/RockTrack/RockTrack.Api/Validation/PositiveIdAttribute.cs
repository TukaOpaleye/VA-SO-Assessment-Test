using System.ComponentModel.DataAnnotations;

namespace RockTrack.Api.Validation
{
    // Rule for MemberID: must be a positive number
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class PositiveIdAttribute : RangeAttribute
    {
        public PositiveIdAttribute() : base(1, int.MaxValue)
        {
            ErrorMessage = "{0} must be a positive number.";
        }
    }
}
