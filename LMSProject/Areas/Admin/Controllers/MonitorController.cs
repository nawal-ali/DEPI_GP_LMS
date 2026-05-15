using LMSProject.Areas.Admin.ViewModels;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class MonitorController : BaseController
    {
        private readonly AppDbContext _db;

        public MonitorController(AppDbContext db, UserManager<ApplicationUser> um) : base(um) => _db = db;

        // ── Materials ──────────────────────────────────────────────────────
        public async Task<IActionResult> Materials(string search = "", string type = "", int courseId = 0)
        {
            ViewData["Title"] = "Content Monitor — Materials";

            var query = _db.CourseMaterials
                .Include(m => m.Course)
                .Where(m => m.CurrentState == 1)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Title.Contains(search) || m.Course.Name.Contains(search));

            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<MaterialType>(type, out var mt))
                query = query.Where(m => m.MaterialType == mt);

            if (courseId > 0)
                query = query.Where(m => m.CourseId == courseId);

            var items = await query.OrderByDescending(m => m.CreatedDate).ToListAsync();

            var vm = items.Select(m => new MaterialMonitorVM
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                CourseName = m.Course?.Name ?? "",
                CourseId = m.CourseId,
                FileName = m.FileName,
                FileUrl = m.FileUrl ?? "",
                MaterialType = m.MaterialType,
                CreatedDate = m.CreatedDate
            }).ToList();

            ViewBag.SearchTerm = search;
            ViewBag.TypeFilter = type;
            ViewBag.CourseId = courseId;
            ViewBag.Courses = await _db.Courses
                .Where(c => c.CurrentState == 1)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View(vm);
        }

        // ── Exams ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Exams(string search = "", int courseId = 0)
        {
            ViewData["Title"] = "Content Monitor — Exams";

            var query = _db.Tests
                .Include(t => t.Questions)
                .Where(t => t.CurrentState == 1)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Title.Contains(search));

            if (courseId > 0)
                query = query.Where(t => t.CourseId == courseId);

            var tests = await query.ToListAsync();

            var testIds = tests.Select(t => t.Id).ToList();

            var attemptData = await _db.StudentTests
                .Where(st => testIds.Contains(st.TestId))
                .GroupBy(st => st.TestId)
                .Select(g => new
                {
                    TestId = g.Key,
                    Count = g.Count(),
                    AvgScore = g.Average(x => (double?)x.Score) ?? 0
                })
                .ToDictionaryAsync(g => g.TestId);

            var courseNames = await _db.Courses
                .Where(c => tests.Select(t => t.CourseId).Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            var vm = tests.Select(t => new ExamMonitorVM
            {
                Id = t.Id,
                Title = t.Title,
                CourseName = courseNames.GetValueOrDefault(t.CourseId, ""),
                CourseId = t.CourseId,
                Deadline = t.Deadline,
                TotalMarks = t.TotalMarks,
                DurationMinutes = t.DurationInMinutes,
                QuestionCount = t.Questions?.Count(q => q.CurrentState == 1) ?? 0,
                AttemptCount = attemptData.GetValueOrDefault(t.Id)?.Count ?? 0,
                AverageScore = attemptData.GetValueOrDefault(t.Id)?.AvgScore ?? 0
            }).ToList();

            ViewBag.SearchTerm = search;
            ViewBag.CourseId = courseId;
            ViewBag.Courses = await _db.Courses
                .Where(c => c.CurrentState == 1)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View(vm);
        }

        // ── Exam Results ───────────────────────────────────────────────────
        public async Task<IActionResult> ExamResults(int examId)
        {
            ViewData["Title"] = "Exam Results";

            var exam = await _db.Tests.FindAsync(examId);
            if (exam == null) return NotFound();

            var results = await _db.StudentTests
                .Where(st => st.TestId == examId)
                .Include(st => st.Student)
                .OrderByDescending(st => st.Score)
                .ToListAsync();

            ViewBag.Exam = exam;
            ViewBag.Results = results;
            return View();
        }

        // ── Assignments ────────────────────────────────────────────────────
        public async Task<IActionResult> Assignments(string search = "", int courseId = 0)
        {
            ViewData["Title"] = "Content Monitor — Assignments";

            var query = _db.Assignments
                .Include(a => a.Course)
                .Include(a => a.Submissions)
                .Where(a => a.CurrentState == 1)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a => a.Title.Contains(search) || a.Course.Name.Contains(search));

            if (courseId > 0)
                query = query.Where(a => a.CourseId == courseId);

            var all = await query.ToListAsync();

            var vm = all.Select(a => new AssignmentMonitorVM
            {
                Id = a.Id,
                Title = a.Title,
                CourseName = a.Course?.Name ?? "",
                CourseId = a.CourseId,
                Deadline = a.Deadline,
                TotalMarks = a.TotalMarks,
                SubmissionCount = a.Submissions?.Count ?? 0,
                GradedCount = a.Submissions?.Count(s => s.Marks != null) ?? 0,
                LateCount = a.Submissions?.Count(s => s.IsLate) ?? 0
            }).ToList();

            ViewBag.SearchTerm = search;
            ViewBag.CourseId = courseId;
            ViewBag.Courses = await _db.Courses
                .Where(c => c.CurrentState == 1)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View(vm);
        }

        // ── Assignment Submissions ─────────────────────────────────────────
        public async Task<IActionResult> AssignmentSubmissions(int assignmentId)
        {
            ViewData["Title"] = "Assignment Submissions";

            var assignment = await _db.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) return NotFound();

            var subs = await _db.AssignmentSubmissions
                .Where(s => s.AssignmentId == assignmentId && s.CurrentState == 1)
                .Include(s => s.Student)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            int enrolled = await _db.StudentCourses
                .CountAsync(sc => sc.CourseId == assignment.CourseId);

            var vm = new AssignmentSubmissionsVM
            {
                Assignment = assignment,
                TotalEnrolled = enrolled,
                Submissions = subs.Select(s => new SubmissionRowVM
                {
                    SubmissionId = s.Id,
                    StudentName = s.Student?.FullName ?? "",
                    TextAnswer = s.TextAnswer,
                    FileUrl = s.FileUrl,
                    FileName = s.FileName,
                    SubmittedAt = s.SubmittedAt,
                    IsLate = s.IsLate,
                    Marks = s.Marks,
                    Feedback = s.InstructorFeedback,
                    IsGraded = s.Marks != null
                }).ToList()
            };

            return View(vm);
        }
    }
}