using RockTrack.Core;
using RockTrack.Core.Services;
using RockTrack.Core.Validation;
using RockTrack.Infrastructure.Repositories;

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
            
            _sut = new RockService(_repository, new RockCategoryValidator(), TimeProvider.System);
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
    }
}
