using LMSProject.Areas.Admin.Helpers;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MLSCore;
using MLSCore.IdentityModel;
using MLSCore.Models;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class ConfigController : BaseController
    {
        private readonly IUnitOfWork _uow;

        public ConfigController(IUnitOfWork uow, UserManager<ApplicationUser> um) : base(um) => _uow = uow;

        // ─── helper: gradient palette ──────────────────────────────────────
        private static readonly string[] Grads =
        {
            "linear-gradient(135deg,#5B72EE,#29B9E7)",
            "linear-gradient(135deg,#00CBB8,#33EFA0)",
            "linear-gradient(135deg,#E13468,#F48C06)",
            "linear-gradient(135deg,#2F327D,#5B72EE)",
            "linear-gradient(135deg,#F48C06,#FDCB6E)",
            "linear-gradient(135deg,#29B9E7,#00CBB8)",
        };

        // ══════════════════════════════════════════════════════════════════
        // STAGES
        // ══════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Stages()
        {
            ViewData["Title"] = "Stages";
            ViewBag.Gradients = Grads;
            return View("~/Areas/SuperAdmin/Views/Config/Stages.cshtml",
                await _uow.Stages.FindAllAsync(s => s.CurrentState == 1));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStage(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                await _uow.Stages.AddAsync(new TbStage { Name = name, ImageName = "", CurrentState = 1, CreatedBy = CurrentUserId, CreatedDate = DateTime.Now });
                _uow.Complete();
                TempData["Success"] = $"Stage \"{name}\" created.";
            }
            return RedirectToAction("Stages");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStage(int id, string name)
        {
            var e = await _uow.Stages.GetById(id);
            if (e != null) { e.Name = name; e.UpdatedBy = CurrentUserId; e.UpdatedDate = DateTime.Now; _uow.Stages.Update(e); _uow.Complete(); TempData["Success"] = "Stage updated."; }
            return RedirectToAction("Stages");
        }

        public async Task<IActionResult> DeleteStage(int id)
        {
            var e = await _uow.Stages.GetById(id);
            if (e != null) { e.CurrentState = 0; _uow.Stages.Update(e); _uow.Complete(); TempData["Success"] = "Stage deleted."; }
            return RedirectToAction("Stages");
        }

        // ══════════════════════════════════════════════════════════════════
        // GRADES
        // ══════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Grades()
        {
            ViewData["Title"] = "Grades";
            ViewBag.Gradients = Grads;
            ViewBag.Stages = (await _uow.Stages.FindAllAsync(s => s.CurrentState == 1)).ToList();
            return View("~/Areas/SuperAdmin/Views/Config/Grades.cshtml",
                await _uow.Grades.FindAllAsync(g => g.CurrentState == 1));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGrade(string name, int stageId)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                await _uow.Grades.AddAsync(new TbGrade { Name = name, StageId = stageId, ImageName = "", CurrentState = 1, CreatedBy = CurrentUserId, CreatedDate = DateTime.Now });
                _uow.Complete();
                TempData["Success"] = $"Grade \"{name}\" created.";
            }
            return RedirectToAction("Grades");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGrade(int id, string name, int stageId)
        {
            var e = await _uow.Grades.GetById(id);
            if (e != null) { e.Name = name; e.StageId = stageId; e.UpdatedBy = CurrentUserId; e.UpdatedDate = DateTime.Now; _uow.Grades.Update(e); _uow.Complete(); TempData["Success"] = "Grade updated."; }
            return RedirectToAction("Grades");
        }

        public async Task<IActionResult> DeleteGrade(int id)
        {
            var e = await _uow.Grades.GetById(id);
            if (e != null) { e.CurrentState = 0; _uow.Grades.Update(e); _uow.Complete(); TempData["Success"] = "Grade deleted."; }
            return RedirectToAction("Grades");
        }

        // ══════════════════════════════════════════════════════════════════
        // SUBJECTS
        // ══════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Subjects()
        {
            ViewData["Title"] = "Subjects";
            ViewBag.Gradients = Grads;
            return View("~/Areas/SuperAdmin/Views/Config/Subjects.cshtml",
                await _uow.Subjects.FindAllAsync(s => s.CurrentState == 1));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubject(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                await _uow.Subjects.AddAsync(new TbSubject { Name = name, ImageName = "", CurrentState = 1, CreatedBy = CurrentUserId, CreatedDate = DateTime.Now });
                _uow.Complete();
                TempData["Success"] = $"Subject \"{name}\" created.";
            }
            return RedirectToAction("Subjects");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubject(int id, string name)
        {
            var e = await _uow.Subjects.GetById(id);
            if (e != null) { e.Name = name; e.UpdatedBy = CurrentUserId; e.UpdatedDate = DateTime.Now; _uow.Subjects.Update(e); _uow.Complete(); TempData["Success"] = "Subject updated."; }
            return RedirectToAction("Subjects");
        }

        public async Task<IActionResult> DeleteSubject(int id)
        {
            var e = await _uow.Subjects.GetById(id);
            if (e != null) { e.CurrentState = 0; _uow.Subjects.Update(e); _uow.Complete(); TempData["Success"] = "Subject deleted."; }
            return RedirectToAction("Subjects");
        }

        // ══════════════════════════════════════════════════════════════════
        // SUB-SUBJECTS
        // ══════════════════════════════════════════════════════════════════
        public async Task<IActionResult> SubSubjects()
        {
            ViewData["Title"] = "Sub-Subjects";
            ViewBag.Gradients = Grads;
            ViewBag.Subjects = (await _uow.Subjects.FindAllAsync(s => s.CurrentState == 1)).ToList();
            return View("~/Areas/SuperAdmin/Views/Config/SubSubjects.cshtml",
                await _uow.SubSubjects.FindAllAsync(s => s.CurrentState == 1, new[] { "Subject" }));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubSubject(string name, int subjectId)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                await _uow.SubSubjects.AddAsync(new TbSubSubject { Name = name, SubjectId = subjectId, ImageName = "", CurrentState = 1, CreatedBy = CurrentUserId, CreatedDate = DateTime.Now });
                _uow.Complete();
                TempData["Success"] = $"Sub-Subject \"{name}\" created.";
            }
            return RedirectToAction("SubSubjects");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubSubject(int id, string name, int subjectId)
        {
            var e = await _uow.SubSubjects.GetById(id);
            if (e != null) { e.Name = name; e.SubjectId = subjectId; e.UpdatedBy = CurrentUserId; e.UpdatedDate = DateTime.Now; await _uow.SubSubjects.Update(e); _uow.Complete(); TempData["Success"] = "Sub-Subject updated."; }
            return RedirectToAction("SubSubjects");
        }

        public async Task<IActionResult> DeleteSubSubject(int id)
        {
            var e = await _uow.SubSubjects.GetById(id);
            if (e != null) { e.CurrentState = 0; _uow.SubSubjects.Update(e); _uow.Complete(); TempData["Success"] = "Sub-Subject deleted."; }
            return RedirectToAction("SubSubjects");
        }

        // ══════════════════════════════════════════════════════════════════
        // TERMS
        // ══════════════════════════════════════════════════════════════════
        public async Task<IActionResult> Terms()
        {
            ViewData["Title"] = "Terms";
            ViewBag.Gradients = Grads;
            return View("~/Areas/SuperAdmin/Views/Config/Terms.cshtml",
                await _uow.Terms.FindAllAsync(t => t.CurrentState == 1));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTerm(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                await _uow.Terms.AddAsync(new TbTerm { Name = name, ImageName = "", CurrentState = 1, CreatedBy = CurrentUserId, CreatedDate = DateTime.Now });
                _uow.Complete();
                TempData["Success"] = $"Term \"{name}\" created.";
            }
            return RedirectToAction("Terms");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTerm(int id, string name)
        {
            var e = await _uow.Terms.GetById(id);
            if (e != null) { e.Name = name; e.UpdatedBy = CurrentUserId; e.UpdatedDate = DateTime.Now; _uow.Terms.Update(e); _uow.Complete(); TempData["Success"] = "Term updated."; }
            return RedirectToAction("Terms");
        }

        public async Task<IActionResult> DeleteTerm(int id)
        {
            var e = await _uow.Terms.GetById(id);
            if (e != null) { e.CurrentState = 0; _uow.Terms.Update(e); _uow.Complete(); TempData["Success"] = "Term deleted."; }
            return RedirectToAction("Terms");
        }
    }
}