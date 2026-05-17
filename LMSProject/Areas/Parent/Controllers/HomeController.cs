using LMSProject.Areas.Parent.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;

namespace LMSProject.Areas.Parent.Controllers
{
    [Area("Parent")]
    [Authorize(Roles = "Parent")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public HomeController(AppDbContext db, UserManager<ApplicationUser> um)
        { _db = db; _um = um; }

        private async Task<MLSCore.Models.TbParent?> GetParent()
        {
            var user = await _um.GetUserAsync(User);
            if (user == null) return null;
            return await _db.Parents
                .Include(p => p.Children).ThenInclude(c => c.Grade)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Parent Dashboard";
            var parent = await GetParent();
            if (parent == null) return RedirectToAction("Login", "Account", new { area = "" });

            var children = parent.Children.Where(c => c.CurrentState == 1).ToList();
            var childIds = children.Select(c => c.Id).ToList();

            // Course count across all children
            var enrollments = await _db.StudentCourses
                .Where(sc => childIds.Contains(sc.StId))
                .Include(sc => sc.Course)
                .ToListAsync();
            int totalCourses = enrollments.Select(e => e.CourseId).Distinct().Count();

            // Exam results
            var examResults = await _db.StudentTests
                .Where(st => childIds.Contains(st.StudentId))
                .Include(st => st.Test).Include(st => st.Student)
                .OrderByDescending(st => st.JoinDate)
                .ToListAsync();

            // Assignments
            var courseIds = enrollments.Select(e => e.CourseId).Distinct().ToList();
            var assignments = await _db.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1)
                .Include(a => a.Submissions)
                .Include(a => a.Course)
                .ToListAsync();

            // Pending exams (active tests not yet taken by at least one child)
            var takenTestIds = examResults.Select(e => e.TestId).ToHashSet();
            var activeTests = await _db.Tests
                .Where(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1
                         && t.Deadline > DateTime.Now)
                .ToListAsync();
            int pendingExams = activeTests.Count(t => !takenTestIds.Contains(t.Id));

            // Pending assignments (not submitted by any child, not expired)
            int pendingAssignments = assignments.Count(a =>
                a.Deadline > DateTime.Now &&
                !a.Submissions.Any(s => childIds.Contains(s.StudentId) && s.CurrentState == 1));

            // Build child summary cards
            var childSummaries = children.Select(child =>
            {
                var childExams = examResults.Where(e => e.StudentId == child.Id).ToList();
                var childSubs = assignments.SelectMany(a => a.Submissions)
                    .Where(s => s.StudentId == child.Id && s.CurrentState == 1).ToList();
                double examAvg = childExams.Any() ? childExams.Average(e => e.Test != null && e.Test.TotalMarks > 0
                    ? e.Score / e.Test.TotalMarks * 100 : 0) : 0;
                double assAvg = childSubs.Any(s => s.Marks.HasValue)
                    ? childSubs.Where(s => s.Marks.HasValue)
                        .Average(s => s.Marks!.Value / (assignments
                            .First(a => a.Id == s.AssignmentId).TotalMarks) * 100) : 0;

                return new ChildSummaryVM
                {
                    StudentId = child.Id,
                    FullName = child.FullName,
                    ImageName = child.ImageName,
                    Grade = child.Grade?.Name ?? "",
                    CourseCount = enrollments.Count(e => e.StId == child.Id),
                    ExamsTaken = childExams.Count,
                    AssignmentsSubmitted = childSubs.Count,
                    AverageExamScore = Math.Round(examAvg, 1),
                    AssignmentAvgScore = Math.Round(assAvg, 1)
                };
            }).ToList();

            // Upcoming deadlines (next 7 days, all children)
            var deadlines = new List<UpcomingDeadlineVM>();
            foreach (var child in children)
            {
                var childCourseIds = enrollments.Where(e => e.StId == child.Id).Select(e => e.CourseId).ToList();

                var upcomingTests = activeTests.Where(t => childCourseIds.Contains(t.CourseId)
                    && !examResults.Any(e => e.StudentId == child.Id && e.TestId == t.Id));
                foreach (var t in upcomingTests)
                    deadlines.Add(new UpcomingDeadlineVM
                    {
                        Title = t.Title,
                        Type = "Exam",
                        ChildName = child.FullName,
                        CourseName = enrollments.FirstOrDefault(e => e.CourseId == t.CourseId)?.Course?.Name ?? "",
                        Deadline = t.Deadline ?? DateTime.MaxValue,
                        IsSubmitted = false
                    });

                var upcomingAss = assignments.Where(a => childCourseIds.Contains(a.CourseId)
                    && a.Deadline > DateTime.Now
                    && !a.Submissions.Any(s => s.StudentId == child.Id && s.CurrentState == 1));
                foreach (var a in upcomingAss)
                    deadlines.Add(new UpcomingDeadlineVM
                    {
                        Title = a.Title,
                        Type = "Assignment",
                        ChildName = child.FullName,
                        CourseName = a.Course?.Name ?? "",
                        Deadline = a.Deadline,
                        IsSubmitted = false
                    });
            }

            // Recent results (last 5)
            var recentResults = examResults.Take(5).Select(e => new RecentResultVM
            {
                Title = e.Test?.Title ?? "",
                Type = "Exam",
                ChildName = e.Student?.FullName ?? "",
                CourseName = "",
                Score = e.Score,
                TotalMarks = e.Test?.TotalMarks ?? 0,
                Date = e.JoinDate
            }).ToList();

            var vm = new ParentDashboardVM
            {
                ParentName = parent.FullName,
                ImageName = parent.ImageName,
                ChildrenCount = children.Count,
                TotalCourses = totalCourses,
                PendingExams = pendingExams,
                PendingAssignments = pendingAssignments,
                TotalSubmissions = assignments.Sum(a => a.Submissions.Count(s => childIds.Contains(s.StudentId))),
                GradedSubmissions = assignments.Sum(a => a.Submissions.Count(s => childIds.Contains(s.StudentId) && s.Marks.HasValue)),
                Announcements = await _db.Announcements.CountAsync(a => a.CurrentState == 1 && a.IsActive),
                Children = childSummaries,
                UpcomingDeadlines = deadlines.OrderBy(d => d.Deadline).Take(8).ToList(),
                RecentResults = recentResults
            };

            return View(vm);
        }
    }
}