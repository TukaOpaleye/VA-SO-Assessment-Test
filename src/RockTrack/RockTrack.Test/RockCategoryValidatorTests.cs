using RockTrack.Core;
using RockTrack.Core.Validation;

namespace RockTrack.Test
{
    public class RockCategoryValidatorTests
    {
        private static readonly DateOnly Today = new(2026, 10, 9);

        private IRockCategoryValidator _sut = null!;

        [SetUp]
        public void Setup()
        {
            _sut = CreateValidator();
        }

        // The only line that changes when the strategy pattern arrives
        private static IRockCategoryValidator CreateValidator() => new RockCategoryValidator();

        //** --- HEALTH category : title at least 10 characters ---

        [TestCase("1234567890")]          // 10 chars
        [TestCase("Run a half marathon")]
        public void Health_TitleAtLeast10Characters_IsValid(string title)
        {
            var errors = _sut.Validate(title, RockCategory.Health, Today, null, Today);

            Assert.That(errors, Is.Empty);
        }


        public void Health_TitleShorterThan10Characters_ReturnsTitleError()
        {
            const string shortText = "12345678";
            var errors = _sut.Validate(shortText, RockCategory.Health, Today, null, Today);

            Assert.That(errors.Keys, Is.EquivalentTo(new[] { "title" }));
        }

        [Test]
        public void Health_WithoutNote_IsValid()
        {
            var errors = _sut.Validate("Run a 10km", RockCategory.Health, Today, null, Today);

            Assert.That(errors, Is.Empty);
        }

        // --- The Health rule applies only to Health ---

        [TestCase(RockCategory.Revenue)]
        [TestCase(RockCategory.Career)]
        [TestCase(RockCategory.Other)]
        public void NonHealthCategory_ShortTitle_HasNoTitleError(RockCategory category)
        {
            var errors = _sut.Validate("Run a 10k", category, Today, "a note", Today);

            Assert.That(errors.ContainsKey("title"), Is.False);
        }

        //** -- CAREER Category: note required ---

        [Test]
        public void Career_WithNote_IsValid()
        {
            var errors = _sut.Validate("promoted", RockCategory.Career, Today, "engineering", Today);

            Assert.That(errors, Is.Empty);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Career_MissingNote_ReturnsNoteError(string? note)
        {
            var errors = _sut.Validate("promoted", RockCategory.Career, Today, note, Today);

            Assert.That(errors.Keys, Is.EquivalentTo(new[] { "note" }));
        }

        [Test]
        public void Career_ShortTitle_HasNoTitleError()
        {
            // The Health title rule must not leak into Career
            var errors = _sut.Validate("Promo", RockCategory.Career, Today, "engineering", Today);

            Assert.That(errors, Is.Empty);
        }

        // --- The Career rule applies only to Career ---

        [TestCase(RockCategory.Revenue)]
        [TestCase(RockCategory.Health)]
        [TestCase(RockCategory.Other)]
        public void NonCareerCategory_WithoutNote_HasNoNoteError(RockCategory category)
        {
            var errors = _sut.Validate("Run a half marathon", category, Today, null, Today);

            Assert.That(errors.ContainsKey("note"), Is.False);
        }
    }
}
