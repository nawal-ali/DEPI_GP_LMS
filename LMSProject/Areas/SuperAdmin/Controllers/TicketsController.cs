using LMSProject.Areas.SuperAdmin.Services;
using LMSProject.Areas.SuperAdmin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class TicketsController : Controller
    {
        private readonly SuperAdminDataService _data;
        public TicketsController(SuperAdminDataService data) => _data = data;

        public IActionResult Index(string search = "", string status = "",
                                   string priority = "", string category = "", int page = 1)
        {
            ViewData["Title"] = "Support Tickets";
            ViewData["Breadcrumb"] = new List<(string, string?)> { ("Support Tickets", null) };
            var vm = _data.GetTickets(search, status, priority, category, page);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Respond(TicketRespondVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Response))
            {
                TempData["Error"] = "Response text cannot be empty.";
                return RedirectToAction("Index");
            }
            _data.RespondToTicket(vm.TicketId, vm.Response, vm.NewStatus);
            TempData["Success"] = $"Ticket #{vm.TicketId} updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Close(int id)
        {
            _data.CloseTicket(id);
            TempData["Success"] = $"Ticket #{id} closed.";
            return RedirectToAction("Index");
        }
    }
}