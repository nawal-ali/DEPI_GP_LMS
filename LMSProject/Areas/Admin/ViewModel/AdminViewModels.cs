using System.ComponentModel.DataAnnotations;
using MLSCore.Models;

namespace LMSProject.Areas.Admin.ViewModels
{
    // ── Dashboard ─────────────────────────────────────────────────────────────
    public class AdminDashboardVM
    {
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalParents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalExams { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalSubmissions { get; set; }
        public int PendingGrading { get; set; }
        public int TotalAnnouncements { get; set; }
        public int ActiveAnnouncements { get; set; }
        public List<RecentCourseVM> RecentCourses { get; set; } = new();
        public List<RecentActivityVM> RecentActivity { get; set; } = new();
    }

    public class RecentCourseVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? ImageName { get; set; }
        public string InstructorName { get; set; } = "";
        public int StudentCount { get; set; }
        public int CurrentState { get; set; }
    }

    public class RecentActivityVM
    {
        public string Icon { get; set; } = "";
        public string Color { get; set; } = "";
        public string Message { get; set; } = "";
        public string Time { get; set; } = "";
    }

    // ── User Management ───────────────────────────────────────────────────────
    public class UserListVM<T>
    {
        public List<T> Items { get; set; } = new();
        public List<T> Paged { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string FilterStatus { get; set; } = "";
        public string FilterExtra { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class InstructorListItemVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Specialization { get; set; } = "";
        public int ExperienceYears { get; set; }
        public int CourseCount { get; set; }
        public string? ImageName { get; set; }
        public int CurrentState { get; set; }
        public string Initials => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();
    }

    public class StudentListItemVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Grade { get; set; } = "";
        public int CourseCount { get; set; }
        public string? ImageName { get; set; }
        public int CurrentState { get; set; }
        public string? ParentName { get; set; }
        public string Initials => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();
    }

    public class ParentListItemVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public int ChildrenCount { get; set; }
        public int CurrentState { get; set; }
        public List<string> ChildNames { get; set; } = new();
        public string Initials => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();
    }

    // Edit user VMs
    public class EditStudentVM
    {
        public int Id { get; set; }
        [Required] public string FullName { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public int GradeId { get; set; }
        public int? ParentId { get; set; }
        public string? ImageName { get; set; }
        public IFormFile? Image { get; set; }
        public List<LMSProject.Areas.Admin.Helpers.SelectDropList> Grades { get; set; } = new();
        public List<LMSProject.Areas.Admin.Helpers.SelectDropList> Parents { get; set; } = new();
    }

    public class EditParentVM
    {
        public int Id { get; set; }
        [Required] public string FullName { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public List<int> ChildIds { get; set; } = new();
        public List<string> ExistingChildNames { get; set; } = new();
        public List<LMSProject.Areas.Admin.Helpers.SelectDropList> AllStudents { get; set; } = new();
    }

    // ── Course Management ─────────────────────────────────────────────────────
    public class CourseListVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? ImageName { get; set; }
        public string InstructorName { get; set; } = "";
        public int InstructorId { get; set; }
        public string Grade { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Term { get; set; } = "";
        public double Price { get; set; }
        public CourseStatus Status { get; set; }
        public int StudentCount { get; set; }
        public int MaterialCount { get; set; }
        public int ExamCount { get; set; }
        public int AssignmentCount { get; set; }
        public int CurrentState { get; set; }
    }

    public class CreateCourseVM
    {
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        public CourseStatus Status { get; set; }
        public double Price { get; set; }
        public bool ShowInHomePage { get; set; }
        [Required] public int TermId { get; set; }
        [Required] public int GradeId { get; set; }
        [Required] public int SubSubjId { get; set; }
        [Required] public int InstructorId { get; set; }
        public string? ImageName { get; set; }
        public IFormFile? Image { get; set; }

        public List<LMSProject.Areas.Admin.Helpers.SelectDropList> Terms { get; set; } = new();
        public List<LMSProject.Areas.Admin.Helpers.SelectDropList> Grades { get; set; } = new();
        public List<LMSProject.Areas.Admin.Helpers.SelectDropList> Subjects { get; set; } = new();
        public List<LMSProject.Areas.Admin.Helpers.SelectDropList> SubSubjects { get; set; } = new();
        public List<LMSProject.Areas.Admin.Helpers.SelectDropList> Instructors { get; set; } = new();
    }

