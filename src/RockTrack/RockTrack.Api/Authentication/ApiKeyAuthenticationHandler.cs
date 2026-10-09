using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace RockTrack.Api.Authentication
{
    // Authenticates requests carrying a valid X-Api-Key header
    public sealed class ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var provided = Request.Headers[ApiKeyAuthenticationOptions.HeaderName].ToString();

            if (string.IsNullOrEmpty(provided))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!KeysMatch(provided, Options.ApiKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));   // the key itself is never logged
            }

            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "api-client")], Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            // Empty 401: UseStatusCodePages turns it into ProblemDetails (with correlationId)
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.Headers.WWWAuthenticate = Scheme.Name;
            return Task.CompletedTask;
        }

        // Constant-time comparison of equal-length hashes: no timing or length leak
        private static bool KeysMatch(string provided, string expected) =>
            CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(provided)),
                SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
    }
}
