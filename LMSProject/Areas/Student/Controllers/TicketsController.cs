using LMSProject.Areas.Student.ViewModels;
using LMSProject.Controllers;
using LMSProject.Services;
using LMSProject.ViewModels.Tickets;
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
    public class TicketsController : BaseController
    {
        private readonly TicketService _ts;
        private readonly IWebHostEnvironment _env;

        public TicketsController(TicketService ts, IWebHostEnvironment env,
            UserManager<ApplicationUser> um) : base(um)
        { _ts = ts; _env = env; }

        public async Task<IActionResult> Index(string status = "")
        {
            ViewData["Title"] = "Support Tickets";
            var tickets = await _ts.GetSenderTicketsAsync(CurrentUserId);
            if (!string.IsNullOrEmpty(status)) tickets = tickets.Where(t => t.Status == status).ToList();
            ViewBag.Status = status;
            return View("~/Areas/Student/Views/Tickets/Index.cshtml", tickets);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "New Ticket";
            return View("~/Areas/Student/Views/Tickets/Create.cshtml", new CreateTicketVM());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketVM vm)
        {
            if (!ModelState.IsValid) return View("~/Areas/Student/Views/Tickets/Create.cshtml", vm);
            var user = await _userManager.GetUserAsync(User);
            await _ts.CreateAsync(vm, CurrentUserId, user?.FullName ?? "Student", "Student", _env);
            TempData["Success"] = "Ticket submitted. An admin will respond shortly.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Detail(int id)
        {
            ViewData["Title"] = "Ticket Detail";
            var vm = await _ts.GetDetailAsync(id, CurrentUserId, "Student");
            if (vm.Ticket is null) return NotFound();
            return View("~/Areas/Student/Views/Tickets/Detail.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(ReplyTicketVM vm)
        {
            if (!ModelState.IsValid) { TempData["Error"] = "Reply cannot be empty."; return RedirectToAction("Detail", new { id = vm.TicketId }); }
            var user = await _userManager.GetUserAsync(User);
            await _ts.AddReplyAsync(vm, CurrentUserId, user?.FullName ?? "Student", "Student", _env);
            return RedirectToAction("Detail", new { id = vm.TicketId });
        }
    }
}