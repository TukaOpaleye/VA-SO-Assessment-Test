using Microsoft.Extensions.Logging.Abstractions;
using RockTrack.Core;
using RockTrack.Core.Profiles;
using RockTrack.Core.Services;

namespace RockTrack.Tests
{
    [TestFixture]
    public class MemberProfileServiceTests
    {
        private const int MemberId = 1;

        private static readonly MemberProfile Leanne = new("Leanne Graham", "Bret", "Sincere@april.biz");

        private static MemberProfileService CreateService(IMemberProfileClient client, params Rock[] rocks) =>
            new(new FakeRockService(rocks), client, NullLogger<MemberProfileService>.Instance);

        private static Rock NewRock() =>
            new(MemberId, "Close 3 deals", RockCategory.Other, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), null);

        [Test]
        public async Task ProfileFound_ReturnsRocksAndProfile_EnrichmentAvailable()
        {
            var rock = NewRock();
            var sut = CreateService(new FakeProfileClient((_, _) => Task.FromResult<MemberProfile?>(Leanne)), rock);

            var result = await sut.GetEnrichedProfileAsync(MemberId, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Rocks, Is.EqualTo(new[] { rock }));
                Assert.That(result.Profile, Is.EqualTo(Leanne));
                Assert.That(result.EnrichmentAvailable, Is.True);
            }
        }

        [Test]
        public async Task ProfileNotFound_ReturnsNullProfile_EnrichmentStillAvailable()
        {
            // External 404: the client returns null. "We checked; no profile", not a failure.
            var sut = CreateService(new FakeProfileClient((_, _) => Task.FromResult<MemberProfile?>(null)), NewRock());

            var result = await sut.GetEnrichedProfileAsync(MemberId, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Rocks, Has.Count.EqualTo(1));
                Assert.That(result.Profile, Is.Null);
                Assert.That(result.EnrichmentAvailable, Is.True);
            }
        }

        [Test]
        public async Task NoRocks_StillCallsProfileApi()
        {
            var client = new FakeProfileClient((_, _) => Task.FromResult<MemberProfile?>(Leanne));
            var sut = CreateService(client);

            var result = await sut.GetEnrichedProfileAsync(MemberId, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Rocks, Is.Empty);
                Assert.That(result.Profile, Is.EqualTo(Leanne));
                Assert.That(client.Calls, Is.EqualTo(1));
            }
        }

        // What the client throws once the retries are used up. TaskCanceledException with an
        // uncancelled caller token is how an HttpClient timeout surfaces: it must fall back, not rethrow.
        private static IEnumerable<Exception> FinalFailures()
        {
            yield return new HttpRequestException("503 Service Unavailable");
            yield return new TaskCanceledException("Timed out");
            yield return new System.Text.Json.JsonException("Invalid JSON");
        }

        [TestCaseSource(nameof(FinalFailures))]
        public async Task ClientFailsAfterRetries_ReturnsRocksWithEnrichmentUnavailable(Exception failure)
        {
            var rock = NewRock();
            var sut = CreateService(new FakeProfileClient((_, _) => Task.FromException<MemberProfile?>(failure)), rock);

            var result = await sut.GetEnrichedProfileAsync(MemberId, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Rocks, Is.EqualTo(new[] { rock }));
                Assert.That(result.Profile, Is.Null);
                Assert.That(result.EnrichmentAvailable, Is.False);
            }
        }

        [Test]
        public void CallerCancels_ExceptionIsRethrown_NoFallback()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Behaves like HttpClient: honours the caller's token
            var sut = CreateService(new FakeProfileClient((_, token) =>
            {
                token.ThrowIfCancellationRequested();
                return Task.FromResult<MemberProfile?>(Leanne);
            }), NewRock());

            // CatchAsync also accepts derived types such as TaskCanceledException
            Assert.CatchAsync<OperationCanceledException>(() => sut.GetEnrichedProfileAsync(MemberId, cts.Token));
        }

        private sealed class FakeProfileClient(Func<int, CancellationToken, Task<MemberProfile?>> behaviour)
            : IMemberProfileClient
        {
            public int Calls { get; private set; }

            public Task<MemberProfile?> GetProfileAsync(int memberId, CancellationToken cancellationToken)
            {
                Calls++;
                return behaviour(memberId, cancellationToken);
            }
        }

        private sealed class FakeRockService(IReadOnlyList<Rock> rocks) : IRockService
        {
            public Task<IReadOnlyList<Rock>> GetRocksAsync(int memberId, RockStatus? status) => Task.FromResult(rocks);

            public Task<CreateRockResult> CreateRockAsync(int memberId, string title, RockCategory category, DateOnly dueDate, string? note) =>
                throw new NotSupportedException();

            public Task<UpdateRockStatusResult> UpdateRockStatusAsync(int memberId, Guid rockId, RockStatus newStatus) =>
                throw new NotSupportedException();
        }
    }
}
