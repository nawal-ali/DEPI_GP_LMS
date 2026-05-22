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
                IsPending = results.TryGetValue(t.Id, out var rp) && rp.IsPending,
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

            var questions = exam.Questions?.Where(q => q.CurrentState == 1).ToList() ?? new();
            bool hasText = questions.Any(q => q.QuestionType == MLSCore.Models.QuestionType.Text);
            double mcqScore = 0;

            var studentTest = new TbStudentTest
            {
                StudentId = sid,
                TestId = vm.ExamId,
                Score = 0,
                IsPending = hasText,
                JoinDate = DateTime.Now,
                TimeInMinutes = exam.DurationInMinutes
            };
            _db.StudentTests.Add(studentTest);
            await _db.SaveChangesAsync();

            var answerRecords = new List<TbStudentAnswer>();
            var selectedChoices = new List<TbSelectedChoice>();

            foreach (var q in questions)
            {
                var ansVM = vm.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
                double qScore = 0;
                bool marked = q.QuestionType != MLSCore.Models.QuestionType.Text;

                var answer = new TbStudentAnswer
                {
                    StudentTestId = studentTest.Id,
                    QuestionId = q.Id,
                    TextAnswer = q.QuestionType == MLSCore.Models.QuestionType.Text ? ansVM?.TextAnswer ?? "" : "",
                    Score = 0,
                    Marked = marked
                };

                if (q.QuestionType != MLSCore.Models.QuestionType.Text && ansVM != null && ansVM.ChoiceId > 0)
                {
                    var ch = q.Choices?.FirstOrDefault(c => c.Id == ansVM.ChoiceId);
                    if (ch?.Correct == true)
                    {
                        qScore = q.Pints;
                        mcqScore += qScore;
                    }
                    answer.Score = qScore;
                    answerRecords.Add(answer);
                    _db.StudentAnswers.Add(answer);
                    await _db.SaveChangesAsync();
                    selectedChoices.Add(new TbSelectedChoice
                    {
                        StAnswerId = answer.Id,
                        ChoiceId = ansVM.ChoiceId,
                        Score = (int)qScore
                    });
                }
                else
                {
                    answerRecords.Add(answer);
                    _db.StudentAnswers.Add(answer);
                    await _db.SaveChangesAsync();
                }
            }

            if (selectedChoices.Any())
            {
                _db.SelectedChoices.AddRange(selectedChoices);
            }

            studentTest.Score = Math.Round(mcqScore, 2);
            await _db.SaveChangesAsync();

            var msg = hasText
                ? $"Exam submitted! MCQ score so far: {Math.Round(mcqScore, 1)} / {exam.TotalMarks}. Text answers are pending instructor review."
                : $"Exam submitted! Your score: {Math.Round(mcqScore, 1)} / {exam.TotalMarks}";
            TempData["Success"] = msg;
            return RedirectToAction("Index");
        }
    }
}