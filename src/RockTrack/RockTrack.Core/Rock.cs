namespace RockTrack.Core
{
    public sealed class Rock
    {
        public Rock(int id, int memberId, string title, RockCategory category, DateOnly dueDate, string? note = null)
        {
            Id = id;
            MemberId = memberId;
            Title = title.Trim();
            Category = category;
            DueDate = dueDate;
            Note = note;
            Status = RockStatus.Pending;
        }

        public int Id { get; }
        public string Title { get; }
        public DateOnly DueDate { get; }
        public string? Note { get; }
        public int MemberId { get; }
        public RockStatus Status { get; private set; }
        public RockCategory Category { get; }
    }
}
