using MLSCore.IdentityModel;

namespace MLSCore.Models
{
    public class TbStudent
    {
        public int Id { get; set; }

        // public string UserId { get; set; } // FK to AspNetUsers
        public string FullName { get; set; }
        public string ParentMobile { get; set; }
        public string ParentNationalId { get; set; }
        public string? CreatedBy { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
        public int CurrentState { get; set; }
        public string ImageName { get; set; } = null!;

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }


        [ForeignKey("Grade")]
        public int GradeId { get; set; }
        public TbGrade Grade { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Foreign key to Parent (one parent per student/family account)
        /// </summary>
        [ForeignKey("Parent")]
        public int? ParentId { get; set; }

        /// <summary>
        /// Navigation property to Parent
        /// </summary>
        public TbParent Parent { get; set; }

        public List<TbStudentCourse> StudentCourses { get; set; }
        public List<TbStudentTest> StudentTests { get; set; }
        public List<TbSessionAttend> Sessions { get; set; }
    }
}
