using LMSProject.Areas.Student.ViewModels;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ExamsController : BaseController
    {
        private readonly AppDbContext _db;

        public ExamsController(AppDbContext db, UserManager<ApplicationUser> um) : base(um) => _db = db;

        private async Task<int> GetStudentId()
        {
            var user = await _userManager.GetUserAsync(User);
            var s = await _db.Students.FirstOrDefaultAsync(x => x.UserId == user!.Id && x.CurrentState == 1);
            return s?.Id ?? 0;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Exams";
            var sid = await GetStudentId();
            var courseIds = await _db.StudentCourses.Where(sc => sc.StId == sid).Select(sc => sc.CourseId).ToListAsync();
            var exams = await _db.Tests.Where(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1).Include(t => t.Questions).OrderByDescending(t => t.Deadline).ToListAsync();
            var results = await _db.StudentTests.Where(st => st.StudentId == sid).ToDictionaryAsync(st => st.TestId);
            var courseNames = await _db.Courses.Where(c => courseIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);

            var vm = exams.Select(t => new ExamVM
            {
                ExamId = t.Id,
                Title = t.Title,
                CourseId = t.CourseId,
                CourseName = courseNames.GetValueOrDefault(t.CourseId, ""),
                DurationMinutes = t.DurationInMinutes,
                TotalMarks = t.TotalMarks,
                Deadline = t.Deadline,
                QuestionCount = t.Questions?.Count(q => q.CurrentState == 1) ?? 0,
                IsSubmitted = results.ContainsKey(t.Id),
                Score = results.TryGetValue(t.Id, out var r) ? r.Score : null,
                TakenAt = results.TryGetValue(t.Id, out var r2) ? r2.JoinDate : null
            }).ToList();

            return View("~/Areas/Student/Views/Exams/Index.cshtml", vm);
        }

        public async Task<IActionResult> Take(int id)
        {
            var sid = await GetStudentId();
            var already = await _db.StudentTests.AnyAsync(st => st.StudentId == sid && st.TestId == id);
            if (already)
            {
                TempData["Error"] = "You have already submitted this exam.";
                return RedirectToAction("Index");
            }

            var exam = await _db.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(t => t.Id == id && t.CurrentState == 1);

            if (exam is null) return NotFound();

            if (exam.Deadline.HasValue && DateTime.Now > exam.Deadline.Value)
            {
                TempData["Error"] = "This exam has expired.";
                return RedirectToAction("Index");
            }

            var courseIds = await _db.StudentCourses.Where(sc => sc.StId == sid).Select(sc => sc.CourseId).ToListAsync();
            if (!courseIds.Contains(exam.CourseId)) return Forbid();

            var courseName = (await _db.Courses.FindAsync(exam.CourseId))?.Name ?? "";

            var vm = new TakeExamVM
            {
                Exam = exam,
                Questions = exam.Questions?.Where(q => q.CurrentState == 1).ToList() ?? new(),
                CourseName = courseName
            };

            ViewData["Title"] = $"Exam — {exam.Title}";
            return View("~/Areas/Student/Views/Exams/Take.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitExamVM vm)
        {
            var sid = await GetStudentId();
            var exam = await _db.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(t => t.Id == vm.ExamId);

            if (exam is null) return NotFound();

            double score = 0;
            var questions = exam.Questions?.Where(q => q.CurrentState == 1).ToList() ?? new();
            double perQ = exam.TotalMarks / Math.Max(1, questions.Count);

            foreach (var ans in vm.Answers)
            {
                var q = questions.FirstOrDefault(x => x.Id == ans.QuestionId);
                var ch = q?.Choices?.FirstOrDefault(c => c.Id == ans.ChoiceId);
                if (ch?.Correct == true) score += perQ;
            }

            _db.StudentTests.Add(new TbStudentTest
            {
                StudentId = sid,
                TestId = vm.ExamId,
                Score = Math.Round(score, 2),
                JoinDate = DateTime.Now,
                TimeInMinutes = exam.DurationInMinutes
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Exam submitted! Your score: {Math.Round(score, 1)} / {exam.TotalMarks}";
            return RedirectToAction("Index");
        }
    }
}