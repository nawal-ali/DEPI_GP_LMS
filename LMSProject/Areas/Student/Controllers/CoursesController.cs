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
    public class CoursesController : BaseController
    {
        private readonly AppDbContext _db;
        public CoursesController(AppDbContext db, UserManager<ApplicationUser> um) : base(um) => _db = db;

        private async Task<int> GetStudentId()
        {
            var user = await _userManager.GetUserAsync(User);
            var s = await _db.Students.FirstOrDefaultAsync(x => x.UserId == user!.Id && x.CurrentState == 1);
            return s?.Id ?? 0;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "My Courses";
            var sid = await GetStudentId();
            if (sid == 0) return NotFound();

            var enrollments = await _db.StudentCourses
                .Where(sc => sc.StId == sid)
                .Include(sc => sc.Course).ThenInclude(c => c.Instructor)
                .Include(sc => sc.Course).ThenInclude(c => c.Grade)
                .Include(sc => sc.Course).ThenInclude(c => c.Term)
                .Include(sc => sc.Course).ThenInclude(c => c.SubSubject).ThenInclude(ss => ss!.Subject)
                .ToListAsync();

            var courseIds = enrollments.Select(e => e.CourseId).ToList();
            var examTakens = await _db.StudentTests.Where(st => st.StudentId == sid).Select(st => st.TestId).ToListAsync();
            var submissions = await _db.AssignmentSubmissions.Where(s => s.StudentId == sid && s.CurrentState == 1).Select(s => s.AssignmentId).ToListAsync();
            var matCounts = await _db.CourseMaterials.Where(m => courseIds.Contains(m.CourseId) && m.CurrentState == 1)
                                .GroupBy(m => m.CourseId).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(g => g.Key, g => g.Count);
            var examCounts = await _db.Tests.Where(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1)
                                .GroupBy(t => t.CourseId).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(g => g.Key, g => g.Count);
            var assCounts = await _db.Assignments.Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1)
                                .GroupBy(a => a.CourseId).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(g => g.Key, g => g.Count);
            var nextDeadlines = await _db.Assignments.Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1 && a.Deadline > DateTime.Now)
                                .GroupBy(a => a.CourseId).Select(g => new { g.Key, Next = g.Min(a => a.Deadline) }).ToDictionaryAsync(g => g.Key, g => (DateTime?)g.Next);

            var vm = enrollments.Select(en => new CourseCardVM
            {
                CourseId = en.CourseId,
                CourseName = en.Course?.Name ?? "",
                ImageName = en.Course?.ImageName,
                InstructorName = en.Course?.Instructor?.FullName ?? "",
                InstructorImage = en.Course?.Instructor?.ImageName,
                GradeName = en.Course?.Grade?.Name ?? "",
                TermName = en.Course?.Term?.Name ?? "",
                Subject = en.Course?.SubSubject?.Subject?.Name ?? "",
                Status = en.Course?.status ?? CourseStatus.OnLine,
                MaterialCount = matCounts.GetValueOrDefault(en.CourseId),
                ExamCount = examCounts.GetValueOrDefault(en.CourseId),
                AssignmentCount = assCounts.GetValueOrDefault(en.CourseId),
                ExamsTaken = examTakens.Count(tid => _db.Tests.Any(t => t.Id == tid && t.CourseId == en.CourseId)),
                SubmittedCount = submissions.Count,
                NextDeadline = nextDeadlines.GetValueOrDefault(en.CourseId)
            }).ToList();

            return View("~/Areas/Student/Views/Courses/Index.cshtml", vm);
        }

        public async Task<IActionResult> Detail(int id)
        {
            ViewData["Title"] = "Course Detail";
            var sid = await GetStudentId();
            var enrolled = await _db.StudentCourses.AnyAsync(sc => sc.StId == sid && sc.CourseId == id);
            if (!enrolled) return Forbid();

            var course = await _db.Courses
                .Include(c => c.Instructor).Include(c => c.Grade)
                .Include(c => c.Term).Include(c => c.SubSubject).ThenInclude(ss => ss!.Subject)
                .FirstOrDefaultAsync(c => c.Id == id && c.CurrentState == 1);
            if (course is null) return NotFound();

            var materials = await _db.CourseMaterials.Where(m => m.CourseId == id && m.CurrentState == 1).OrderByDescending(m => m.CreatedDate).ToListAsync();
            var exams = await _db.Tests.Where(t => t.CourseId == id && t.CurrentState == 1).Include(t => t.Questions).ToListAsync();
            var assignments = await _db.Assignments.Where(a => a.CourseId == id && a.CurrentState == 1).Include(a => a.Submissions.Where(s => s.StudentId == sid && s.CurrentState == 1)).ToListAsync();
            var examResults = await _db.StudentTests.Where(st => st.StudentId == sid).ToDictionaryAsync(st => st.TestId);

            var vm = new CourseDetailVM
            {
                Course = course,
                Instructor = course.Instructor,
                Materials = materials,
                Exams = exams.Select(t => new ExamVM
                {
                    ExamId = t.Id,
                    Title = t.Title,
                    CourseId = id,
                    CourseName = course.Name,
                    DurationMinutes = t.DurationInMinutes,
                    TotalMarks = t.TotalMarks,
                    Deadline = t.Deadline,
                    QuestionCount = t.Questions?.Count(q => q.CurrentState == 1) ?? 0,
                    IsSubmitted = examResults.ContainsKey(t.Id),
                    Score = examResults.TryGetValue(t.Id, out var er) ? er.Score : null,
                    TakenAt = examResults.TryGetValue(t.Id, out var er2) ? er2.JoinDate : null
                }).ToList(),
                Assignments = assignments.Select(a =>
                {
                    var sub = a.Submissions.FirstOrDefault();
                    return new AssignmentVM
                    {
                        AssignmentId = a.Id,
                        Title = a.Title,
                        Description = a.Description,
                        CourseId = id,
                        CourseName = course.Name,
                        TotalMarks = a.TotalMarks,
                        Deadline = a.Deadline,
                        SubType = a.SubmissionType,
                        IsSubmitted = sub != null,
                        IsLate = sub?.IsLate ?? false,
                        IsGraded = sub?.Marks.HasValue ?? false,
                        Score = sub?.Marks,
                        Feedback = sub?.InstructorFeedback,
                        SubmittedAt = sub?.SubmittedAt,
                        FileUrl = sub?.FileUrl,
                        FileName = sub?.FileName,
                        TextAnswer = sub?.TextAnswer
                    };
                }).ToList()
            };

            return View("~/Areas/Student/Views/Courses/Detail.cshtml", vm);
        }
    }
}