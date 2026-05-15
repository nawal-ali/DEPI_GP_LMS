using System;
using System.ComponentModel.DataAnnotations;

namespace LMSProject.ViewModel
{
    /// <summary>
    /// ViewModel for reviewing and grading student task submissions
    /// </summary>
    public class ReviewSubmissionVM
    {
        [Required]
        [Display(Name = "Submission ID")]
        public int SubmissionId { get; set; }

        [Required]
        [Display(Name = "Task ID")]
        public int TaskId { get; set; }

        [Display(Name = "Student Name")]
        public string StudentName { get; set; }

        [Display(Name = "Student ID")]
        public int StudentId { get; set; }

        [Display(Name = "Student Answer URL")]
        public string AnswerURL { get; set; }

        [Display(Name = "Current Score")]
        public double CurrentScore { get; set; }

        [Display(Name = "Current Comment")]
        public string CurrentComment { get; set; }

        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
        [Display(Name = "New Score")]
        public double NewScore { get; set; }

        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        [Display(Name = "Instructor Comment/Feedback")]
        public string NewComment { get; set; }

        [Display(Name = "Date Graded")]
        public DateTime DateGraded { get; set; } = DateTime.Now;
    }
}
