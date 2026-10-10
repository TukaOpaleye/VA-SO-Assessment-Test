namespace RockTrack.Infrastructure.Profiles
{
    // Defaults live here; an optional "ProfileApi" config section (or env vars, e.g. ProfileApi__RetryCount)
    // overrides any of them. Checked at startup in Program.cs.
    public sealed class ProfileApiOptions
    {
        public const string SectionName = "ProfileApi";

        // External API; trailing slash so relative paths ("users/1") append to it
        public string BaseAddress { get; set; } = "https://jsonplaceholder.typicode.com/";

        // Per HTTP call; a slow call is cut off and retried
        public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(2);

        // Spec minimum is 3
        public int RetryCount { get; set; } = 3;

        // First wait; then ~1 s, ~2 s (exponential + jitter)
        public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        // All attempts + waits; worst case ~11.5 s, so every retry fits
        public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromSeconds(15);

        // Safety net only; covers all retries, so it must stay above TotalTimeout
        public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
