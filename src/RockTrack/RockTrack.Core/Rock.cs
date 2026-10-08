namespace RockTrack.Core
{
    public sealed class Rock
    {
        public Rock(int memberId, string title, RockCategory category, DateOnly dueDate, string? note = null)
        {
            Id = Guid.NewGuid();
            MemberId = memberId;
            Title = title.Trim();
            Category = category;
            DueDate = dueDate;
            Note = note;
            Status = RockStatus.Pending;
        }

        public Guid Id { get; }
        public string Title { get; }
        public DateOnly DueDate { get; }
        public string? Note { get; }
        public int MemberId { get; }
        public RockStatus Status { get; private set; }
        public RockCategory Category { get; }

        // Status transitions: Only pending to completed & pending to missed are allowed
        public bool TryTransitionTo(RockStatus newStatus)
        {
            if (Status != RockStatus.Pending || newStatus == RockStatus.Pending)
            {
                return false;
            }

            Status = newStatus;
            return true;
        }
    }
}
