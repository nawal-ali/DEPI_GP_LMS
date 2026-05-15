

namespace MLSCore.Models
{
    public class TbGrade
    {
        public int Id { get; set; }
        
        [Display(Name = "Grade Name")]
        public string Name { get; set; }
        public string? CreatedBy { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public int CurrentState { get; set; }

        public string ImageName { get; set; } = null!;

        public bool ShowInHomePage { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
        [ForeignKey("Stage")]
        public int StageId { get; set; }
        public TbStage Stage { get; set; }
        public List<TbCourse> Courses { get; set; }
        public List<TbStudent> Students { get; set; }
    }
}
