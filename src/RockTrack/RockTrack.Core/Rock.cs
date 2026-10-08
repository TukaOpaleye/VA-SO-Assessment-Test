namespace RockTrack.Core
{
    public class Rock
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateOnly DueDate { get; set; }
        public string? Note { get; set; }
        public int MemberId { get; set; }
        public RockStatus Status { get; set; }
        public RockCategory Category { get; set; }

    }
}
