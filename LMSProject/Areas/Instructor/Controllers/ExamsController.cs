using LMSProject.Areas.Instructor.ViewModel;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class ExamsController : BaseController
    {
        private readonly AppDbContext _context;

        public ExamsController(AppDbContext context, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _context = context;
        }

        private async Task<int?> GetInstructorIdAsync()
        {
            var userId = CurrentUserId;
            return await _context.Instructors
                .Where(i => i.UserId == userId)
                .Select(i => (int?)i.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<List<int>> GetMyCourseIdsAsync(int instructorId)
        {
            return await _context.Courses
                .Where(c => c.InstructorId == instructorId && c.CurrentState == 1)
                .Select(c => c.Id)
                .ToListAsync();
        }

        private async Task<List<CourseDropItem>> GetMyCoursesDropAsync(int instructorId)
        {
            return await _context.Courses
                .Where(c => c.InstructorId == instructorId && c.CurrentState == 1)
                .Select(c => new CourseDropItem { Id = c.Id, Name = c.Name })
                .ToListAsync();
        }

        public async Task<IActionResult> Index()
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            // Guard: instructor has no assigned courses
            var hasCourses = await _context.Courses
                .AnyAsync(c => c.InstructorId == instructorId.Value && c.CurrentState == 1);
            if (!hasCourses)
            {
                ViewData["Title"] = "Exams";
                return View("~/Areas/Instructor/Views/Shared/_NoCourseAccess.cshtml", "Exams");
            }

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var courseNames = await _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            var tests = await _context.Tests
                .Where(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1)
                .Include(t => t.Questions)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            var attemptCounts = await _context.StudentTests
                .Where(st => tests.Select(t => t.Id).Contains(st.TestId))
                .GroupBy(st => st.TestId)
                .Select(g => new { TestId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TestId, x => x.Count);

            var vm = tests.Select(t => new ExamListItemVM
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                CourseName = courseNames.GetValueOrDefault(t.CourseId, ""),
                CourseId = t.CourseId,
                DurationInMinutes = t.DurationInMinutes,
                Deadline = t.Deadline,
                TotalMarks = t.TotalMarks,
                QuestionCount = t.Questions?.Count(q => q.CurrentState == 1) ?? 0,
                AttemptCount = attemptCounts.GetValueOrDefault(t.Id, 0)
            }).ToList();

            ViewData["Title"] = "Exams";
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var vm = new CreateExamVM
            {
                Courses = await GetMyCoursesDropAsync(instructorId.Value),
                DurationInMinutes = 60,
                TotalMarks = 100
            };
            ViewData["Title"] = "Create Exam";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateExamVM vm)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            if (!courseIds.Contains(vm.CourseId))
                ModelState.AddModelError("CourseId", "Invalid course selection.");

            if (!ModelState.IsValid)
            {
                vm.Courses = await GetMyCoursesDropAsync(instructorId.Value);
                ViewData["Title"] = "Create Exam";
                return View(vm);
            }

            var exam = new TbTest
            {
                Title = vm.Title,
                Description = vm.Description ?? "",
                CourseId = vm.CourseId,
                DurationInMinutes = vm.DurationInMinutes,
                Deadline = vm.Deadline,
                TotalMarks = vm.TotalMarks,
                CreatedBy = CurrentUserId,
                CurrentState = 1
            };

            _context.Tests.Add(exam);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Exam created. Now add questions to the question bank.";
            return RedirectToAction(nameof(Details), new { id = exam.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var exam = await _context.Tests
                .FirstOrDefaultAsync(t => t.Id == id && courseIds.Contains(t.CourseId) && t.CurrentState == 1);

            if (exam == null) return NotFound();

            var vm = new EditExamVM
            {
                Id = exam.Id,
                Title = exam.Title,
                Description = exam.Description,
                CourseId = exam.CourseId,
                DurationInMinutes = exam.DurationInMinutes,
                Deadline = exam.Deadline,
                TotalMarks = exam.TotalMarks,
                Courses = await GetMyCoursesDropAsync(instructorId.Value)
            };
            ViewData["Title"] = "Edit Exam";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditExamVM vm)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var exam = await _context.Tests
                .FirstOrDefaultAsync(t => t.Id == vm.Id && courseIds.Contains(t.CourseId));

            if (exam == null) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.Courses = await GetMyCoursesDropAsync(instructorId.Value);
                ViewData["Title"] = "Edit Exam";
                return View(vm);
            }

            exam.Title = vm.Title;
            exam.Description = vm.Description ?? "";
            exam.CourseId = vm.CourseId;
            exam.DurationInMinutes = vm.DurationInMinutes;
            exam.Deadline = vm.Deadline;
            exam.TotalMarks = vm.TotalMarks;
            exam.UpdatedBy = CurrentUserId;
            exam.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Exam updated successfully.";
            return RedirectToAction(nameof(Details), new { id = exam.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var exam = await _context.Tests
                .Include(t => t.Questions.Where(q => q.CurrentState == 1))
                    .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(t => t.Id == id && courseIds.Contains(t.CourseId) && t.CurrentState == 1);

            if (exam == null) return NotFound();

            var courseName = await _context.Courses
                .Where(c => c.Id == exam.CourseId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync() ?? "";

            var attemptCount = await _context.StudentTests
                .CountAsync(st => st.TestId == id);

            var vm = new ExamDetailsVM
            {
                Id = exam.Id,
                Title = exam.Title,
                Description = exam.Description,
                CourseName = courseName,
                DurationInMinutes = exam.DurationInMinutes,
                Deadline = exam.Deadline,
                TotalMarks = exam.TotalMarks,
                AttemptCount = attemptCount,
                Questions = exam.Questions?.Select(q => new QuestionListItemVM
                {
                    Id = q.Id,
                    Title = q.Title,
                    QuestionType = q.QuestionType,
                    Points = q.Pints,
                    Choices = q.Choices?.Select(c => new ChoiceVM
                    {
                        Id = c.Id,
                        Title = c.Title,
                        IsCorrect = c.Correct
                    }).ToList() ?? new()
                }).ToList() ?? new()
            };

            ViewData["Title"] = $"Exam: {exam.Title}";
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AddQuestion(int examId)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var exam = await _context.Tests
                .FirstOrDefaultAsync(t => t.Id == examId && courseIds.Contains(t.CourseId) && t.CurrentState == 1);

            if (exam == null) return NotFound();

            var usedPoints = await _context.TestsQuestion
                .Where(q => q.TestId == examId && q.CurrentState == 1)
                .SumAsync(q => q.Pints);

            var vm = new CreateQuestionVM { TestId = examId, Points = 1 };
            ViewBag.ExamTitle = exam.Title;
            ViewBag.TotalMarks = (int)exam.TotalMarks;
            ViewBag.UsedPoints = usedPoints;
            ViewBag.RemainingPoints = (int)exam.TotalMarks - usedPoints;
            ViewData["Title"] = "Add Question";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(CreateQuestionVM vm)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var exam = await _context.Tests
                .FirstOrDefaultAsync(t => t.Id == vm.TestId && courseIds.Contains(t.CourseId));

            if (exam == null) return NotFound();

            var usedPoints = await _context.TestsQuestion
                .Where(q => q.TestId == vm.TestId && q.CurrentState == 1)
                .SumAsync(q => q.Pints);
            var remaining = (int)exam.TotalMarks - usedPoints;

            if (vm.Points > remaining)
                ModelState.AddModelError("Points", $"Only {remaining} mark(s) remaining out of {exam.TotalMarks} total. Reduce question points.");

            if (vm.QuestionType == QuestionType.Choice || vm.QuestionType == QuestionType.MultiChoice)
            {
                if (string.IsNullOrWhiteSpace(vm.ChoiceA) || string.IsNullOrWhiteSpace(vm.ChoiceB))
                    ModelState.AddModelError("ChoiceA", "At least two answer choices are required.");
                if (string.IsNullOrWhiteSpace(vm.CorrectChoice))
                    ModelState.AddModelError("CorrectChoice", "Please mark a correct answer.");
            }

            if (!ModelState.IsValid)
            {
                var examForTitle = await _context.Tests.FindAsync(vm.TestId);
                ViewBag.ExamTitle = examForTitle?.Title;
                ViewBag.TotalMarks = (int)(examForTitle?.TotalMarks ?? 0);
                ViewBag.UsedPoints = usedPoints;
                ViewBag.RemainingPoints = remaining;
                ViewData["Title"] = "Add Question";
                return View(vm);
            }

            var question = new TbTestQuestion
            {
                Title = vm.Title,
                Description = vm.Description ?? "",
                Pints = vm.Points,
                QuestionType = vm.QuestionType,
                TestId = vm.TestId,
                CreatedBy = CurrentUserId,
                CurrentState = 1
            };

            _context.TestsQuestion.Add(question);
            await _context.SaveChangesAsync();

            // Add choices for MCQ questions
            if (vm.QuestionType == QuestionType.Choice || vm.QuestionType == QuestionType.MultiChoice)
            {
                var choices = new[]
                {
                    (vm.ChoiceA, "A"),
                    (vm.ChoiceB, "B"),
                    (vm.ChoiceC, "C"),
                    (vm.ChoiceD, "D")
                }
                .Where(c => !string.IsNullOrWhiteSpace(c.Item1))
                .Select(c => new TbChoice
                {
                    Title = c.Item1!,
                    Correct = c.Item2 == vm.CorrectChoice,
                    QuestionId = question.Id
                });

                _context.Choices.AddRange(choices);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Question added to question bank.";
            return RedirectToAction(nameof(Details), new { id = vm.TestId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int questionId, int examId)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var question = await _context.TestsQuestion
                .Include(q => q.Test)
                .FirstOrDefaultAsync(q => q.Id == questionId && courseIds.Contains(q.Test.CourseId));

            if (question == null) return NotFound();

            question.CurrentState = 0;
            question.UpdatedBy = CurrentUserId;
            question.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Question removed.";
            return RedirectToAction(nameof(Details), new { id = examId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var exam = await _context.Tests
                .FirstOrDefaultAsync(t => t.Id == id && courseIds.Contains(t.CourseId));

            if (exam == null) return NotFound();

            exam.CurrentState = 0;
            exam.UpdatedBy = CurrentUserId;
            exam.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Exam deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Student results for an exam
        public async Task<IActionResult> Results(int id)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var exam = await _context.Tests
                .FirstOrDefaultAsync(t => t.Id == id && courseIds.Contains(t.CourseId));

            if (exam == null) return NotFound();

            var results = await _context.StudentTests
                .Where(st => st.TestId == id)
                .Include(st => st.Student)
                .OrderByDescending(st => st.Score)
                .Select(st => new ExamResultRowVM
                {
                    StudentTestId = st.Id,
                    StudentName = st.Student != null ? st.Student.FullName : "",
                    Score = st.Score,
                    IsPending = st.IsPending,
                    JoinDate = st.JoinDate,
                    TimeInMinutes = st.TimeInMinutes
                })
                .ToListAsync();

            ViewBag.Exam = exam;
            ViewData["Title"] = $"Results: {exam.Title}";
            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> GradeTextAnswers(int studentTestId)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var studentTest = await _context.StudentTests
                .Include(st => st.Student)
                .Include(st => st.Test)
                .Include(st => st.Answers)
                    .ThenInclude(a => a.Question)
                .FirstOrDefaultAsync(st => st.Id == studentTestId && courseIds.Contains(st.Test.CourseId));

            if (studentTest == null) return NotFound();

            var vm = new GradeTextAnswersVM
            {
                StudentTestId = studentTestId,
                StudentName = studentTest.Student?.FullName ?? "",
                ExamTitle = studentTest.Test?.Title ?? "",
                ExamId = studentTest.TestId,
                TotalMarks = studentTest.Test?.TotalMarks ?? 0,
                AutoScore = studentTest.Answers.Where(a => a.Marked).Sum(a => a.Score),
                TextAnswers = studentTest.Answers
                    .Where(a => a.Question?.QuestionType == QuestionType.Text)
                    .Select(a => new TextAnswerGradeItemVM
                    {
                        AnswerId = a.Id,
                        QuestionTitle = a.Question?.Title ?? "",
                        QuestionDescription = a.Question?.Description,
                        MaxPoints = a.Question?.Pints ?? 0,
                        StudentAnswer = a.TextAnswer,
                        AwardedPoints = a.Marked ? a.Score : null
                    }).ToList()
            };

            ViewData["Title"] = $"Grade: {vm.StudentName}";
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeTextAnswers(GradeTextAnswersVM vm)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var studentTest = await _context.StudentTests
                .Include(st => st.Answers)
                    .ThenInclude(a => a.Question)
                .Include(st => st.Test)
                .FirstOrDefaultAsync(st => st.Id == vm.StudentTestId && courseIds.Contains(st.Test.CourseId));

            if (studentTest == null) return NotFound();

            foreach (var item in vm.TextAnswers)
            {
                var answer = studentTest.Answers.FirstOrDefault(a => a.Id == item.AnswerId);
                if (answer == null) continue;
                var max = answer.Question?.Pints ?? 0;
                answer.Score = Math.Round(Math.Min(item.AwardedPoints ?? 0, max), 2);
                answer.Marked = true;
            }

            studentTest.Score = Math.Round(studentTest.Answers.Sum(a => a.Score), 2);
            studentTest.IsPending = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Graded successfully. Final score: {studentTest.Score} / {studentTest.Test?.TotalMarks}";
            return RedirectToAction(nameof(Results), new { id = studentTest.TestId });
        }
    }
}