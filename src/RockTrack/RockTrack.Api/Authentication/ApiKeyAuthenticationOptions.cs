using Microsoft.AspNetCore.Authentication;

namespace RockTrack.Api.Authentication
{
    public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string SchemeName = "ApiKey";
        public const string HeaderName = "X-Api-Key";

        // Read from configuration ("ApiKeyAuthentication:ApiKey"); never hardcoded
        public string ApiKey { get; set; } = string.Empty;
    }
}
