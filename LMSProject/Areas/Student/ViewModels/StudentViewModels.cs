using MLSCore.Models;
using System.ComponentModel.DataAnnotations;

namespace LMSProject.Areas.Student.ViewModels
{
    // ── Dashboard overview ─────────────────────────────────────────────────
    public class StudentDashboardVM
    {
        public string StudentName { get; set; } = "";
        public string? ImageName { get; set; }
        public string? GradeName { get; set; }
        public string Initials => StudentName.Length >= 2 ? StudentName[..2].ToUpper() : StudentName.ToUpper();

        public int EnrolledCourses { get; set; }
        public int CompletedExams { get; set; }
        public int PendingAssignments { get; set; }
        public int AvailableExams { get; set; }
        public double ExamAverage { get; set; }
        public int Announcements { get; set; }

        public List<UpcomingDeadlineVM> UpcomingDeadlines { get; set; } = new();
        public List<RecentResultVM> RecentResults { get; set; } = new();
        public List<RecentAnnouncementVM> RecentAnnouncements { get; set; } = new();
        public List<RecentMaterialVM> RecentMaterials { get; set; } = new();
        public List<CourseCardVM> Courses { get; set; } = new();
    }

    public class UpcomingDeadlineVM
    {
        public string Title { get; set; } = "";
        public string Type { get; set; } = "";
        public string CourseName { get; set; } = "";
        public DateTime Deadline { get; set; }
        public int DaysLeft => Math.Max(0, (int)(Deadline - DateTime.Now).TotalDays);
        public bool IsUrgent => DaysLeft <= 2;
        public string Color => Type == "Exam" ? "#5B72EE" : "#F48C06";
        public string Icon => Type == "Exam" ? "fa-file-alt" : "fa-tasks";
    }

    public class RecentResultVM
    {
        public string Title { get; set; } = "";
        public string CourseName { get; set; } = "";
        public double Score { get; set; }
        public double TotalMarks { get; set; }
        public DateTime Date { get; set; }
        public double Pct => TotalMarks > 0 ? Score / TotalMarks * 100 : 0;
        public string Grade => Pct switch { >= 90 => "A+", >= 80 => "A", >= 70 => "B", >= 60 => "C", >= 50 => "D", _ => "F" };
        public string Color => Pct >= 70 ? "#16a34a" : Pct >= 50 ? "#F48C06" : "#E13468";
    }

    public class RecentAnnouncementVM
    {
        public string Title { get; set; } = "";
        public string Priority { get; set; } = "Normal";
        public DateTime Date { get; set; }
    }

    public class RecentMaterialVM
    {
        public string Title { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string Type { get; set; } = "";
        public DateTime Date { get; set; }
    }

    // ── Courses ────────────────────────────────────────────────────────────
    public class CourseCardVM
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string? ImageName { get; set; }
        public string InstructorName { get; set; } = "";
        public string? InstructorImage { get; set; }
        public string GradeName { get; set; } = "";
        public string TermName { get; set; } = "";
        public string Subject { get; set; } = "";
        public CourseStatus Status { get; set; }
        public int MaterialCount { get; set; }
        public int ExamCount { get; set; }
        public int AssignmentCount { get; set; }
        public int SubmittedCount { get; set; }
        public int ExamsTaken { get; set; }
        public DateTime? NextDeadline { get; set; }
    }

    public class CourseDetailVM
    {
        public TbCourse Course { get; set; } = null!;
        public TbInstructor? Instructor { get; set; }
        public List<TbCourseMaterial> Materials { get; set; } = new();
        public List<ExamVM> Exams { get; set; } = new();
        public List<AssignmentVM> Assignments { get; set; } = new();
    }

