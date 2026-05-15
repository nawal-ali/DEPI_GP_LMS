using LMSProject.Areas.Admin.Helpers;
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
    public class AssignmentsController : BaseController
    {
        private readonly AppDbContext _context;

        public AssignmentsController(AppDbContext context, UserManager<ApplicationUser> userManager)
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

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var courseNames = await _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            var assignments = await _context.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1)
                .OrderByDescending(a => a.Deadline)
                .ToListAsync();

            var submissionCounts = await _context.AssignmentSubmissions
                .Where(s => assignments.Select(a => a.Id).Contains(s.AssignmentId) && s.CurrentState == 1)
                .GroupBy(s => s.AssignmentId)
                .Select(g => new { AssignmentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AssignmentId, x => x.Count);

            var vm = assignments.Select(a => new AssignmentListItemVM
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                TotalMarks = a.TotalMarks,
                Deadline = a.Deadline,
                SubmissionType = a.SubmissionType,
                CourseName = courseNames.GetValueOrDefault(a.CourseId, ""),
                CourseId = a.CourseId,
                SubmissionCount = submissionCounts.GetValueOrDefault(a.Id, 0)
            }).ToList();

            ViewData["Title"] = "Assignments";
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var vm = new CreateAssignmentVM
            {
                Courses = await GetMyCoursesDropAsync(instructorId.Value),
                Deadline = DateTime.Now.AddDays(7),
                TotalMarks = 100
            };
            ViewData["Title"] = "Create Assignment";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAssignmentVM vm)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            if (!courseIds.Contains(vm.CourseId))
                ModelState.AddModelError("CourseId", "Invalid course.");

            if (!ModelState.IsValid)
            {
                vm.Courses = await GetMyCoursesDropAsync(instructorId.Value);
                ViewData["Title"] = "Create Assignment";
                return View(vm);
            }

            var assignment = new TbAssignment
            {
                Title = vm.Title,
                Description = vm.Description,
                TotalMarks = vm.TotalMarks,
                Deadline = vm.Deadline,
                SubmissionType = vm.SubmissionType,
                CourseId = vm.CourseId,
                CreatedBy = CurrentUserId,
                CurrentState = 1
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Assignment created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.Id == id && courseIds.Contains(a.CourseId));

            if (assignment == null) return NotFound();

            var vm = new EditAssignmentVM
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                TotalMarks = assignment.TotalMarks,
                Deadline = assignment.Deadline,
                SubmissionType = assignment.SubmissionType,
                CourseId = assignment.CourseId,
                Courses = await GetMyCoursesDropAsync(instructorId.Value)
            };
            ViewData["Title"] = "Edit Assignment";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAssignmentVM vm)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.Id == vm.Id && courseIds.Contains(a.CourseId));

            if (assignment == null) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.Courses = await GetMyCoursesDropAsync(instructorId.Value);
                ViewData["Title"] = "Edit Assignment";
                return View(vm);
            }

            assignment.Title = vm.Title;
            assignment.Description = vm.Description;
            assignment.TotalMarks = vm.TotalMarks;
            assignment.Deadline = vm.Deadline;
            assignment.SubmissionType = vm.SubmissionType;
            assignment.CourseId = vm.CourseId;
            assignment.UpdatedBy = CurrentUserId;
            assignment.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Assignment updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Submissions(int id)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id && courseIds.Contains(a.CourseId));

            if (assignment == null) return NotFound();

            var submissions = await _context.AssignmentSubmissions
                .Where(s => s.AssignmentId == id && s.CurrentState == 1)
                .Include(s => s.Student)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            var enrolledCount = await _context.StudentCourses
                .CountAsync(sc => sc.CourseId == assignment.CourseId);

            var vm = new AssignmentSubmissionsVM
            {
                AssignmentId = assignment.Id,
                AssignmentTitle = assignment.Title,
                CourseName = assignment.Course.Name,
                TotalMarks = assignment.TotalMarks,
                Deadline = assignment.Deadline,
                SubmissionType = assignment.SubmissionType,
                TotalEnrolled = enrolledCount,
                Submissions = submissions.Select(s => new SubmissionRowVM
                {
                    Id = s.Id,
                    StudentName = s.Student?.FullName ?? "Unknown",
                    StudentId = s.StudentId,
                    TextAnswer = s.TextAnswer,
                    FileUrl = s.FileUrl,
                    FileName = s.FileName,
                    SubmittedAt = s.SubmittedAt,
                    IsLate = s.IsLate,
                    Marks = s.Marks,
                    InstructorFeedback = s.InstructorFeedback
                }).ToList()
            };

            ViewData["Title"] = $"Submissions: {assignment.Title}";
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Grade(int id)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id && courseIds.Contains(s.Assignment.CourseId));

            if (submission == null) return NotFound();

            var vm = new GradeSubmissionVM
            {
                SubmissionId = submission.Id,
                AssignmentId = submission.AssignmentId,
                StudentName = submission.Student?.FullName ?? "",
                AssignmentTitle = submission.Assignment?.Title ?? "",
                TotalMarks = submission.Assignment?.TotalMarks ?? 0,
                TextAnswer = submission.TextAnswer,
                FileUrl = submission.FileUrl,
                FileName = submission.FileName,
                SubmittedAt = submission.SubmittedAt,
                IsLate = submission.IsLate,
                NewMarks = submission.Marks,
                NewFeedback = submission.InstructorFeedback
            };

            ViewData["Title"] = "Grade Submission";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(GradeSubmissionVM vm)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == vm.SubmissionId && courseIds.Contains(s.Assignment.CourseId));

            if (submission == null) return NotFound();

            submission.Marks = vm.NewMarks;
            submission.InstructorFeedback = vm.NewFeedback;
            submission.UpdatedBy = CurrentUserId;
            submission.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Grade submitted successfully.";
            return RedirectToAction(nameof(Submissions), new { id = vm.AssignmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.Id == id && courseIds.Contains(a.CourseId));

            if (assignment == null) return NotFound();

            assignment.CurrentState = 0;
            assignment.UpdatedBy = CurrentUserId;
            assignment.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Assignment deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
