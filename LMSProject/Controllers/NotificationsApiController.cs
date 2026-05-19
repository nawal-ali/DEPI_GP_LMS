using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;

namespace LMSProject.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public NotificationsApiController(AppDbContext db, UserManager<ApplicationUser> um)
        { _db = db; _um = um; }

        // ── Universal bell data — adapts by role ───────────────────────────
        [HttpGet("bell")]
        public async Task<IActionResult> Bell()
        {
            var user = await _um.GetUserAsync(User);
            if (user == null) return Ok(new { total = 0, items = Array.Empty<object>() });

            var roles = await _um.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";
            var now = DateTime.Now;
            var items = new List<object>();

            // ── Announcements (all roles) ──────────────────────────────────
            var annCount = await _db.Announcements
                .CountAsync(a => a.CurrentState == 1 && a.IsActive
                             && (a.ExpiryDate == null || a.ExpiryDate > now)
                             && (a.TargetAudience == "All"
                                 || a.TargetAudience == role
                                 || (role == "Student" && a.TargetAudience == "Students")));
            if (annCount > 0)
                items.Add(new
                {
                    icon = "fa-bullhorn",
                    color = "#00CBB8",
                    bg = "rgba(0,203,184,.1)",
                    text = $"{annCount} new announcement{(annCount > 1 ? "s" : "")}",
                    link = GetAnnouncementLink(role),
                    count = annCount
                });

            // ── Ticket replies (all roles) ─────────────────────────────────
            var ticketCount = await _db.Tickets
                .CountAsync(t => t.SenderUserId == user.Id
                              && t.Status == "In Progress"
                              && t.CurrentState == 1);
            if (ticketCount > 0)
                items.Add(new
                {
                    icon = "fa-headset",
                    color = "#5B72EE",
                    bg = "rgba(91,114,238,.1)",
                    text = $"{ticketCount} ticket{(ticketCount > 1 ? "s" : "")} have replies",
                    link = GetTicketLink(role),
                    count = ticketCount
                });

            // ── Role-specific ──────────────────────────────────────────────
            if (role == "Student")
            {
                var student = await _db.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.Id && s.CurrentState == 1);
                if (student != null)
                {
                    var courseIds = await _db.StudentCourses
                        .Where(sc => sc.StId == student.Id)
                        .Select(sc => sc.CourseId).ToListAsync();

                    var pendingExams = await _db.Tests
                        .CountAsync(t => courseIds.Contains(t.CourseId) && t.CurrentState == 1
                                      && (t.Deadline == null || t.Deadline > now)
                                      && !_db.StudentTests.Any(st => st.StudentId == student.Id && st.TestId == t.Id));
                    if (pendingExams > 0)
                        items.Add(new
                        {
                            icon = "fa-file-alt",
                            color = "#F48C06",
                            bg = "rgba(244,140,6,.1)",
                            text = $"{pendingExams} exam{(pendingExams > 1 ? "s" : "")} available",
                            link = "/Student/Exams/Index",
                            count = pendingExams
                        });

                    var pendingAssign = await _db.Assignments
                        .CountAsync(a => courseIds.Contains(a.CourseId) && a.CurrentState == 1
                                      && a.Deadline > now
                                      && !_db.AssignmentSubmissions.Any(s => s.StudentId == student.Id && s.AssignmentId == a.Id));
                    if (pendingAssign > 0)
                        items.Add(new
                        {
                            icon = "fa-tasks",
                            color = "#E13468",
                            bg = "rgba(225,52,104,.1)",
                            text = $"{pendingAssign} assignment{(pendingAssign > 1 ? "s" : "")} due",
                            link = "/Student/Assignments/Index",
                            count = pendingAssign
                        });
                }
            }

            if (role == "Admin" || role == "SuperAdmin")
            {
                var openTickets = await _db.Tickets
                    .CountAsync(t => t.Status == "Open"
                                  && !t.IsForwarded
                                  && t.CurrentState == 1);
                if (openTickets > 0)
                    items.Add(new
                    {
                        icon = "fa-inbox",
                        color = "#E13468",
                        bg = "rgba(225,52,104,.1)",
                        text = $"{openTickets} open ticket{(openTickets > 1 ? "s" : "")}",
                        link = "/Admin/AdminTickets/Index",
                        count = openTickets
                    });
            }

            if (role == "SuperAdmin")
            {
                var forwarded = await _db.Tickets
                    .CountAsync(t => t.IsForwarded && t.Status == "Forwarded" && t.CurrentState == 1);
                if (forwarded > 0)
                    items.Add(new
                    {
                        icon = "fa-forward",
                        color = "#5B72EE",
                        bg = "rgba(91,114,238,.1)",
                        text = $"{forwarded} escalated ticket{(forwarded > 1 ? "s" : "")}",
                        link = "/SuperAdmin/Support/Index",
                        count = forwarded
                    });
                var newContacts = await _db.ContactForms.CountAsync(f => !f.IsRead);
                if (newContacts > 0)
                    items.Add(new
                    {
                        icon = "fa-envelope-open-text",
                        color = "#E13468",
                        bg = "rgba(225,52,104,.1)",
                        text = $"{newContacts} new contact form{(newContacts > 1 ? "s" : "")}",
                        link = "/SuperAdmin/ContactForms/Index",
                        count = newContacts
                    });
            }

            if (role == "Instructor")
            {
                var instructor = await _db.Instructors
                    .FirstOrDefaultAsync(i => i.UserId == user.Id && i.CurrentState == 1);
                if (instructor != null)
                {
                    var courseIds = await _db.Courses
                        .Where(c => c.InstructorId == instructor.Id && c.CurrentState == 1)
                        .Select(c => c.Id).ToListAsync();
                    var newSubmissions = await _db.AssignmentSubmissions
                        .CountAsync(s => _db.Assignments.Any(a => courseIds.Contains(a.CourseId) && a.Id == s.AssignmentId)
                                      && s.CurrentState == 1 && s.Marks == null);
                    if (newSubmissions > 0)
                        items.Add(new
                        {
                            icon = "fa-tasks",
                            color = "#00CBB8",
                            bg = "rgba(0,203,184,.1)",
                            text = $"{newSubmissions} submission{(newSubmissions > 1 ? "s" : "")} to grade",
                            link = "/Instructor/Assignments/Index",
                            count = newSubmissions
                        });
                }
            }

            if (role == "Parent")
            {
                var parent = await _db.Parents
                    .Include(p => p.Children)
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (parent?.Children?.Any() == true)
                {
                    var studentIds = parent.Children.Where(c => c.CurrentState == 1).Select(c => c.Id).ToList();
                    var recentExams = await _db.StudentTests
                        .CountAsync(st => studentIds.Contains(st.StudentId)
                                       && st.JoinDate >= DateTime.Now.AddDays(-7));
                    if (recentExams > 0)
                        items.Add(new
                        {
                            icon = "fa-chart-bar",
                            color = "#5B72EE",
                            bg = "rgba(91,114,238,.1)",
                            text = $"{recentExams} new exam result{(recentExams > 1 ? "s" : "")} this week",
                            link = "/",
                            count = recentExams
                        });
                }
            }

            var total = items.Sum(i => (int)((dynamic)i).count);
            return Ok(new { total, items });
        }

        // ── Sidebar badge counts ───────────────────────────────────────────
        [HttpGet("badges")]
        public async Task<IActionResult> Badges()
        {
            var user = await _um.GetUserAsync(User);
            if (user == null) return Ok(new { });

            var roles = await _um.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";
            var now = DateTime.Now;

            var annCount = await _db.Announcements
                .CountAsync(a => a.CurrentState == 1 && a.IsActive
                             && (a.ExpiryDate == null || a.ExpiryDate > now));

            var ticketCount = await _db.Tickets
                .CountAsync(t => t.SenderUserId == user.Id
                              && t.Status == "In Progress" && t.CurrentState == 1);

            var adminTickets = (role is "Admin" or "SuperAdmin")
                ? await _db.Tickets.CountAsync(t => t.Status == "Open" && !t.IsForwarded && t.CurrentState == 1)
                : 0;

            var contactForms = (role == "SuperAdmin")
                ? await _db.ContactForms.CountAsync(f => !f.IsRead)
                : 0;
            return Ok(new
            {
                announcements = annCount,
                tickets = ticketCount,
                adminTickets = adminTickets,
                contactForms = contactForms
            });
        }

        private static string GetAnnouncementLink(string role) => role switch
        {
            "Student" => "/Student/Announcements/Index",
            "Instructor" => "/Instructor/Announcements/Index",
            "Parent" => "/Parent/Announcements/Index",
            "Admin" => "/Admin/AdminAnnouncements/Index",
            _ => "/"
        };

        private static string GetTicketLink(string role) => role switch
        {
            "Student" => "/Student/Tickets/Index",
            "Instructor" => "/Instructor/Tickets/Index",
            "Parent" => "/Parent/Tickets/Index",
            _ => "/"
        };
    }

    // ── Also keep the old announcements count endpoint ─────────────────────
    [Route("api/announcements")]
    [ApiController]
    public class AnnouncementsApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AnnouncementsApiController(AppDbContext db) => _db = db;

        [HttpGet("count")]
        public async Task<IActionResult> Count()
        {
            var count = await _db.Announcements
                .CountAsync(a => a.CurrentState == 1 && a.IsActive
                              && (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now));
            return Ok(count);
        }
    }
}