namespace RockTrack.Infrastructure.Profiles
{
    // Shape of jsonplaceholder's /users/{id} response; only the fields we use, the rest is ignored.
    // required: a response missing one of them fails deserialization and counts as a failed call.
    internal sealed class JsonPlaceholderUser
    {
        public required string Name { get; init; }
        public required string Username { get; init; }
        public required string Email { get; init; }
    }
}
