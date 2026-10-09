using Microsoft.Extensions.Logging.Abstractions;
using RockTrack.Core;
using RockTrack.Core.Services;
using RockTrack.Core.Validation;
using RockTrack.Infrastructure.Repositories;
using RockTrack.Test;

namespace RockTrack.Tests
{
    public class RockServiceCreateTests
    {
        private InMemoryRockRepository _repository;
        private RockService _sut;

        [SetUp]
        public void Setup()
        {
            _repository = new InMemoryRockRepository();
            
            _sut = new RockService(_repository, RockCategoryValidatorTests.CreateValidator(), TimeProvider.System, NullLogger<RockService>.Instance);
        }

        [Test]
        public async Task CreateRock_ValidHealthRock_ReturnsSuccessAndStoresRock()
        {
            var result = await _sut.CreateRockAsync(1, "Run a half marathon", RockCategory.Health, new DateOnly(2026, 11, 1), null);

            var rock = result.Rock ?? throw new AssertionException("Expected the rock to be created.");
            var stored = await _repository.GetByMemberAsync(1, null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Errors, Is.Empty);
                Assert.That(stored.Select(r => r.Id), Is.EquivalentTo(new[] { rock.Id }));
            });
        }

        [Test]
        public async Task CreateRock_HealthTitleTooShort_ReturnsFailureAndStoresNothing()
        {
            var result = await _sut.CreateRockAsync(1, "Run a 10k", RockCategory.Health, new DateOnly(2026, 11, 1), null);

            var stored = await _repository.GetByMemberAsync(1, null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Rock, Is.Null);
                Assert.That(result.Errors.Keys, Is.EquivalentTo(new[] { "title" }));
            });
        }

        [Test]
        public async Task CreateRock_CareerWithoutNote_ReturnsNoteError()
        {
            // Also guards against title and note being swapped when the service calls the validator
            var result = await _sut.CreateRockAsync(1, "Get promoted", RockCategory.Career, new DateOnly(2027, 3, 1), null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Errors.Keys, Is.EquivalentTo(new[] { "note" }));
            }
        }

        // Same rock, two clocks, opposite results: proves the service reads "today" from the clock
        [TestCase("2026-10-09", false)] // Q4 2026: January is next quarter
        [TestCase("2027-01-05", true)]  // Q1 2027: January is the current quarter
        public async Task CreateRock_RevenueUsesClockForCurrentQuarter(string clockDate, bool expectedSuccess)
        {
            var clock = new FixedTimeProvider(new DateTimeOffset(DateTime.Parse(clockDate).AddHours(12), TimeSpan.Zero));
            var sut = new RockService(_repository, RockCategoryValidatorTests.CreateValidator(), clock, NullLogger<RockService>.Instance);

            var result = await sut.CreateRockAsync(1, "Close deal", RockCategory.Revenue, new DateOnly(2027, 1, 15), null);

            Assert.That(result.IsSuccess, Is.EqualTo(expectedSuccess));
        }

        private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => now;
        }
    }
}
