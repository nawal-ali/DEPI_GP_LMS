using System.ComponentModel.DataAnnotations.Schema;

namespace MLSCore.Models
{
    public enum SubmissionType { TextOnly, FileOnly, Both }

    public class TbAssignment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public double TotalMarks { get; set; }
        public DateTime Deadline { get; set; }
        public SubmissionType SubmissionType { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CurrentState { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public TbCourse Course { get; set; }

        public List<TbAssignmentSubmission> Submissions { get; set; } = new();
    }
}
