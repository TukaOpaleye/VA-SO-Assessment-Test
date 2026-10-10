namespace RockTrack.Core.Profiles
{
    // Profile data from the external API. Only the fields the API returns to its callers (data minimisation).
    public sealed record MemberProfile(string Name, string Username, string Email);
}
