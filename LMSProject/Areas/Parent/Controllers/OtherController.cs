using LMSProject.Areas.Parent.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;

// ═══════════════════════════════════════════════════════════════════════════════
// AnnouncementsController
// ═══════════════════════════════════════════════════════════════════════════════
namespace LMSProject.Areas.Parent.Controllers
{
    [Area("Parent")]
    [Authorize(Roles = "Parent")]
    public class AnnouncementsController : Controller
    {
        private readonly AppDbContext _db;

        public AnnouncementsController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string search = "", string priority = "", int page = 1)
        {
            ViewData["Title"] = "Announcements";
            int pageSize = 6;

            var query = _db.Announcements
                .Where(a => a.CurrentState == 1 && a.IsActive
                         && (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now)
                         && (a.TargetAudience == "All" || a.TargetAudience == "Parents"))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a => a.Title.Contains(search) || a.Content.Contains(search));
            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(a => a.Priority == priority);

            var all = await query
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.PublishedDate)
                .ToListAsync();

            var vm = new ParentAnnouncementVM
            {
                Paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = all.Count,
                TotalPages = (int)Math.Ceiling((double)all.Count / pageSize),
                CurrentPage = page,
                SearchTerm = search,
                SelectedPriority = priority,
                UrgentCount = all.Count(a => a.Priority == "Urgent"),
                PinnedCount = all.Count(a => a.IsPinned)
            };
            return View(vm);
        }
    }
}


// ═══════════════════════════════════════════════════════════════════════════════
// ReportController — generates weekly PDF report
// ═══════════════════════════════════════════════════════════════════════════════
namespace LMSProject.Areas.Parent.Controllers
{
    [Area("Parent")]
    [Authorize(Roles = "Parent")]
    public class ReportController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        private readonly LMSProject.AI.Services.GithubAiService _ai;

        public ReportController(AppDbContext db, UserManager<ApplicationUser> um,
            LMSProject.AI.Services.GithubAiService ai)
        { _db = db; _um = um; _ai = ai; }

        private async Task<MLSCore.Models.TbParent?> GetParent()
        {
            var user = await _um.GetUserAsync(User);
            if (user == null) return null;
            return await _db.Parents
                .Include(p => p.Children).ThenInclude(c => c.Grade)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
        }

