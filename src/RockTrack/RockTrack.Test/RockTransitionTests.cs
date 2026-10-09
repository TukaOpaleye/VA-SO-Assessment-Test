using RockTrack.Core;

namespace RockTrack.Tests
{
    public class RockTransitionTest
    {
        private Rock _sutRock = null!;

        [SetUp]
        public void Setup()
        {
            _sutRock = new Rock(1, "Test Rock", RockCategory.Career, new DateOnly(2026, 12, 1));
        }

        [Test]
        public void Rock_StatusTransition_TraversesPendingToCompleted()
        {
            Assert.That(_sutRock.TryTransitionTo(RockStatus.Completed), Is.True);
        }

        [Test]
        public void Rock_StatusTransition_TraversesPendingToMissed()
        {
            Assert.That(_sutRock.TryTransitionTo(RockStatus.Missed), Is.True);
        }

        [Test]
        public void Rock_StatusTransition_FailsCompletedToPending()
        {
            Assume.That(_sutRock.TryTransitionTo(RockStatus.Completed));
            
            Assert.That(_sutRock.TryTransitionTo(RockStatus.Pending), Is.False);
        }

        [Test]
        public void Rock_StatusTransition_FailsMissedToPending()
        {
            Assume.That(_sutRock.TryTransitionTo(RockStatus.Missed));

            Assert.That(_sutRock.TryTransitionTo(RockStatus.Pending), Is.False);
        }

        [Test]
        public void Rock_StatusTransition_FailsMissedToCompleted()
        {
            Assume.That(_sutRock.TryTransitionTo(RockStatus.Missed));

            Assert.That(_sutRock.TryTransitionTo(RockStatus.Completed), Is.False);
        }
    }
}   
