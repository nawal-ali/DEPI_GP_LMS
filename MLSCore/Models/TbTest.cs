namespace MLSCore.Models
{
    public class TbTest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CourseId { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime? Deadline { get; set; }
        public double TotalMarks { get; set; }
        public string? CreatedBy { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public int CurrentState { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public List<TbTestQuestion> Questions { get; set; }

    }
}
