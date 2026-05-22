using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class SchedulesController : Controller
    {
        private readonly AppDbContext _db;
        public SchedulesController(AppDbContext db) { _db = db; }

        // ── All-grades weekly overview ────────────────────────────────────
        public async Task<IActionResult> Index(int? gradeId, int? termId, int? instructorId)
        {
            await LoadDropdowns(gradeId, termId, instructorId);

            var query = _db.ScheduleSessions
                .Include(s => s.Course).ThenInclude(c => c.Grade).ThenInclude(g => g.Stage)
                .Include(s => s.Course).ThenInclude(c => c.Term)
                .Include(s => s.Course).ThenInclude(c => c.SubSubject).ThenInclude(ss => ss.Subject)
                .Include(s => s.Course).ThenInclude(c => c.Instructor)
                .Where(s => s.CurrentState == 1 && s.Course.CurrentState == 1);

            if (gradeId.HasValue) query = query.Where(s => s.Course.GradeId == gradeId);
            if (termId.HasValue) query = query.Where(s => s.Course.TermId == termId);
            if (instructorId.HasValue) query = query.Where(s => s.Course.InstructorId == instructorId);

            return View("~/Areas/SuperAdmin/Views/Schedules/Index.cshtml", await query.ToListAsync());
        }

        // ── Grade-specific weekly schedule ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GradeSchedule(int gradeId, int? termId)
        {
            var grade = await _db.Grades
                .Include(g => g.Stage)
                .FirstOrDefaultAsync(g => g.Id == gradeId && g.CurrentState == 1);

            if (grade == null) return NotFound();

            var query = _db.ScheduleSessions
                .Include(s => s.Course).ThenInclude(c => c.Grade).ThenInclude(g => g.Stage)
                .Include(s => s.Course).ThenInclude(c => c.Term)
                .Include(s => s.Course).ThenInclude(c => c.SubSubject).ThenInclude(ss => ss.Subject)
                .Include(s => s.Course).ThenInclude(c => c.Instructor)
                .Where(s => s.CurrentState == 1 && s.Course.CurrentState == 1
                         && s.Course.GradeId == gradeId);

            if (termId.HasValue) query = query.Where(s => s.Course.TermId == termId);

            var sessions = await query.ToListAsync();

            // Live courses for this grade (for Add Session modal)
            var liveCourses = await _db.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Term)
                .Where(c => c.CurrentState == 1 && c.IsLive && c.GradeId == gradeId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var terms = await _db.Terms
                .Where(t => t.CurrentState == 1).OrderBy(t => t.Name).ToListAsync();

            ViewBag.Grade = grade;
            ViewBag.GradeId = gradeId;
            ViewBag.SelTerm = termId;
            ViewBag.LiveCourses = liveCourses;
            ViewBag.Terms = terms;

            return View("~/Areas/SuperAdmin/Views/Schedules/GradeSchedule.cshtml", sessions);
        }

        // ── Manage a single course's sessions ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Manage(int courseId)
        {
            var course = await _db.Courses
                .Include(c => c.Grade).ThenInclude(g => g.Stage)
                .Include(c => c.Term)
                .Include(c => c.SubSubject).ThenInclude(ss => ss.Subject)
                .Include(c => c.Instructor)
                .Include(c => c.ScheduleSessions!.Where(s => s.CurrentState == 1))
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound();
            return View("~/Areas/SuperAdmin/Views/Schedules/Manage.cshtml", course);
        }

        // ── Grade-scoped: Add session ─────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeAddSession(
            int gradeId, int courseId, int dayOfWeek,
            string startTime, string endTime,
            string? meetingLink, string? color, string? notes)
        {
            if (!TimeSpan.TryParse(startTime, out var start) ||
                !TimeSpan.TryParse(endTime, out var end) || end <= start)
            {
                TempData["Error"] = "Invalid time range.";
                return RedirectToAction("GradeSchedule", new { gradeId });
            }

            // Check overlap for same grade at same time (any course)
            var gradeConflict = await _db.ScheduleSessions
                .AnyAsync(s => s.Course.GradeId == gradeId
                            && s.DayOfWeek == dayOfWeek && s.CurrentState == 1
                            && start < s.EndTime && end > s.StartTime);

            if (gradeConflict)
            {
                TempData["Error"] = "Another session for this grade overlaps that time slot.";
                return RedirectToAction("GradeSchedule", new { gradeId });
            }

            // Check instructor double-booking
            var course = await _db.Courses.FindAsync(courseId);
            if (course != null)
            {
                var instructorConflict = await _db.ScheduleSessions
                    .AnyAsync(s => s.Course.InstructorId == course.InstructorId
                                && s.DayOfWeek == dayOfWeek && s.CurrentState == 1
                                && start < s.EndTime && end > s.StartTime);

                if (instructorConflict)
                {
                    TempData["Error"] = "The instructor already has a session at that time.";
                    return RedirectToAction("GradeSchedule", new { gradeId });
                }
            }

            _db.ScheduleSessions.Add(new TbScheduleSession
            {
                CourseId = courseId,
                DayOfWeek = dayOfWeek,
                StartTime = start,
                EndTime = end,
                MeetingLink = meetingLink,
                Color = string.IsNullOrEmpty(color) ? "#5B72EE" : color,
                Notes = notes,
                CurrentState = 1
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Session added successfully.";
            return RedirectToAction("GradeSchedule", new { gradeId });
        }

        // ── Grade-scoped: Edit session ────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeEditSession(
            int id, int gradeId, int dayOfWeek,
            string startTime, string endTime,
            string? meetingLink, string? color, string? notes)
        {
            var s = await _db.ScheduleSessions
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null) return NotFound();

            if (!TimeSpan.TryParse(startTime, out var start) ||
                !TimeSpan.TryParse(endTime, out var end) || end <= start)
            {
                TempData["Error"] = "Invalid time range.";
                return RedirectToAction("GradeSchedule", new { gradeId });
            }

            // Grade overlap (exclude self)
            var gradeConflict = await _db.ScheduleSessions
                .AnyAsync(x => x.Id != id
                            && x.Course.GradeId == gradeId
                            && x.DayOfWeek == dayOfWeek && x.CurrentState == 1
                            && start < x.EndTime && end > x.StartTime);

            if (gradeConflict)
            {
                TempData["Error"] = "Another session for this grade overlaps that time slot.";
                return RedirectToAction("GradeSchedule", new { gradeId });
            }

            s.DayOfWeek = dayOfWeek;
            s.StartTime = start;
            s.EndTime = end;
            s.MeetingLink = meetingLink;
            s.Color = string.IsNullOrEmpty(color) ? s.Color : color;
            s.Notes = notes;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Session updated.";
            return RedirectToAction("GradeSchedule", new { gradeId });
        }

        // ── Grade-scoped: Delete session ──────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeDeleteSession(int id, int gradeId)
        {
            var s = await _db.ScheduleSessions.FindAsync(id);
            if (s != null) { s.CurrentState = 0; await _db.SaveChangesAsync(); }
            TempData["Success"] = "Session removed.";
            return RedirectToAction("GradeSchedule", new { gradeId });
        }

        // ── Course-scoped: Add/Edit/Delete (Manage page) ──────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSession(
            int courseId, int dayOfWeek,
            string startTime, string endTime,
            string? meetingLink, string? color, string? notes)
        {
            var conflict = await _db.ScheduleSessions
                .AnyAsync(s => s.CourseId == courseId && s.DayOfWeek == dayOfWeek
                            && s.CurrentState == 1
                            && TimeSpan.Parse(startTime) < s.EndTime
                            && TimeSpan.Parse(endTime) > s.StartTime);

            if (conflict) { TempData["Error"] = "Session time conflicts with an existing slot."; }
            else
            {
                _db.ScheduleSessions.Add(new TbScheduleSession
                {
                    CourseId = courseId,
                    DayOfWeek = dayOfWeek,
                    StartTime = TimeSpan.Parse(startTime),
                    EndTime = TimeSpan.Parse(endTime),
                    MeetingLink = meetingLink,
                    Notes = notes,
                    Color = string.IsNullOrEmpty(color) ? "#5B72EE" : color,
                    CurrentState = 1
                });
                await _db.SaveChangesAsync();
                TempData["Success"] = "Session added.";
            }
            return RedirectToAction("Manage", new { courseId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSession(
            int id, int dayOfWeek,
            string startTime, string endTime,
            string? meetingLink, string? color, string? notes)
        {
            var s = await _db.ScheduleSessions.FindAsync(id);
            if (s == null) return NotFound();
            s.DayOfWeek = dayOfWeek; s.StartTime = TimeSpan.Parse(startTime);
            s.EndTime = TimeSpan.Parse(endTime); s.MeetingLink = meetingLink;
            s.Color = string.IsNullOrEmpty(color) ? s.Color : color; s.Notes = notes;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Session updated.";
            return RedirectToAction("Manage", new { courseId = s.CourseId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var s = await _db.ScheduleSessions.FindAsync(id);
            if (s != null) { s.CurrentState = 0; await _db.SaveChangesAsync(); }
            TempData["Success"] = "Session removed.";
            return RedirectToAction("Manage", new { courseId = s?.CourseId ?? 0 });
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private async Task LoadDropdowns(int? gradeId, int? termId, int? instructorId)
        {
            ViewBag.Grades = await _db.Grades.Where(g => g.CurrentState == 1)
                                .Include(g => g.Stage).OrderBy(g => g.Name).ToListAsync();
            ViewBag.Terms = await _db.Terms.Where(t => t.CurrentState == 1)
                                .OrderBy(t => t.Name).ToListAsync();
            ViewBag.Instructors = await _db.Instructors.Where(i => i.CurrentState == 1)
                                .OrderBy(i => i.FullName).ToListAsync();
            ViewBag.Courses = await _db.Courses.Where(c => c.CurrentState == 1 && c.IsLive)
                                .Include(c => c.Grade).Include(c => c.Term)
                                .OrderBy(c => c.Name).ToListAsync();
            ViewBag.SelGrade = gradeId; ViewBag.SelTerm = termId; ViewBag.SelInstructor = instructorId;
        }
    }
}