    // ── Exams ──────────────────────────────────────────────────────────────
    public class ExamVM
    {
        public int ExamId { get; set; }
        public string Title { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int CourseId { get; set; }
        public int DurationMinutes { get; set; }
        public double TotalMarks { get; set; }
        public DateTime? Deadline { get; set; }
        public int QuestionCount { get; set; }

        public bool IsSubmitted { get; set; }
        public bool IsPending { get; set; }
        public double? Score { get; set; }
        public DateTime? TakenAt { get; set; }

        public bool IsExpired => Deadline.HasValue && DateTime.Now > Deadline.Value;
        public bool CanTake => !IsSubmitted && !IsExpired;

        // ── FIX: double? switch — use HasValue guard, no null arm needed ──
        public double? Percentage => TotalMarks > 0 && Score.HasValue
            ? Score.Value / TotalMarks * 100
            : null;

        public string? GradeLabel => Percentage.HasValue
            ? Percentage.Value switch
            {
                >= 90 => "A+",
                >= 80 => "A",
                >= 70 => "B",
                >= 60 => "C",
                >= 50 => "D",
                _ => "F"
            }
            : null;

        public string ScoreColor => Percentage.HasValue
            ? Percentage.Value switch
            {
                >= 70 => "#16a34a",
                >= 50 => "#F48C06",
                _ => "#E13468"
            }
            : "#9ca3af";
    }

    public class TakeExamVM
    {
        public TbTest Exam { get; set; } = null!;
        public List<TbTestQuestion> Questions { get; set; } = new();
        public string CourseName { get; set; } = "";
    }

    public class SubmitExamVM
    {
        public int ExamId { get; set; }
        public List<AnswerVM> Answers { get; set; } = new();
    }

    public class AnswerVM
    {
        public int QuestionId { get; set; }
        public int ChoiceId { get; set; }
        public string? TextAnswer { get; set; }
    
    }

    // ── Assignments ────────────────────────────────────────────────────────
    public class AssignmentVM
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string CourseName { get; set; } = "";
    public int CourseId { get; set; }
    public double TotalMarks { get; set; }
    public DateTime Deadline { get; set; }
    public SubmissionType SubType { get; set; }

    public bool IsSubmitted { get; set; }
    public bool IsLate { get; set; }
    public bool IsGraded { get; set; }
    public double? Score { get; set; }
    public string? Feedback { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public string? TextAnswer { get; set; }

    public bool IsExpired => !IsSubmitted && DateTime.Now > Deadline;
    public string StatusLabel => IsSubmitted ? (IsGraded ? $"{Score}/{TotalMarks}" : "Submitted") : (IsExpired ? "Missing" : "Pending");
    public string StatusColor => IsSubmitted ? (IsGraded ? "#16a34a" : "#29B9E7") : (IsExpired ? "#E13468" : "#F48C06");
    public string BadgeClass => IsSubmitted ? (IsGraded ? "pb-green" : "pb-blue") : (IsExpired ? "pb-red" : "pb-orange");
}

public class SubmitAssignmentVM
{
    [Required] public int AssignmentId { get; set; }
    public string? TextAnswer { get; set; }
    public IFormFile? File { get; set; }
}

// ── Profile ────────────────────────────────────────────────────────────
public class StudentProfileVM
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? GradeName { get; set; }
    public string? ImageName { get; set; }
    public IFormFile? Image { get; set; }
    public string Initials => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();

    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}

// ── Instructors ────────────────────────────────────────────────────────
public class InstructorCardVM
{
    public int InstructorId { get; set; }
    public string FullName { get; set; } = "";
    public string? ImageName { get; set; }
    public string? Specialization { get; set; }
    public string? Bio { get; set; }
    public int ExperienceYears { get; set; }
    public string Initials => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();
    public List<string> CourseNames { get; set; } = new();
}

// ── Parent info ────────────────────────────────────────────────────────
public class StudentParentVM
{
    public string? ParentName { get; set; }
    public string? ParentEmail { get; set; }
    public string? ParentPhone { get; set; }
    public string? Relationship { get; set; }
    public string? Occupation { get; set; }
    public string? ImageName { get; set; }
    public bool HasParent => !string.IsNullOrEmpty(ParentName);
    public string Initials => HasParent ? (ParentName!.Length >= 2 ? ParentName[..2].ToUpper() : ParentName.ToUpper()) : "?";
}

// ── Progress ───────────────────────────────────────────────────────────
public class ProgressVM
{
    public double ExamAverage { get; set; }
    public double AssignmentCompletion { get; set; }
    public List<(string Course, double Avg)> CourseAverages { get; set; } = new();
    public List<(string Label, double Score, double Total)> ExamHistory { get; set; } = new();
}
}