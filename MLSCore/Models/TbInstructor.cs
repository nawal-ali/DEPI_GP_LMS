using MLSCore.IdentityModel;

namespace MLSCore.Models
{
    public class TbInstructor
    {
        public int Id { get; set; }

       // public string UserId { get; set; } // FK to AspNetUsers
       public string FullName { get; set; }
        public string Bio { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public decimal Rating { get; set; } = 0;
        public string? CreatedBy { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
        public int CurrentState { get; set; }
        public string ImageName { get; set; } = null!;
        public bool ShowInHomePage { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public List<TbCourse> Courses { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
