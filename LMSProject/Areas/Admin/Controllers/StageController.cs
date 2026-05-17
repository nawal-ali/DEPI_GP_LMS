using AutoMapper;
using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Admin.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MLSCore;
using MLSCore.Models;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class StageController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public StageController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ── Index ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Stages";
            var stages = await _unitOfWork.Stages.FindAllAsync(s => s.CurrentState == 1);
            return View("~/Areas/Admin/Views/Stage/Index.cshtml", stages);
        }

        // ── Create ─────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Stage";
            return View("~/Areas/Admin/Views/Stage/Create.cshtml", new StageVM());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StageVM vm)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/Stage/Create.cshtml", vm);

            // No image upload for stages — icon used instead
            var stage = new TbStage
            {
                Name = vm.Name,
                ImageName = "",
                CurrentState = 1,
                CreatedBy = User.Identity?.Name ?? "Admin",
                CreatedDate = DateTime.Now
            };

            await _unitOfWork.Stages.AddAsync(stage);
            _unitOfWork.Complete();
            TempData["Success"] = $"Stage \"{vm.Name}\" added successfully.";
            return RedirectToAction("Index");
        }

        // ── Edit ───────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            ViewData["Title"] = "Edit Stage";
            var stage = await _unitOfWork.Stages.GetById(Id);
            if (stage == null) return NotFound();
            return View("~/Areas/Admin/Views/Stage/Edit.cshtml", _mapper.Map<StageEditVM>(stage));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StageEditVM vm)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/Stage/Edit.cshtml", vm);

            var stage = await _unitOfWork.Stages.GetById(vm.Id);
            if (stage == null) return NotFound();

            stage.Name = vm.Name;
            stage.UpdatedBy = User.Identity?.Name ?? "Admin";
            stage.UpdatedDate = DateTime.Now;

            await _unitOfWork.Stages.Update(stage);
            _unitOfWork.Complete();
            TempData["Success"] = "Stage updated successfully.";
            return RedirectToAction("Index");
        }

        // ── Delete ─────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            var stage = await _unitOfWork.Stages.GetById(Id);
            if (stage == null) return NotFound();

            // Safe delete — only tries to delete image if one exists
            if (!string.IsNullOrEmpty(stage.ImageName))
                Upload.DeletImage(stage.ImageName);

            stage.CurrentState = 0;
            _unitOfWork.Stages.Update(stage);
            _unitOfWork.Complete();
            TempData["Success"] = "Stage deleted.";
            return RedirectToAction("Index");
        }
    }
}