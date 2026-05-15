using LMSProject.Areas.Instructor.ViewModel;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;

namespace LMSProject.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class StudentController : BaseController
    {
        private readonly AppDbContext _context;

        public StudentController(AppDbContext context, UserManager<ApplicationUser> userManager)
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

        public async Task<IActionResult> Index()
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await _context.Courses
                .Where(c => c.InstructorId == instructorId && c.CurrentState == 1)
                .Select(c => c.Id)
                .ToListAsync();

            var courseNames = await _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            var enrollments = await _context.StudentCourses
                .Where(sc => courseIds.Contains(sc.CourseId))
                .Include(sc => sc.Student)
                    .ThenInclude(s => s.Grade)
                .Include(sc => sc.Student)
                    .ThenInclude(s => s.User)
                .ToListAsync();

            var students = enrollments
                .GroupBy(sc => sc.StId)
                .Select(g =>
                {
                    var student = g.First().Student;
                    return new StudentListVM
                    {
                        Id = student.Id,
                        FullName = student.FullName,
                        GradeName = student.Grade?.Name,
                        Email = student.User?.Email,
                        EnrolledCourses = g
                            .Select(sc => courseNames.GetValueOrDefault(sc.CourseId, ""))
                            .Where(n => !string.IsNullOrEmpty(n))
                            .ToList()
                    };
                })
                .OrderBy(s => s.FullName)
                .ToList();

            ViewData["Title"] = "My Students";
            return View(students);
        }

        public async Task<IActionResult> Details(int id)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await _context.Courses
                .Where(c => c.InstructorId == instructorId && c.CurrentState == 1)
                .ToListAsync();

            var student = await _context.Students
                .Include(s => s.Grade)
                .Include(s => s.User)
                .Include(s => s.StudentCourses)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            var enrolledCourseIds = student.StudentCourses?.Select(sc => sc.CourseId).ToList() ?? new();
            var myEnrolledCourses = courseIds.Where(c => enrolledCourseIds.Contains(c.Id)).ToList();

            if (!myEnrolledCourses.Any()) return Forbid();

            var progress = new List<CourseProgressItem>();

            foreach (var course in myEnrolledCourses)
            {
                var courseExams = await _context.Tests
                    .Where(t => t.CourseId == course.Id && t.CurrentState == 1)
                    .CountAsync();

                var attempts = await _context.StudentTests
                    .Where(st => st.StudentId == id && _context.Tests
                        .Where(t => t.CourseId == course.Id).Select(t => t.Id).Contains(st.TestId))
                    .ToListAsync();

                var courseAssignments = await _context.Assignments
                    .Where(a => a.CourseId == course.Id && a.CurrentState == 1)
                    .CountAsync();

                var submissions = await _context.AssignmentSubmissions
                    .Where(s => s.StudentId == id && s.CurrentState == 1 &&
                                _context.Assignments
                                    .Where(a => a.CourseId == course.Id).Select(a => a.Id)
                                    .Contains(s.AssignmentId))
                    .ToListAsync();

                progress.Add(new CourseProgressItem
                {
                    CourseName = course.Name,
                    TotalExams = courseExams,
                    AttemptedExams = attempts.Count,
                    AvgExamScore = attempts.Any() ? attempts.Average(a => a.Score) : null,
                    TotalAssignments = courseAssignments,
                    SubmittedAssignments = submissions.Count,
                    LateSubmissions = submissions.Count(s => s.IsLate),
                    AvgAssignmentMarks = submissions.Where(s => s.Marks.HasValue).Any()
                        ? submissions.Where(s => s.Marks.HasValue).Average(s => s.Marks!.Value)
                        : null
                });
            }

            var vm = new StudentProgressVM
            {
                StudentId = student.Id,
                FullName = student.FullName,
                GradeName = student.Grade?.Name,
                Email = student.User?.Email,
                CourseProgress = progress
            };

            ViewData["Title"] = $"Student: {student.FullName}";
            return View(vm);
        }
    }
}
