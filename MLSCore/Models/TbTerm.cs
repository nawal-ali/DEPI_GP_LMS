

namespace MLSCore.Models
{
    public class TbTerm
    {
        public int Id { get; set; }
        
        [Display(Name = "Term Name")]
        public string Name { get; set; }

        public string? CreatedBy { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public int CurrentState { get; set; }

        public string ImageName { get; set; } = null!;

        public bool ShowInHomePage { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public List<TbCourse> Courses { get; set; }
    }

}
