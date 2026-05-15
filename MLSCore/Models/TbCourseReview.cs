namespace MLSCore.Models
{
    public class TbCourseReview
    {
        public int Id { get; set; }

        public string Review { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        public string? CreatedBy { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
        public int CurrentState { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [ForeignKey("Course")]
        public int CourseId {  get; set; }
        public TbCourse Course { get; set; }

    }
}
