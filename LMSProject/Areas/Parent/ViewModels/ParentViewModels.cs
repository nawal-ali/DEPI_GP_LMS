using MLSCore.Models;

namespace LMSProject.Areas.Parent.ViewModels
{
    // ── Dashboard ─────────────────────────────────────────────────────────────
    public class ParentDashboardVM
    {
        public string ParentName { get; set; } = "";
        public string? ImageName { get; set; }
        public int ChildrenCount { get; set; }
        public int TotalCourses { get; set; }
        public int PendingExams { get; set; }
        public int PendingAssignments { get; set; }
        public int TotalSubmissions { get; set; }
        public int GradedSubmissions { get; set; }
        public int Announcements { get; set; }
        public List<ChildSummaryVM> Children { get; set; } = new();
        public List<UpcomingDeadlineVM> UpcomingDeadlines { get; set; } = new();
        public List<RecentResultVM> RecentResults { get; set; } = new();
    }

    // ── Child Summary (dashboard cards) ───────────────────────────────────────
    public class ChildSummaryVM
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = "";
        public string? ImageName { get; set; }
        public string Grade { get; set; } = "";
        public int CourseCount { get; set; }
        public int ExamsTaken { get; set; }
        public int AssignmentsSubmitted { get; set; }
        public double AverageExamScore { get; set; }
        public double AssignmentAvgScore { get; set; }
        public string Initials => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();
        public string PerformanceLabel => AverageExamScore switch
        {
            >= 85 => "Excellent",
            >= 70 => "Good",
            >= 50 => "Average",
            _ => "Needs Attention"
        };
        public string PerformanceColor => AverageExamScore switch
        {
            >= 85 => "#16a34a",
            >= 70 => "#29B9E7",
            >= 50 => "#F48C06",
            _ => "#E13468"
        };
    }

    // ── Upcoming Deadline ─────────────────────────────────────────────────────
    public class UpcomingDeadlineVM
    {
        public string Title { get; set; } = "";
        public string Type { get; set; } = ""; // "Exam" or "Assignment"
        public string CourseName { get; set; } = "";
        public string ChildName { get; set; } = "";
        public DateTime Deadline { get; set; }
        public bool IsSubmitted { get; set; }
        public string Color => Type == "Exam" ? "#5B72EE" : "#F48C06";
        public int DaysLeft => (int)(Deadline - DateTime.Now).TotalDays;
    }

    // ── Recent Result ─────────────────────────────────────────────────────────
    public class RecentResultVM
    {
        public string Title { get; set; } = "";
        public string Type { get; set; } = "";
        public string ChildName { get; set; } = "";
        public string CourseName { get; set; } = "";
        public double Score { get; set; }
        public double TotalMarks { get; set; }
        public DateTime Date { get; set; }
        public double Percentage => TotalMarks > 0 ? Score / TotalMarks * 100 : 0;
        public string Color => Percentage >= 70 ? "#16a34a" : Percentage >= 50 ? "#F48C06" : "#E13468";
    }

    // ── Child Detail ──────────────────────────────────────────────────────────
    public class ChildDetailVM
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = "";
        public string? ImageName { get; set; }
        public string Grade { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Initials => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();

        public List<CourseProgressVM> Courses { get; set; } = new();
        public List<ExamResultVM> ExamResults { get; set; } = new();
        public List<AssignmentStatusVM> Assignments { get; set; } = new();

        // Computed stats
        public double AverageExamScore => ExamResults.Any(e => e.IsSubmitted)
            ? ExamResults.Where(e => e.IsSubmitted).Average(e => e.ScorePercentage) : 0;
        public int MissingAssignments => Assignments.Count(a => !a.IsSubmitted && a.IsExpired);
        public int PendingAssignments => Assignments.Count(a => !a.IsSubmitted && !a.IsExpired);
    }

    public class CourseProgressVM
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string InstructorName { get; set; } = "";
        public int ExamCount { get; set; }
        public int AssignmentCount { get; set; }
        public int MaterialCount { get; set; }
        public double ExamAvg { get; set; }
        public double AssignmentAvg { get; set; }
    }

    public class ExamResultVM
    {
        public int StudentTestId { get; set; }
        public string ExamTitle { get; set; } = "";
        public string CourseName { get; set; } = "";
        public double Score { get; set; }
        public double TotalMarks { get; set; }
        public DateTime? TakenAt { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsLate { get; set; }
        public double ScorePercentage => TotalMarks > 0 ? Score / TotalMarks * 100 : 0;
        public string Grade => ScorePercentage switch
        {
            >= 90 => "A+",
            >= 80 => "A",
            >= 70 => "B",
            >= 60 => "C",
            >= 50 => "D",
            _ => "F"
        };
        public string GradeColor => ScorePercentage switch
        {
            >= 70 => "#16a34a",
            >= 50 => "#F48C06",
            _ => "#E13468"
        };
    }

    public class AssignmentStatusVM
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = "";
        public string CourseName { get; set; } = "";
        public DateTime Deadline { get; set; }
        public double TotalMarks { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsLate { get; set; }
        public bool IsExpired => !IsSubmitted && DateTime.Now > Deadline;
        public double? Score { get; set; }
        public bool IsGraded { get; set; }
        public string StatusLabel => IsSubmitted
            ? (IsGraded ? $"{Score}/{TotalMarks}" : "Submitted — Pending Grade")
            : (IsExpired ? "Missing" : "Not Submitted Yet");
        public string StatusColor => IsSubmitted
            ? (IsGraded ? "#16a34a" : "#29B9E7")
            : (IsExpired ? "#E13468" : "#F48C06");
    }

    // ── Announcements ─────────────────────────────────────────────────────────
    public class ParentAnnouncementVM
    {
        public List<TbAnnouncement> Paged { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string SearchTerm { get; set; } = "";
        public string SelectedPriority { get; set; } = "";
        public int UrgentCount { get; set; }
        public int PinnedCount { get; set; }
    }

    // ── Profile ───────────────────────────────────────────────────────────────
    public class ParentProfileVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? AltPhone { get; set; }
        public string? Occupation { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? NationalId { get; set; }
        public string? Relationship { get; set; }
        public string? ImageName { get; set; }
        public IFormFile? Image { get; set; }
        public string Initials => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();
        public int ChildrenCount { get; set; }
    }

    // ── Weekly Report data ────────────────────────────────────────────────────
    public class WeeklyReportVM
    {
        public string ParentName { get; set; } = "";
        public DateTime ReportDate { get; set; } = DateTime.Now;
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<ChildReportVM> Children { get; set; } = new();
    }

    public class ChildReportVM
    {
        public string FullName { get; set; } = "";
        public string Grade { get; set; } = "";
        public List<ExamResultVM> ExamResults { get; set; } = new();
        public List<AssignmentStatusVM> Assignments { get; set; } = new();
        public double OverallAvg { get; set; }
        public int SubmittedThisWeek { get; set; }
        public int MissingWork { get; set; }
        public string PerformanceSummary { get; set; } = "";
    }
}