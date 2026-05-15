using System.ComponentModel.DataAnnotations.Schema;

namespace MLSCore.Models
{
    public class TbAssignmentSubmission
    {
        public int Id { get; set; }
        public string? TextAnswer { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool IsLate { get; set; }
        public double? Marks { get; set; }
        public string? InstructorFeedback { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CurrentState { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }
        public TbAssignment Assignment { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public TbStudent Student { get; set; }
    }
}