        // Preview page
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Weekly Progress Report";
            var vm = await BuildReport();
            return View(vm);
        }

        // Download as PDF (rendered HTML → PDF via browser print)
        public async Task<IActionResult> Download()
        {
            var vm = await BuildReport();
            return View("ReportPdf", vm);
        }

        private async Task<WeeklyReportVM> BuildReport()
        {
            var parent = await GetParent();
            if (parent == null) return new WeeklyReportVM();

            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var weekEnd = weekStart.AddDays(6);

            var children = parent.Children.Where(c => c.CurrentState == 1).ToList();
            var childIds = children.Select(c => c.Id).ToList();

            var enrollments = await _db.StudentCourses
                .Where(sc => childIds.Contains(sc.StId))
                .Include(sc => sc.Course)
                .ToListAsync();

            var allCourseIds = enrollments.Select(e => e.CourseId).Distinct().ToList();

            var examResults = await _db.StudentTests
                .Where(st => childIds.Contains(st.StudentId))
                .Include(st => st.Test)
                .OrderByDescending(st => st.JoinDate)
                .ToListAsync();

            var assignments = await _db.Assignments
                .Where(a => allCourseIds.Contains(a.CourseId) && a.CurrentState == 1)
                .Include(a => a.Course)
                .Include(a => a.Submissions.Where(s => childIds.Contains(s.StudentId) && s.CurrentState == 1))
                .ToListAsync();

            var childReports = children.Select(child =>
            {
                var childCourseIds = enrollments.Where(e => e.StId == child.Id).Select(e => e.CourseId).ToList();
                var childExams = examResults.Where(e => e.StudentId == child.Id).ToList();
                var childAssigns = assignments.Where(a => childCourseIds.Contains(a.CourseId)).ToList();
                var childSubs = childAssigns.SelectMany(a => a.Submissions.Where(s => s.StudentId == child.Id)).ToList();
                var weekSubs = childSubs.Where(s => s.SubmittedAt >= weekStart && s.SubmittedAt <= weekEnd).Count();
                double avg = childExams.Any()
                    ? childExams.Average(e => e.Test?.TotalMarks > 0 ? e.Score / e.Test.TotalMarks * 100 : 0) : 0;
                int missing = childAssigns.Count(a =>
                    !childSubs.Any(s => s.AssignmentId == a.Id) && DateTime.Now > a.Deadline);

                return new ChildReportVM
                {
                    FullName = child.FullName,
                    Grade = child.Grade?.Name ?? "",
                    ExamResults = childExams.Select(e => new ExamResultVM
                    {
                        ExamTitle = e.Test?.Title ?? "",
                        Score = e.Score,
                        TotalMarks = e.Test?.TotalMarks ?? 0,
                        TakenAt = e.JoinDate
                    }).ToList(),
                    Assignments = childAssigns.Select(a =>
                    {
                        var sub = childSubs.FirstOrDefault(s => s.AssignmentId == a.Id);
                        return new AssignmentStatusVM
                        {
                            Title = a.Title,
                            CourseName = a.Course?.Name ?? "",
                            Deadline = a.Deadline,
                            TotalMarks = a.TotalMarks,
                            IsSubmitted = sub != null,
                            IsLate = sub?.IsLate ?? false,
                            Score = sub?.Marks,
                            IsGraded = sub?.Marks.HasValue ?? false
                        };
                    }).ToList(),
                    OverallAvg = Math.Round(avg, 1),
                    SubmittedThisWeek = weekSubs,
                    MissingWork = missing,
                    PerformanceSummary = "PENDING_AI"  // replaced below
                };
            }).ToList();

            // Generate AI plain-language summary for each child
            foreach (var child in childReports)
            {
                try
                {
                    // ── Smart context: compare submitted vs published ──────────
                    int totalPublishedAssignments = child.Assignments.Count;
                    int totalSubmitted = child.Assignments.Count(a => a.IsSubmitted);
                    int totalPublishedExams = child.ExamResults.Count; // exams taken
                    // Count all tests published for student courses (need from childReports context)
                    // We pass counts directly so AI doesn't wrongly accuse with empty data

                    bool hasPublishedAssignments = totalPublishedAssignments > 0;
                    bool hasExams = totalPublishedExams > 0;
                    bool submittedBelowHalf = hasPublishedAssignments &&
                                                   totalSubmitted < totalPublishedAssignments / 2.0;

                    var examSummary = hasExams
                        ? string.Join("; ", child.ExamResults.Take(5).Select(e =>
                            $"{e.ExamTitle}: {e.Score}/{e.TotalMarks} " +
                            $"({(e.TotalMarks > 0 ? e.Score / e.TotalMarks * 100 : 0):F0}%)"))
                        : "none";

                    var missingList = child.Assignments
                        .Where(a => !a.IsSubmitted && a.IsExpired)
                        .Select(a => a.Title).Take(3).ToList();

                    var pendingList = child.Assignments
                        .Where(a => !a.IsSubmitted && !a.IsExpired)
                        .Select(a => a.Title).Take(3).ToList();

                    // Build situation-aware prompt
                    var situation = new System.Text.StringBuilder();
                    situation.AppendLine($"Child: {child.FullName} | Grade: {child.Grade}");

                    if (!hasPublishedAssignments && !hasExams)
                    {
                        situation.AppendLine("No assignments or exams have been published yet for this student.");
                        situation.AppendLine("Write a brief encouraging message saying the week has no academic tasks yet.");
                    }
                    else
                    {
                        if (hasExams)
                            situation.AppendLine($"Exams taken: {examSummary} | Average: {child.OverallAvg}%");
                        else
                            situation.AppendLine("No exams have been given yet this period.");

                        if (hasPublishedAssignments)
                        {
                            situation.AppendLine($"Published assignments: {totalPublishedAssignments} | Submitted: {totalSubmitted}");
                            if (missingList.Any())
                                situation.AppendLine($"Overdue/missing: {string.Join(", ", missingList)}");
                            if (pendingList.Any())
                                situation.AppendLine($"Still pending (not due yet): {string.Join(", ", pendingList)}");
                        }
                        else
                        {
                            situation.AppendLine("No assignments have been published yet.");
                        }

                        situation.AppendLine($"Submitted this week: {child.SubmittedThisWeek}");
                        situation.AppendLine();

                        if (!hasPublishedAssignments && !hasExams)
                            situation.AppendLine("Rule: Do NOT mention missing work or low scores — nothing has been assigned yet.");
                        else if (!submittedBelowHalf && !missingList.Any())
                            situation.AppendLine("Rule: The child is on track. Be positive and encouraging.");
                        else if (submittedBelowHalf || missingList.Any())
                            situation.AppendLine("Rule: Gently flag the missing/low submission rate and encourage the parent to follow up.");
                    }

                    situation.AppendLine("Write 2-3 warm plain-language sentences. No bullet points. No lists.");

                    child.PerformanceSummary = await _ai.ChatAsync(
                        new List<(string, string)> { ("user", situation.ToString()) },
                        "You are a school assistant writing parent academic summaries. " +
                        "NEVER mention missing assignments or low performance if no work has been published. " +
                        "Only flag issues when the data actually shows them. Be warm, factual, and concise.");
                }
                catch
                {
                    child.PerformanceSummary = child.OverallAvg switch
                    {
                        >= 85 => "Excellent performance this period. Keep up the great work!",
                        >= 70 => "Good progress. A few areas could use more attention.",
                        >= 50 => "Average performance. Encourage more regular study.",
                        _ => "This period needs more focus. Please check in with the teacher."
                    };
                }
            }


            // AI: generate plain-language summary per child
            foreach (var child in childReports)
            {
                try
                {
                    var examSummary = child.ExamResults.Any()
                        ? string.Join("; ", child.ExamResults.Take(5)
                            .Select(e => $"{e.ExamTitle}: {e.Score}/{e.TotalMarks}"))
                        : "no exams taken";
                    var missingTitles = child.Assignments.Where(a => !a.IsSubmitted && a.IsExpired).Select(a => a.Title).Take(3);
                    var pendingTitles = child.Assignments.Where(a => !a.IsSubmitted && !a.IsExpired).Select(a => a.Title).Take(3);
                    var prompt =
                        $"Write 2-3 warm plain-language sentences for a parent about their child's school week.\n" +
                        $"Child: {child.FullName} | Grade: {child.Grade} | Average: {child.OverallAvg}%\n" +
                        $"Exams: {examSummary}\n" +
                        $"Missing: {(missingTitles.Any() ? string.Join(", ", missingTitles) : "none")}\n" +
                        $"Pending: {(pendingTitles.Any() ? string.Join(", ", pendingTitles) : "none")}\n" +
                        $"Submitted this week: {child.SubmittedThisWeek}\n" +
                        "Be direct and supportive. Plain sentences only, no lists.";
                    child.PerformanceSummary = await _ai.ChatAsync(
                        new List<(string, string)> { ("user", prompt) },
                        "You are a school assistant writing brief academic summaries for parents. Be warm and factual. Maximum 3 sentences.");
                }
                catch
                {
                    child.PerformanceSummary = child.OverallAvg switch
                    {
                        >= 85 => "Excellent performance this period. Keep it up!",
                        >= 70 => "Good overall progress with some room to improve.",
                        >= 50 => "Average performance — encourage more regular study.",
                        _ => "This period needs more attention. Consider speaking with the teacher."
                    };
                }
            }

            return new WeeklyReportVM
            {
                ParentName = parent.FullName,
                ReportDate = DateTime.Now,
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                Children = childReports
            };
        }
    }
}