    public class EditCourseVM : CreateCourseVM
    {
        public int Id { get; set; }
        public int SubjId { get; set; }
    }

    public class EnrollStudentsVM
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string InstructorName { get; set; } = "";
        public List<EnrollStudentItemVM> Students { get; set; } = new();
        public string SearchTerm { get; set; } = "";
    }

    public class EnrollStudentItemVM
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = "";
        public string Grade { get; set; } = "";
        public bool IsEnrolled { get; set; }
        public string Initials => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();
    }

    // ── Content Monitoring ────────────────────────────────────────────────────
    public class MaterialMonitorVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string CourseName { get; set; } = "";
        public int CourseId { get; set; }
        public string? FileName { get; set; }
        public string FileUrl { get; set; } = "";
        public MaterialType MaterialType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string TypeIcon => MaterialType switch
        {
            MaterialType.PDF => "fa-file-pdf",
            MaterialType.DOC => "fa-file-word",
            MaterialType.Image => "fa-file-image",
            MaterialType.Link => "fa-link",
            _ => "fa-file"
        };
        public string TypeColor => MaterialType switch
        {
            MaterialType.PDF => "#E13468",
            MaterialType.DOC => "#29B9E7",
            MaterialType.Image => "#33EFA0",
            MaterialType.Link => "#F48C06",
            _ => "#9ca3af"
        };
    }

    public class ExamMonitorVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int CourseId { get; set; }
        public DateTime? Deadline { get; set; }
        public double TotalMarks { get; set; }
        public int DurationMinutes { get; set; }
        public int QuestionCount { get; set; }
        public int AttemptCount { get; set; }
        public double AverageScore { get; set; }
        public bool IsExpired => Deadline.HasValue && DateTime.Now > Deadline.Value;
    }

    public class AssignmentMonitorVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int CourseId { get; set; }
        public DateTime Deadline { get; set; }
        public double TotalMarks { get; set; }
        public int SubmissionCount { get; set; }
        public int GradedCount { get; set; }
        public int LateCount { get; set; }
        public bool IsExpired => DateTime.Now > Deadline;
    }

    public class AssignmentSubmissionsVM
    {
        public TbAssignment Assignment { get; set; } = null!;
        public List<SubmissionRowVM> Submissions { get; set; } = new();
        public int TotalEnrolled { get; set; }
    }

    public class SubmissionRowVM
    {
        public int SubmissionId { get; set; }
        public string StudentName { get; set; } = "";
        public string? TextAnswer { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool IsLate { get; set; }
        public double? Marks { get; set; }
        public string? Feedback { get; set; }
        public bool IsGraded { get; set; }
    }

    // ── Announcements ─────────────────────────────────────────────────────────
    public class AnnouncementListVM
    {
        public List<TbAnnouncement> Items { get; set; } = new();
        public List<TbAnnouncement> Filtered { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string SelectedPriority { get; set; } = "";
        public string SelectedAudience { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 6;
        public int TotalPages => (int)Math.Ceiling((double)Filtered.Count / PageSize);
        public int TotalActive { get; set; }
        public int TotalPinned { get; set; }
        public int TotalUrgent { get; set; }
        public CreateAnnouncementVM NewItem { get; set; } = new();
    }

    public class CreateAnnouncementVM
    {
        [Required, MaxLength(200)] public string Title { get; set; } = "";
        [Required] public string Content { get; set; } = "";
        [MaxLength(500)] public string? Description { get; set; }
        public string TargetAudience { get; set; } = "All";
        public string Priority { get; set; } = "Medium";
        public string Category { get; set; } = "General";
        public bool IsPinned { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class EditAnnouncementVM : CreateAnnouncementVM
    {
        public int Id { get; set; }
    }

    // ── Statistics ────────────────────────────────────────────────────────────
    public class AdminStatisticsVM
    {
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalParents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalExams { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalSubmissions { get; set; }
        public int GradedSubmissions { get; set; }
        public int TotalMaterials { get; set; }
        public int TotalAnnouncements { get; set; }

        // Chart data (JSON-ready)
        public int[] MonthlyEnrollments { get; set; } = new int[12];
        public int[] MonthlySubmissions { get; set; } = new int[12];

        public List<(string Label, int Value, string Color)> UserBreakdown { get; set; } = new();
        public List<(string CourseName, int Students)> TopCourses { get; set; } = new();
    }
}