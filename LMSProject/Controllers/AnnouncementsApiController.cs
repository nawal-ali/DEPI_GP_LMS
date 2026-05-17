using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSEF;

namespace LMSProject.Controllers
{
    [Route("api/announcements")]
    [ApiController]
    public class AnnouncementsApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AnnouncementsApiController(AppDbContext db) => _db = db;

        /// <summary>
        /// Returns count of active announcements — used by the bell icon dot.
        /// Publicly accessible (no auth required) so the JS fetch works from any dashboard.
        /// </summary>
        [HttpGet("count")]
        public async Task<IActionResult> Count()
        {
            var count = await _db.Announcements
                .CountAsync(a => a.CurrentState == 1
                              && a.IsActive
                              && (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now));
            return Ok(count);
        }
    }
}