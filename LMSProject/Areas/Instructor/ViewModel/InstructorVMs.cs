using MLSCore.Models;
using System.ComponentModel.DataAnnotations;

namespace LMSProject.Areas.Instructor.ViewModel
{
    // ─── Dashboard ──────────────────────────────────────────────────────────────

    public class InstructorDashboardVM
    {
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalExams { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalMaterials { get; set; }
        public int PendingSubmissions { get; set; }
        public List<RecentActivityVM> RecentActivity { get; set; } = new();
        public List<UpcomingDeadlineVM> UpcomingDeadlines { get; set; } = new();
    }

    public class RecentActivityVM
    {
        public string Message { get; set; }
        public string Time { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }

    public class UpcomingDeadlineVM
    {
        public string Title { get; set; }
        public string CourseName { get; set; }
        public DateTime Deadline { get; set; }
        public string Type { get; set; }
        public string BadgeColor { get; set; }
    }

    // ─── Materials ───────────────────────────────────────────────────────────────

    public class MaterialListItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string FileUrl { get; set; }
        public string? FileName { get; set; }
        public MaterialType MaterialType { get; set; }
        public string CourseName { get; set; }
        public int CourseId { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class CreateMaterialVM
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Please select a material type")]
        public MaterialType MaterialType { get; set; }

        public string? ExternalUrl { get; set; }
        public IFormFile? FileUpload { get; set; }

        public List<CourseDropItem> Courses { get; set; } = new();
    }

    public class EditMaterialVM : CreateMaterialVM
    {
        public int Id { get; set; }
        public string? ExistingFileUrl { get; set; }
        public string? ExistingFileName { get; set; }
    }

    public class CourseDropItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // ─── Exams ───────────────────────────────────────────────────────────────────

    public class ExamListItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string CourseName { get; set; }
        public int CourseId { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime? Deadline { get; set; }
        public double TotalMarks { get; set; }
        public int QuestionCount { get; set; }
        public int AttemptCount { get; set; }
    }

    public class CreateExamVM
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes")]
        public int DurationInMinutes { get; set; }

        public DateTime? Deadline { get; set; }

        [Range(0, 10000)]
        public double TotalMarks { get; set; } = 100;

        public List<CourseDropItem> Courses { get; set; } = new();
    }

    public class EditExamVM : CreateExamVM
    {
        public int Id { get; set; }
    }

    public class ExamDetailsVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string CourseName { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime? Deadline { get; set; }
        public double TotalMarks { get; set; }
        public List<QuestionListItemVM> Questions { get; set; } = new();
        public int AttemptCount { get; set; }
    }

    public class QuestionListItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Points { get; set; }
        public List<ChoiceVM> Choices { get; set; } = new();
    }

    public class ChoiceVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class CreateQuestionVM
    {
        [Required(ErrorMessage = "Question text is required")]
        [MaxLength(1000)]
        [Display(Name = "Question Text")]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Points")]
        public int Points { get; set; } = 1;

        public QuestionType QuestionType { get; set; } = QuestionType.Choice;

        public int TestId { get; set; }

        // For MCQ: up to 4 choices + correct answer index
        [Display(Name = "Choice A")]
        public string? ChoiceA { get; set; }
        [Display(Name = "Choice B")]
        public string? ChoiceB { get; set; }
        [Display(Name = "Choice C")]
        public string? ChoiceC { get; set; }
        [Display(Name = "Choice D")]
        public string? ChoiceD { get; set; }

        [Display(Name = "Correct Answer")]
        public string? CorrectChoice { get; set; }
    }

    // ─── Assignments ──────────────────────────────────────────────────────────────

    public class AssignmentListItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public double TotalMarks { get; set; }
        public DateTime Deadline { get; set; }
        public SubmissionType SubmissionType { get; set; }
        public string CourseName { get; set; }
        public int CourseId { get; set; }
        public int SubmissionCount { get; set; }
        public bool IsOverdue => DateTime.Now > Deadline;
    }

    public class CreateAssignmentVM
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Total marks are required")]
        [Range(1, 10000)]
        [Display(Name = "Total Marks")]
        public double TotalMarks { get; set; } = 100;

        [Required(ErrorMessage = "Deadline is required")]
        [DataType(DataType.DateTime)]
        public DateTime Deadline { get; set; } = DateTime.Now.AddDays(7);

        [Required]
        [Display(Name = "Submission Type")]
        public SubmissionType SubmissionType { get; set; } = SubmissionType.Both;

        public List<CourseDropItem> Courses { get; set; } = new();
    }

    public class EditAssignmentVM : CreateAssignmentVM
    {
        public int Id { get; set; }
    }

    public class AssignmentSubmissionsVM
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; }
        public string CourseName { get; set; }
        public double TotalMarks { get; set; }
        public DateTime Deadline { get; set; }
        public SubmissionType SubmissionType { get; set; }
        public List<SubmissionRowVM> Submissions { get; set; } = new();
        public int TotalEnrolled { get; set; }
    }

    public class SubmissionRowVM
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public int StudentId { get; set; }
        public string? TextAnswer { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool IsLate { get; set; }
        public double? Marks { get; set; }
        public string? InstructorFeedback { get; set; }
        public bool HasSubmission => SubmittedAt.HasValue;
    }

    public class GradeSubmissionVM
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public string StudentName { get; set; }
        public string AssignmentTitle { get; set; }
        public double TotalMarks { get; set; }
        public string? TextAnswer { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool IsLate { get; set; }

        [Range(0, 10000)]
        [Display(Name = "Marks Awarded")]
        public double? NewMarks { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Feedback")]
        public string? NewFeedback { get; set; }
    }

    // ─── Students ─────────────────────────────────────────────────────────────────

    public class StudentListVM
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? GradeName { get; set; }
        public string? Email { get; set; }
        public List<string> EnrolledCourses { get; set; } = new();
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? ParentEmail { get; set; }
        public string? ParentRelationship { get; set; }
    }

    public class StudentProgressVM
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string? GradeName { get; set; }
        public string? Email { get; set; }
        public List<CourseProgressItem> CourseProgress { get; set; } = new();
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? ParentEmail { get; set; }
        public string? ParentRelationship { get; set; }
        public string? ParentOccupation { get; set; }
        public string? ParentImageName { get; set; }
    }

    public class CourseProgressItem
    {
        public string CourseName { get; set; }
        public int TotalExams { get; set; }
        public int AttemptedExams { get; set; }
        public double? AvgExamScore { get; set; }
        public int TotalAssignments { get; set; }
        public int SubmittedAssignments { get; set; }
        public int LateSubmissions { get; set; }
        public double? AvgAssignmentMarks { get; set; }
    }
}