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
    public class ChildrenController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public ChildrenController(AppDbContext db, UserManager<ApplicationUser> um)
        { _db = db; _um = um; }

        private async Task<MLSCore.Models.TbParent?> GetParent()
        {
            var user = await _um.GetUserAsync(User);
            if (user == null) return null;
            return await _db.Parents
                .Include(p => p.Children).ThenInclude(c => c.Grade)
                .Include(p => p.Children).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
        }

        // ── Index: list all children ───────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "My Children";
            var parent = await GetParent();
            if (parent == null) return RedirectToAction("Index", "Home");

            var children = parent.Children.Where(c => c.CurrentState == 1).ToList();
            var childIds = children.Select(c => c.Id).ToList();

            var examResults = await _db.StudentTests
                .Where(st => childIds.Contains(st.StudentId))
                .Include(st => st.Test)
                .ToListAsync();

            var enrollments = await _db.StudentCourses
                .Where(sc => childIds.Contains(sc.StId))
                .ToListAsync();

            var courseIds = enrollments.Select(e => e.CourseId).Distinct().ToList();
            var assignments = await _db.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1)
                .Include(a => a.Submissions)
                .ToListAsync();

            var summaries = children.Select(child =>
            {
                var childExams = examResults.Where(e => e.StudentId == child.Id).ToList();
                double examAvg = childExams.Any()
                    ? childExams.Average(e => e.Test != null && e.Test.TotalMarks > 0
                        ? e.Score / e.Test.TotalMarks * 100 : 0) : 0;

                return new ChildSummaryVM
                {
                    StudentId = child.Id,
                    FullName = child.FullName,
                    ImageName = child.ImageName,
                    Grade = child.Grade?.Name ?? "",
                    CourseCount = enrollments.Count(e => e.StId == child.Id),
                    ExamsTaken = childExams.Count,
                    AssignmentsSubmitted = assignments.Sum(a => a.Submissions.Count(s => s.StudentId == child.Id && s.CurrentState == 1)),
                    AverageExamScore = Math.Round(examAvg, 1)
                };
            }).ToList();

            return View(summaries);
        }

        // ── Detail: full academic profile of one child ─────────────────────
        public async Task<IActionResult> Detail(int studentId)
        {
            ViewData["Title"] = "Child Academic Profile";
            var parent = await GetParent();
            if (parent == null) return RedirectToAction("Index", "Home");

            // Security: make sure this student belongs to this parent
            var child = parent.Children.FirstOrDefault(c => c.Id == studentId && c.CurrentState == 1);
            if (child == null) return Forbid();

            var enrollments = await _db.StudentCourses
                .Where(sc => sc.StId == studentId)
                .Include(sc => sc.Course).ThenInclude(c => c.Instructor)
                .ToListAsync();

            var courseIds = enrollments.Select(e => e.CourseId).ToList();

            // Exams
            var examResults = await _db.StudentTests
                .Where(st => st.StudentId == studentId)
                .Include(st => st.Test).ThenInclude(t => t.Questions)
                .OrderByDescending(st => st.JoinDate)
                .ToListAsync();

            // Assignments
            var assignments = await _db.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1)
                .Include(a => a.Course)
                .Include(a => a.Submissions.Where(s => s.StudentId == studentId && s.CurrentState == 1))
                .OrderByDescending(a => a.Deadline)
                .ToListAsync();

            // Materials count per course
            var materialCounts = await _db.CourseMaterials
                .Where(m => courseIds.Contains(m.CourseId) && m.CurrentState == 1)
                .GroupBy(m => m.CourseId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => g.Count);

            // Exam count per course
            var examCounts = await _db.Tests
                .Where(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1)
                .GroupBy(t => t.CourseId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => g.Count);

            // Build course progress
            var courseProgress = enrollments.Select(en =>
            {
                var courseExamIds = examResults
                    .Where(er => er.Test?.CourseId == en.CourseId).ToList();
                double examAvg = courseExamIds.Any()
                    ? courseExamIds.Average(e => e.Test != null && e.Test.TotalMarks > 0
                        ? e.Score / e.Test.TotalMarks * 100 : 0) : 0;

                var courseAssignments = assignments.Where(a => a.CourseId == en.CourseId).ToList();
                var gradedSubs = courseAssignments
                    .SelectMany(a => a.Submissions)
                    .Where(s => s.Marks.HasValue).ToList();
                double assAvg = gradedSubs.Any()
                    ? gradedSubs.Average(s =>
                    {
                        var totalMarks = assignments.FirstOrDefault(a => a.Id == s.AssignmentId)?.TotalMarks ?? 1;
                        return s.Marks!.Value / totalMarks * 100;
                    }) : 0;

                return new CourseProgressVM
                {
                    CourseId = en.CourseId,
                    CourseName = en.Course?.Name ?? "",
                    InstructorName = en.Course?.Instructor?.FullName ?? "",
                    ExamCount = examCounts.GetValueOrDefault(en.CourseId),
                    AssignmentCount = courseAssignments.Count,
                    MaterialCount = materialCounts.GetValueOrDefault(en.CourseId),
                    ExamAvg = Math.Round(examAvg, 1),
                    AssignmentAvg = Math.Round(assAvg, 1)
                };
            }).ToList();

            // Build exam result VMs
            var examVMs = examResults.Select(e =>
            {
                var courseEnroll = enrollments.FirstOrDefault(en => en.CourseId == e.Test?.CourseId);
                return new ExamResultVM
                {
                    StudentTestId = e.Id,
                    ExamTitle = e.Test?.Title ?? "",
                    CourseName = courseEnroll?.Course?.Name ?? "",
                    Score = e.Score,
                    TotalMarks = e.Test?.TotalMarks ?? 0,
                    TakenAt = e.JoinDate,
                    Deadline = e.Test?.Deadline,
                    IsSubmitted = true,
                    IsLate = e.Test?.Deadline.HasValue == true && e.JoinDate > e.Test.Deadline!.Value
                };
            }).ToList();

            // Build assignment status VMs
            var assignmentVMs = assignments.Select(a =>
            {
                var sub = a.Submissions.FirstOrDefault();
                return new AssignmentStatusVM
                {
                    AssignmentId = a.Id,
                    Title = a.Title,
                    CourseName = a.Course?.Name ?? "",
                    Deadline = a.Deadline,
                    TotalMarks = a.TotalMarks,
                    IsSubmitted = sub != null,
                    IsLate = sub?.IsLate ?? false,
                    Score = sub?.Marks,
                    IsGraded = sub?.Marks.HasValue ?? false
                };
            }).ToList();

            var vm = new ChildDetailVM
            {
                StudentId = child.Id,
                FullName = child.FullName,
                ImageName = child.ImageName,
                Grade = child.Grade?.Name ?? "",
                Email = child.User?.Email,
                Phone = child.User?.PhoneNumber,
                Courses = courseProgress,
                ExamResults = examVMs,
                Assignments = assignmentVMs
            };

            return View(vm);
        }
    }
}