using LMSProject.Areas.Admin.Helpers;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore;
using MLSCore.IdentityModel;
using MLSCore.Models;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class GradesController : BaseController
    {
        private readonly IUnitOfWork _uow;

        public GradesController(IUnitOfWork uow, UserManager<ApplicationUser> um) : base(um) => _uow = uow;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Grades Management";
            var grades = await _uow.Grades.FindAllAsync(g => g.CurrentState == 1);
            return View(grades);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Grade";
            return View(new TbGrade());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TbGrade vm, IFormFile? Image)
        {
            if (!ModelState.IsValid) return View(vm);

            string? img = null;
            if (Image != null) img = Upload.UploadImage("Images/images/", Image);

            var grade = new TbGrade
            {
                Name = vm.Name,
                ImageName = img ?? "",
                CurrentState = 1,
                CreatedBy = CurrentUserId,
                CreatedDate = DateTime.Now
            };
            await _uow.Grades.AddAsync(grade);
            _uow.Complete();
            TempData["Success"] = "Grade added.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Edit Grade";
            var g = await _uow.Grades.GetById(id);
            if (g == null) return NotFound();
            return View(g);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TbGrade vm, IFormFile? Image)
        {
            var g = await _uow.Grades.GetById(vm.Id);
            if (g == null) return NotFound();

            if (Image != null) { Upload.DeletImage(g.ImageName); g.ImageName = Upload.UploadImage("Images/images/", Image); }
            g.Name = vm.Name;
            g.UpdatedBy = CurrentUserId;
            g.UpdatedDate = DateTime.Now;
            await _uow.Grades.Update(g);
            _uow.Complete();
            TempData["Success"] = "Grade updated.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var g = await _uow.Grades.GetById(id);
            if (g != null) { g.CurrentState = 0; await _uow.Grades.Update(g); _uow.Complete(); }
            TempData["Success"] = "Grade deleted.";
            return RedirectToAction("Index");
        }
    }
}