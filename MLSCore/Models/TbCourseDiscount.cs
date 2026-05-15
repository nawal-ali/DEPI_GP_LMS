namespace MLSCore.Models
{
    public class TbCourseDiscount
    {
        public int Id { get; set; }
        public double Discount { get; set; }
        public double TotalPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndtDate { get; set; }
        public string? CreatedBy { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
        public int CurrentState { get; set; }
        public string ImageName { get; set; } = null!;
        public bool ShowInHomePage { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }


        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public TbCourse Course { get; set; }


    }
}
