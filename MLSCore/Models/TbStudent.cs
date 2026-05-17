using MLSCore.IdentityModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLSCore.Models
{
    public class TbStudent
    {
        public int Id { get; set; }

        public string FullName { get; set; } = "";
        public string? ParentMobile { get; set; }
        public string? ParentNationalId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CurrentState { get; set; } = 1;
        public string? ImageName { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("Grade")]
        public int GradeId { get; set; }
        public TbGrade? Grade { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        [ForeignKey("Parent")]
        public int? ParentId { get; set; }
        public TbParent? Parent { get; set; }

        public List<TbStudentCourse>? StudentCourses { get; set; }
        public List<TbStudentTest>? StudentTests { get; set; }
        public List<TbSessionAttend>? Sessions { get; set; }
    }
}