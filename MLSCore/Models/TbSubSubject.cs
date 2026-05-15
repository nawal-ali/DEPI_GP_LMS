
namespace MLSCore.Models
{
    public class TbSubSubject
    {
        public int Id { get; set; }

        [Display(Name = "Sub - Subject Name")]
        public string Name { get; set; }
        public string? CreatedBy { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public int CurrentState { get; set; }

        public string ImageName { get; set; } = null!;

        public bool ShowInHomePage { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
        
        
        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public TbSubject Subject { get; set; }

        public List<TbCourse> Courses { get; set; }
    }
}