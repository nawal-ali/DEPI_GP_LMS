using System.ComponentModel.DataAnnotations;

namespace LMSProject.Areas.SuperAdmin.ViewModels
{
    // ── Dashboard ────────────────────────────────────────────────────────────
    public class DashboardVM
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalParents { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalUsers => TotalStudents + TotalTeachers + TotalParents + TotalAdmins;
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int OpenTickets { get; set; }
        public int TotalAnnouncements { get; set; }
        public double SystemUptime { get; set; } = 99.9;
        public List<RecentActivityVM> RecentActivity { get; set; } = new();
    }

    public class RecentActivityVM
    {
        public string Icon { get; set; } = "";
        public string Color { get; set; } = "";
        public string Message { get; set; } = "";
        public string Time { get; set; } = "";
    }

    // ── Shared User ──────────────────────────────────────────────────────────
    public class UserListItemVM
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Role { get; set; } = "";
        public string Status { get; set; } = "Active";
        public string? ImageUrl { get; set; }
        public DateTime JoinDate { get; set; }
        public string Initials => Name.Length >= 2 ? Name.Substring(0, 2).ToUpper() : Name.ToUpper();
    }

    // ── Students ─────────────────────────────────────────────────────────────
    public class StudentListVM
    {
        public List<StudentItemVM> Students { get; set; } = new();
        public List<StudentItemVM> Filtered { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string SelectedGrade { get; set; } = "";
        public string SelectedStatus { get; set; } = "";
        public string SelectedSection { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)Filtered.Count / PageSize);
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int GraduatingStudents { get; set; }
        public int StudentsWithParents { get; set; }
    }

    public class StudentItemVM
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Grade { get; set; } = "";
        public string Section { get; set; } = "";
        public string Status { get; set; } = "Active";
        public string? ParentId { get; set; }
        public string? ImageUrl { get; set; }
        public string Initials => Name.Length >= 2 ? Name.Substring(0, 2).ToUpper() : Name.ToUpper();
    }

    // ── Teachers ─────────────────────────────────────────────────────────────
    public class TeacherListVM
    {
        public List<TeacherItemVM> Teachers { get; set; } = new();
        public List<TeacherItemVM> Filtered { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string SelectedSubject { get; set; } = "";
        public string SelectedStatus { get; set; } = "";
        public string SelectedGrade { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)Filtered.Count / PageSize);
        public int TotalTeachers { get; set; }
        public int ActiveTeachers { get; set; }
        public int SubjectsTaught { get; set; }
        public int AvgExperience { get; set; }
    }

    public class TeacherItemVM
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Grade { get; set; } = "";
        public string Status { get; set; } = "Active";
        public int Experience { get; set; }
        public string? ImageUrl { get; set; }
        public string Initials => Name.Length >= 2 ? Name.Substring(0, 2).ToUpper() : Name.ToUpper();
    }

    // ── Parents ──────────────────────────────────────────────────────────────
    public class ParentListVM
    {
        public List<ParentItemVM> Parents { get; set; } = new();
        public List<ParentItemVM> Filtered { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string SelectedStatus { get; set; } = "";
        public string SelectedOccupation { get; set; } = "";
        public string SelectedChildren { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)Filtered.Count / PageSize);
        public int TotalParents { get; set; }
        public int TotalChildren { get; set; }
        public int ActiveContacts { get; set; }
    }

    public class ParentItemVM
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Occupation { get; set; } = "";
        public int ChildrenCount { get; set; }
        public string Status { get; set; } = "Active";
        public string? ImageUrl { get; set; }
        public string Initials => Name.Length >= 2 ? Name.Substring(0, 2).ToUpper() : Name.ToUpper();
    }

    // ── Admins ───────────────────────────────────────────────────────────────
    public class AdminListVM
    {
        public List<AdminItemVM> Admins { get; set; } = new();
        public List<AdminItemVM> Filtered { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string SelectedStatus { get; set; } = "";
        public string SelectedAccess { get; set; } = "";
        public string SelectedDept { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)Filtered.Count / PageSize);
        public int TotalAdmins { get; set; }
        public int FullAccess { get; set; }
    }

    public class AdminItemVM
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Department { get; set; } = "";
        public string AccessLevel { get; set; } = "Full Access";
        public string Status { get; set; } = "Active";
        public string? ImageUrl { get; set; }
        public string Initials => Name.Length >= 2 ? Name.Substring(0, 2).ToUpper() : Name.ToUpper();
    }

    // ── User Profile (View) ──────────────────────────────────────────────────
    public class UserProfileVM
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Role { get; set; } = "";
        public string Status { get; set; } = "Active";
        public string? ImageUrl { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public string? Department { get; set; }
        public string? Grade { get; set; }
        public string? Subject { get; set; }
        public string? Occupation { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string Initials => Name.Length >= 2 ? Name.Substring(0, 2).ToUpper() : Name.ToUpper();
        public string RoleColor => Role switch
        {
            "Student" => "#5B72EE",
            "Teacher" => "#29B9E7",
            "Parent" => "#33EFA0",
            "Admin" => "#E13468",
            _ => "#2F327D"
        };
    }

    // ── Create User ──────────────────────────────────────────────────────────
    public class CreateUserVM
    {
        [Required] public string FullName { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string Password { get; set; } = "";
        [Required] public string Role { get; set; } = "Student";
        public string? Phone { get; set; }
        public string? Grade { get; set; }
        public string? Subject { get; set; }
        public string? Department { get; set; }
    }

    // ── Announcements ────────────────────────────────────────────────────────
    public class AnnouncementListVM
    {
        public List<AnnouncementItemVM> Announcements { get; set; } = new();
        public List<AnnouncementItemVM> Filtered { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string SelectedPriority { get; set; } = "";
        public string SelectedStatus { get; set; } = "";
        public string SelectedTarget { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalPages => (int)Math.Ceiling((double)Filtered.Count / PageSize);
        public int TotalAnnouncements { get; set; }
        public int ActiveAnnouncements { get; set; }
        public int TotalViews { get; set; }
        public int UrgentAnnouncements { get; set; }
        public CreateAnnouncementVM NewAnnouncement { get; set; } = new();
    }

    public class AnnouncementItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string Author { get; set; } = "";
        public string Priority { get; set; } = "Normal";
        public string Status { get; set; } = "Active";
        public string TargetAudience { get; set; } = "All";
        public int Views { get; set; }
        public DateTime Date { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class CreateAnnouncementVM
    {
        [Required] public string Title { get; set; } = "";
        [Required] public string Content { get; set; } = "";
        public string Priority { get; set; } = "Normal";
        public string Status { get; set; } = "Active";
        public string TargetAudience { get; set; } = "All";
        public DateTime? ExpiryDate { get; set; }
    }

    // ── Tickets ──────────────────────────────────────────────────────────────
    public class TicketListVM
    {
        public List<TicketItemVM> Tickets { get; set; } = new();
        public List<TicketItemVM> Filtered { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string SelectedStatus { get; set; } = "";
        public string SelectedPriority { get; set; } = "";
        public string SelectedCategory { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)Filtered.Count / PageSize);
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int UrgentTickets { get; set; }
    }

    public class TicketItemVM
    {
        public int Id { get; set; }
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
        public string RequesterName { get; set; } = "";
        public string RequesterEmail { get; set; } = "";
        public string Category { get; set; } = "";
        public string Priority { get; set; } = "Normal";
        public string Status { get; set; } = "Open";
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? Response { get; set; }
    }

    public class TicketRespondVM
    {
        public int TicketId { get; set; }
        public string Response { get; set; } = "";
        public string NewStatus { get; set; } = "In Progress";
    }

    // ── My Profile (SuperAdmin own) ──────────────────────────────────────────
    public class SuperAdminProfileVM
    {
        public string FullName { get; set; } = "Super Admin";
        public string Email { get; set; } = "superadmin@totc.edu";
        public string Phone { get; set; } = "+20 100 000 0000";
        public string Address { get; set; } = "Cairo, Egypt";
        public string? Bio { get; set; }
        public string AccessLevel { get; set; } = "Full System Access";
        public DateTime JoinedDate { get; set; } = new DateTime(2023, 1, 1);
        public string Status { get; set; } = "Active";
        public string Initials => FullName.Length >= 2 ? FullName.Substring(0, 2).ToUpper() : "SA";
    }

    public class EditProfileVM
    {
        [Required] public string FullName { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Bio { get; set; }
    }

    public class ChangePasswordVM
    {
        [Required] public string CurrentPassword { get; set; } = "";
        [Required, MinLength(6)] public string NewPassword { get; set; } = "";
        [Required, Compare("NewPassword")] public string ConfirmPassword { get; set; } = "";
    }

    // ── Statistics ───────────────────────────────────────────────────────────
    public class StatisticsVM
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalParents { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int TotalTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int TotalAnnouncements { get; set; }
        public double SystemUptime { get; set; } = 99.9;

        // Chart data (JSON-serializable)
        public int[] MonthlyRegistrations { get; set; } = new int[12];
        public int[] MonthlyTickets { get; set; } = new int[12];
        public int[] CourseEnrollments { get; set; } = new int[12];

        // Breakdown
        public List<(string Label, int Value, string Color)> UserBreakdown { get; set; } = new();
        public List<(string Label, int Value)> TopCourses { get; set; } = new();
    }
}