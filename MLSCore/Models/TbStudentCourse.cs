namespace MLSCore.Models
{
    public class TbStudentCourse
    {
        public int Id { get; set; }
        [ForeignKey("Student")]
        public int StId {  get; set; }
        public TbStudent Student { get; set; }
        [ForeignKey("Course")]
        public int CourseId {  get; set; }
        public TbCourse Course { get; set; }
    }
}
