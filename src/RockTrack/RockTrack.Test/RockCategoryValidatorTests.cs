using RockTrack.Core;
using RockTrack.Core.Validation;
using RockTrack.Core.Validation.CategoryRules;

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

        // All the category rules, as registered in Program.cs
        internal static IRockCategoryValidator CreateValidator() =>
            new RockCategoryValidator([new RevenueRule(), new HealthRule(), new CareerRule(), new OtherRule()]);

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

        //** -- REVENUE Category: due date within the current quarter ---
        // Today = 2026-10-09 → Q4: 2026-10-01 to 2026-12-31

        [TestCase("2026-10-01")] // first day of quarter
        [TestCase("2026-10-09")] // today
        [TestCase("2026-12-31")] // last day of quarter
        public void Revenue_DueDateInCurrentQuarter_IsValid(string dueDate)
        {
            var errors = _sut.Validate("Close deal", RockCategory.Revenue, DateOnly.Parse(dueDate), null, Today);

            Assert.That(errors, Is.Empty);
        }

        [TestCase("2026-09-30")] // day before quarter
        [TestCase("2027-01-01")] // day after quarter (across the year boundary)
        [TestCase("2027-03-31")] // next quarter
        public void Revenue_DueDateOutsideCurrentQuarter_ReturnsDueDateError(string dueDate)
        {
            var errors = _sut.Validate("Close deal", RockCategory.Revenue, DateOnly.Parse(dueDate), null, Today);

            Assert.That(errors.Keys, Is.EquivalentTo(new[] { "dueDate" }));
        }

        // The quarter is worked out from "today", for every quarter
        // Heuristic: IF today is A, the due Date B must be in the same quarter as A (expect true) otherwise it's an error (expect false)
        [TestCase("2026-01-01", "2026-03-31", true)]  // Q1, first day of quarter: last day valid
        [TestCase("2026-02-15", "2026-04-01", false)] // Q1: next quarter invalid
        [TestCase("2026-06-30", "2026-04-01", true)]  // Q2, last day: first day of quarter valid - note that the dueDate is before today - this is OK as the spec on speak about due date being in teh quarter. Another rule from teh UI catches this edge case  
        public void Revenue_QuarterIsWorkedOutFromToday(string today, string dueDate, bool expectedValid)
        {
            var errors = _sut.Validate("Close deal", RockCategory.Revenue, DateOnly.Parse(dueDate), null, DateOnly.Parse(today));

            Assert.That(errors.Count == 0, Is.EqualTo(expectedValid));
        }

        // --- The Revenue rule applies only to Revenue ---

        [TestCase(RockCategory.Health)]
        [TestCase(RockCategory.Career)]
        [TestCase(RockCategory.Other)]
        public void NonRevenueCategory_DueDateOutsideQuarter_HasNoDueDateError(RockCategory category)
        {
            var errors = _sut.Validate("Run a half marathon", category, new DateOnly(2027, 6, 1), "a note", Today);

            Assert.That(errors.ContainsKey("dueDate"), Is.False);
        }

        //** -- Dispatcher (strategy selection) ---

        [Test]
        public void Validate_CategoryWithNoRule_Throws()
        {
            var sut = new RockCategoryValidator([new HealthRule()]);

            Assert.Throws<InvalidOperationException>(() =>
                sut.Validate("Close deal", RockCategory.Revenue, Today, null, Today)); //RockCategory.Revenue <!=>  HealthRule()
        }

        [Test]
        public void Constructor_TwoRulesForSameCategory_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new RockCategoryValidator([new HealthRule(), new HealthRule()]));
        }

        [Test]
        public void EveryCategory_HasARule()
        {
            // Catches a new enum value added without its rule class (checks CreateValidator, not Program.cs)
            foreach (var category in Enum.GetValues<RockCategory>())
            {
                Assert.DoesNotThrow(() => _sut.Validate("A valid rock title", category, Today, "a note", Today), $"{category}");
            }
        }
    }
}
