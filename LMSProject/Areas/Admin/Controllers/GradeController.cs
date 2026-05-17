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
    public class GradeController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public GradeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Grades";
            var grades = await _unitOfWork.Grades.FindAllAsync(g => g.CurrentState == 1);
            return View("~/Areas/Admin/Views/Grade/Index.cshtml", grades);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Add Grade";
            var stages = (await _unitOfWork.Stages.FindAllAsyncDroplist(
                s => s.CurrentState == 1,
                s => new SelectDropList { Id = s.Id, Name = s.Name })).ToList();
            return View("~/Areas/Admin/Views/Grade/Create.cshtml", new GradeVM { Stages = stages });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GradeVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Stages = (await _unitOfWork.Stages.FindAllAsyncDroplist(
                    s => s.CurrentState == 1,
                    s => new SelectDropList { Id = s.Id, Name = s.Name })).ToList();
                return View("~/Areas/Admin/Views/Grade/Create.cshtml", vm);
            }

            await _unitOfWork.Grades.AddAsync(new TbGrade
            {
                Name = vm.Name,
                StageId = vm.StageId,
                ImageName = "",
                CurrentState = 1,
                CreatedBy = User.Identity?.Name ?? "Admin",
                CreatedDate = DateTime.Now
            });
            _unitOfWork.Complete();
            TempData["Success"] = $"Grade \"{vm.Name}\" added successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            ViewData["Title"] = "Edit Grade";
            var grade = await _unitOfWork.Grades.GetById(Id);
            if (grade == null) return NotFound();

            var vm = _mapper.Map<GradeEditVM>(grade);
            vm.Stages = (await _unitOfWork.Stages.FindAllAsyncDroplist(
                s => s.CurrentState == 1,
                s => new SelectDropList { Id = s.Id, Name = s.Name })).ToList();
            return View("~/Areas/Admin/Views/Grade/Edit.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GradeEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Stages = (await _unitOfWork.Stages.FindAllAsyncDroplist(
                    s => s.CurrentState == 1,
                    s => new SelectDropList { Id = s.Id, Name = s.Name })).ToList();
                return View("~/Areas/Admin/Views/Grade/Edit.cshtml", vm);
            }

            var grade = await _unitOfWork.Grades.GetById(vm.Id);
            if (grade == null) return NotFound();

            grade.Name = vm.Name;
            grade.StageId = vm.StageId;
            grade.UpdatedBy = User.Identity?.Name ?? "Admin";
            grade.UpdatedDate = DateTime.Now;

            await _unitOfWork.Grades.Update(grade);
            _unitOfWork.Complete();
            TempData["Success"] = "Grade updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            var grade = await _unitOfWork.Grades.GetById(Id);
            if (grade == null) return NotFound();

            if (!string.IsNullOrEmpty(grade.ImageName))
                Upload.DeletImage(grade.ImageName);

            grade.CurrentState = 0;
            _unitOfWork.Grades.Update(grade);
            _unitOfWork.Complete();
            TempData["Success"] = "Grade deleted.";
            return RedirectToAction("Index");
        }
    }
}