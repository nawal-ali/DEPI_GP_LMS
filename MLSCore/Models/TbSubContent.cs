namespace MLSCore.Models
{
    //-----------------------Lesson List-----------------------
    public class TbSubContent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? SessionURL {  get; set; }
        public string? RecordURL {  get; set; }
        public DateTime StartDate {  get; set; }
        public DateTime EndDate { get; set; }
        public TimeOnly StartTime {  get; set; }
        public TimeOnly EndTime { get; set; }
        public string? CreatedBy { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
        public int CurrentState { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        [ForeignKey("CourseContent")]
        public int ContentId { get; set; }

        public TbCourseContent CourseContent { get; set; }
        public List<TbTask> Tasks { get; set; }
        public List<TbMterials> Mterials { get; set; }
        public List<TbSessionAttend> Sessions { get; set; }
    }
}
