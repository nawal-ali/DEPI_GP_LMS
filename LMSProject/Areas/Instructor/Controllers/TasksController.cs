using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;
using LMSProject.ViewModel;

namespace LMSProject.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Get current user ID
        private string GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }

        // GET: List all tasks created by the current instructor
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var tasks = await _context.Tasks
                    .Where(t => t.CreatedBy == currentUserId)
                    .Include(t => t.SubContent)
                        .ThenInclude(sc => sc.CourseContent)
                            .ThenInclude(cc => cc.Course)
                    .OrderByDescending(t => t.CreatedDate)
                    .ToListAsync();

                return View(tasks);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while loading tasks: {ex.Message}";
                return View(new List<TbTask>());
            }
        }

        // GET: Create Task form
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Get all sub-contents that belong to courses created by this instructor
                var subContents = await _context.SubContents
                    .Include(sc => sc.CourseContent)
                        .ThenInclude(cc => cc.Course)
                    .Where(sc => sc.CourseContent.Course.CreatedBy == currentUserId)
                    .OrderBy(sc => sc.Title)
                    .ToListAsync();

                ViewBag.SubContents = subContents;
                return View(new CreateTaskVM());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while loading the form: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Create Task
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var currentUserId = GetCurrentUserId();
                    var subContents = await _context.SubContents
                        .Include(sc => sc.CourseContent)
                            .ThenInclude(cc => cc.Course)
                        .Where(sc => sc.CourseContent.Course.CreatedBy == currentUserId)
                        .OrderBy(sc => sc.Title)
                        .ToListAsync();

                    ViewBag.SubContents = subContents;
                    return View(model);
                }

                // Verify that the selected SubContent belongs to this instructor's course
                var subContent = await _context.SubContents
                    .Include(sc => sc.CourseContent)
                        .ThenInclude(cc => cc.Course)
                    .FirstOrDefaultAsync(sc => sc.Id == model.SubContentId);

                if (subContent?.CourseContent?.Course?.CreatedBy != GetCurrentUserId())
                {
                    TempData["Error"] = "You do not have permission to create tasks for this sub-content.";
                    return RedirectToAction("Index");
                }

                var task = new TbTask
                {
                    Title = model.Title,
                    URL = model.URL,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    SubContentId = model.SubContentId,
                    CreatedBy = GetCurrentUserId(),
                    CreatedDate = DateTime.Now,
                    CurrentState = 1
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Task created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while creating the task: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Edit Task form
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var task = await _context.Tasks
                    .Include(t => t.SubContent)
                        .ThenInclude(sc => sc.CourseContent)
                            .ThenInclude(cc => cc.Course)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    TempData["Error"] = "Task not found.";
                    return RedirectToAction("Index");
                }

                // Check authorization
                if (task.CreatedBy != GetCurrentUserId())
                {
                    TempData["Error"] = "You do not have permission to edit this task.";
                    return RedirectToAction("Index");
                }

                var currentUserId = GetCurrentUserId();
                var subContents = await _context.SubContents
                    .Include(sc => sc.CourseContent)
                        .ThenInclude(cc => cc.Course)
                    .Where(sc => sc.CourseContent.Course.CreatedBy == currentUserId)
                    .OrderBy(sc => sc.Title)
                    .ToListAsync();

                ViewBag.SubContents = subContents;

                var model = new CreateTaskVM
                {
                    Title = task.Title,
                    URL = task.URL,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    SubContentId = task.SubContentId
                };

                return View("Create", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while loading the task: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Update Task
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, CreateTaskVM model)
        {
            try
            {
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    TempData["Error"] = "Task not found.";
                    return RedirectToAction("Index");
                }

                // Check authorization
                if (task.CreatedBy != GetCurrentUserId())
                {
                    TempData["Error"] = "You do not have permission to edit this task.";
                    return RedirectToAction("Index");
                }

                if (!ModelState.IsValid)
                {
                    var currentUserId = GetCurrentUserId();
                    var subContents = await _context.SubContents
                        .Include(sc => sc.CourseContent)
                            .ThenInclude(cc => cc.Course)
                        .Where(sc => sc.CourseContent.Course.CreatedBy == currentUserId)
                        .OrderBy(sc => sc.Title)
                        .ToListAsync();

                    ViewBag.SubContents = subContents;
                    return View("Create", model);
                }

                task.Title = model.Title;
                task.URL = model.URL;
                task.StartDate = model.StartDate;
                task.EndDate = model.EndDate;
                task.SubContentId = model.SubContentId;
                task.UpdatedBy = GetCurrentUserId();
                task.UpdatedDate = DateTime.Now;

                _context.Tasks.Update(task);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Task updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while updating the task: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Delete Task (confirmation)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var task = await _context.Tasks
                    .Include(t => t.SubContent)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    TempData["Error"] = "Task not found.";
                    return RedirectToAction("Index");
                }

                // Check authorization
                if (task.CreatedBy != GetCurrentUserId())
                {
                    TempData["Error"] = "You do not have permission to delete this task.";
                    return RedirectToAction("Index");
                }

                return View(task);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Confirm Delete Task
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    TempData["Error"] = "Task not found.";
                    return RedirectToAction("Index");
                }

                // Check authorization
                if (task.CreatedBy != GetCurrentUserId())
                {
                    TempData["Error"] = "You do not have permission to delete this task.";
                    return RedirectToAction("Index");
                }

                // Delete all related task answers first
                var taskAnswers = await _context.TaskAnswers
                    .Where(ta => ta.TaskId == id)
                    .ToListAsync();

                _context.TaskAnswers.RemoveRange(taskAnswers);
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Task deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while deleting the task: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: View all submissions for a specific task
        [HttpGet]
        public async Task<IActionResult> Submissions(int taskId)
        {
            try
            {
                var task = await _context.Tasks
                    .Include(t => t.SubContent)
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                {
                    TempData["Error"] = "Task not found.";
                    return RedirectToAction("Index");
                }

                // Check authorization
                if (task.CreatedBy != GetCurrentUserId())
                {
                    TempData["Error"] = "You do not have permission to view submissions for this task.";
                    return RedirectToAction("Index");
                }

                var submissions = await _context.TaskAnswers
                    .Where(ta => ta.TaskId == taskId)
                    .Include(ta => ta.Student)
                        .ThenInclude(s => s.User)
                    .OrderBy(ta => ta.Student.FullName)
                    .ToListAsync();

                ViewBag.Task = task;
                return View(submissions);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while loading submissions: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Review and grade a student's submission
        [HttpGet]
        public async Task<IActionResult> ReviewSubmission(int submissionId)
        {
            try
            {
                var submission = await _context.TaskAnswers
                    .Include(ta => ta.SubContentTask)
                    .Include(ta => ta.Student)
                        .ThenInclude(s => s.User)
                    .FirstOrDefaultAsync(ta => ta.Id == submissionId);

                if (submission == null)
                {
                    TempData["Error"] = "Submission not found.";
                    return RedirectToAction("Index");
                }

                // Check authorization
                if (submission.SubContentTask.CreatedBy != GetCurrentUserId())
                {
                    TempData["Error"] = "You do not have permission to review this submission.";
                    return RedirectToAction("Index");
                }

                var model = new ReviewSubmissionVM
                {
                    SubmissionId = submission.Id,
                    TaskId = submission.TaskId,
                    StudentName = submission.Student.FullName,
                    StudentId = submission.StudId,
                    AnswerURL = submission.AnswerURL,
                    CurrentScore = submission.Score,
                    CurrentComment = submission.Comment
                };

                ViewBag.Submission = submission;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while loading the submission: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Submit grades and comments for a submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitGrade(ReviewSubmissionVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var submission = await _context.TaskAnswers
                        .Include(ta => ta.SubContentTask)
                        .Include(ta => ta.Student)
                        .FirstOrDefaultAsync(ta => ta.Id == model.SubmissionId);

                    ViewBag.Submission = submission;
                    return View("ReviewSubmission", model);
                }

                var taskSubmission = await _context.TaskAnswers
                    .Include(ta => ta.SubContentTask)
                    .FirstOrDefaultAsync(ta => ta.Id == model.SubmissionId);

                if (taskSubmission == null)
                {
                    TempData["Error"] = "Submission not found.";
                    return RedirectToAction("Index");
                }

                // Check authorization
                if (taskSubmission.SubContentTask.CreatedBy != GetCurrentUserId())
                {
                    TempData["Error"] = "You do not have permission to grade this submission.";
                    return RedirectToAction("Index");
                }

                taskSubmission.Score = model.NewScore;
                taskSubmission.Comment = model.NewComment;
                taskSubmission.UpdatedBy = GetCurrentUserId();
                taskSubmission.UpdatedDate = DateTime.Now;

                _context.TaskAnswers.Update(taskSubmission);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Grade submitted successfully!";
                return RedirectToAction("Submissions", new { taskId = model.TaskId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while submitting the grade: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
