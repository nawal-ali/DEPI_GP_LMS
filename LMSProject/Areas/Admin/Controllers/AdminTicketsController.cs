using LMSProject.Controllers;
using LMSProject.Services;
using LMSProject.ViewModels.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MLSCore.IdentityModel;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminTicketsController : BaseController
    {
        private readonly TicketService _ts;
        private readonly IWebHostEnvironment _env;

        public AdminTicketsController(TicketService ts, IWebHostEnvironment env,
            UserManager<ApplicationUser> um) : base(um)
        { _ts = ts; _env = env; }

        public async Task<IActionResult> Index(string status = "")
        {
            ViewData["Title"] = "Support Tickets";
            ViewBag.Status = status;
            var tickets = await _ts.GetAdminTicketsAsync(status);
            ViewBag.OpenCount = tickets.Count(t => t.Status == "Open");
            ViewBag.InProgCount = tickets.Count(t => t.Status == "In Progress");
            ViewBag.ResolvedCount = tickets.Count(t => t.Status == "Resolved");
            return View("~/Areas/Admin/Views/AdminTickets/Index.cshtml", tickets);
        }

        public async Task<IActionResult> Detail(int id)
        {
            ViewData["Title"] = "Ticket Detail";
            var vm = await _ts.GetDetailAsync(id, CurrentUserId, "Admin");
            if (vm == null) return NotFound();
            return View("~/Areas/Admin/Views/AdminTickets/Detail.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(ReplyTicketVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Reply cannot be empty.";
                return RedirectToAction("Detail", new { id = vm.TicketId });
            }
            var user = await _userManager.GetUserAsync(User);
            await _ts.AddReplyAsync(vm, CurrentUserId,
                user?.FullName ?? "Admin", "Admin", _env);
            return RedirectToAction("Detail", new { id = vm.TicketId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(ChangeStatusVM vm)
        {
            await _ts.ChangeStatusAsync(vm.TicketId, vm.NewStatus, CurrentUserId);
            TempData["Success"] = $"Status changed to {vm.NewStatus}.";
            return RedirectToAction("Detail", new { id = vm.TicketId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Forward(ForwardTicketVM vm)
        {
            var user = await _userManager.GetUserAsync(User);
            await _ts.ForwardAsync(vm.TicketId, CurrentUserId,
                user?.FullName ?? "Admin", vm.ForwardNote);
            TempData["Success"] = "Ticket forwarded to SuperAdmin.";
            return RedirectToAction("Index");
        }
    }
}