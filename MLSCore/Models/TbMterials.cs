namespace MLSCore.Models
{
    public class TbMterials
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [RegularExpression(@"https?:\/\/(www\.)?[^\s]+",
        ErrorMessage = "Invalid URL")]
        public string MaterialURL { get; set; }
        public string? CreatedBy { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public int CurrentState { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("SubContent")]
        public int LessonId { get; set; }
        public TbSubContent SubContent { get; set; }

    }
}
