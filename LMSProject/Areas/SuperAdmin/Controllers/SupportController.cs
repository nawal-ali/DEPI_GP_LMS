using LMSProject.Controllers;
using LMSProject.Services;
using LMSProject.ViewModels.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MLSCore.IdentityModel;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class SupportController : BaseController
    {
        private readonly TicketService _ts;
        private readonly IWebHostEnvironment _env;

        public SupportController(TicketService ts, IWebHostEnvironment env,
            UserManager<ApplicationUser> um) : base(um)
        { _ts = ts; _env = env; }

        public async Task<IActionResult> Index(string status = "")
        {
            ViewData["Title"] = "Forwarded Tickets";
            ViewBag.Status = status;
            var tickets = await _ts.GetForwardedTicketsAsync(status);
            return View("~/Areas/SuperAdmin/Views/Support/Index.cshtml", tickets);
        }

        public async Task<IActionResult> Detail(int id)
        {
            ViewData["Title"] = "Ticket Detail";
            var vm = await _ts.GetDetailAsync(id, CurrentUserId, "SuperAdmin");
            if (vm == null) return NotFound();
            return View("~/Areas/SuperAdmin/Views/Support/Detail.cshtml", vm);
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
                user?.FullName ?? "SuperAdmin", "SuperAdmin", _env);
            return RedirectToAction("Detail", new { id = vm.TicketId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(ChangeStatusVM vm)
        {
            await _ts.ChangeStatusAsync(vm.TicketId, vm.NewStatus);
            TempData["Success"] = $"Status changed to {vm.NewStatus}.";
            return RedirectToAction("Detail", new { id = vm.TicketId });
        }
    }
}