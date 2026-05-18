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
    public class AssignmentsController : BaseController
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public AssignmentsController(AppDbContext db, UserManager<ApplicationUser> um, IWebHostEnvironment env) : base(um)
        { _db = db; _env = env; }

        private async Task<int> GetStudentId()
        {
            var user = await _userManager.GetUserAsync(User);
            var s = await _db.Students.FirstOrDefaultAsync<TbStudent>(x => x.UserId == user!.Id && x.CurrentState == 1);
            return s?.Id ?? 0;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Assignments";
            var sid = await GetStudentId();
            var courseIds = await _db.StudentCourses.Where(sc => sc.StId == sid).Select(sc => sc.CourseId).ToListAsync();
            var assignments = await _db.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1)
                .Include(a => a.Course)
                .Include(a => a.Submissions.Where(s => s.StudentId == sid && s.CurrentState == 1))
                .OrderByDescending(a => a.Deadline).ToListAsync();

            var vm = assignments.Select(a =>
            {
                var sub = a.Submissions.FirstOrDefault();
                return new AssignmentVM
                {
                    AssignmentId = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    CourseName = a.Course?.Name ?? "",
                    CourseId = a.CourseId,
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
            }).ToList();

            return View("~/Areas/Student/Views/Assignments/Index.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitAssignmentVM vm)
        {
            var sid = await GetStudentId();
            var assignment = await _db.Assignments.FindAsync(vm.AssignmentId);
            if (assignment is null) return NotFound();

            var existing = await _db.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == vm.AssignmentId && s.StudentId == sid && s.CurrentState == 1);
            if (existing != null) { TempData["Error"] = "Already submitted."; return RedirectToAction("Index"); }

            string? fileUrl = null, fileName = null;
            if (vm.File != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "Uploads", "Assignments");
                Directory.CreateDirectory(folder);
                var unique = $"{Guid.NewGuid()}{Path.GetExtension(vm.File.FileName)}";
                using var stream = System.IO.File.Create(Path.Combine(folder, unique));
                await vm.File.CopyToAsync(stream);
                fileUrl = $"Uploads/Assignments/{unique}";
                fileName = vm.File.FileName;
            }

            _db.AssignmentSubmissions.Add(new TbAssignmentSubmission
            {
                AssignmentId = vm.AssignmentId,
                StudentId = sid,
                TextAnswer = vm.TextAnswer,
                FileUrl = fileUrl,
                FileName = fileName,
                SubmittedAt = DateTime.Now,
                IsLate = DateTime.Now > assignment.Deadline,
                CurrentState = 1,
                CreatedDate = DateTime.Now
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Assignment submitted successfully!";
            return RedirectToAction("Index");
        }
    }
}