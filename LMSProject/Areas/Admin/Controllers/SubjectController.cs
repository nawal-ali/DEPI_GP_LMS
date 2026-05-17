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
    public class SubjectController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public SubjectController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Subjects";
            var subjects = await _unitOfWork.Subjects.FindAllAsync(s => s.CurrentState == 1);
            return View("~/Areas/Admin/Views/Subject/Index.cshtml", subjects);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Subject";
            return View("~/Areas/Admin/Views/Subject/Create.cshtml", new SubjectVM());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectVM vm)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/Subject/Create.cshtml", vm);

            await _unitOfWork.Subjects.AddAsync(new TbSubject
            {
                Name = vm.Name,
                ImageName = "",
                CurrentState = 1,
                CreatedBy = User.Identity?.Name ?? "Admin",
                CreatedDate = DateTime.Now
            });
            _unitOfWork.Complete();
            TempData["Success"] = $"Subject \"{vm.Name}\" added successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            ViewData["Title"] = "Edit Subject";
            var subject = await _unitOfWork.Subjects.GetById(Id);
            if (subject == null) return NotFound();
            return View("~/Areas/Admin/Views/Subject/Edit.cshtml", _mapper.Map<SubjectEditVM>(subject));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubjectEditVM vm)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/Subject/Edit.cshtml", vm);

            var subject = await _unitOfWork.Subjects.GetById(vm.Id);
            if (subject == null) return NotFound();

            subject.Name = vm.Name;
            subject.UpdatedBy = User.Identity?.Name ?? "Admin";
            subject.UpdatedDate = DateTime.Now;

            _unitOfWork.Subjects.Update(subject);
            _unitOfWork.Complete();
            TempData["Success"] = "Subject updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            var subject = await _unitOfWork.Subjects.GetById(Id);
            if (subject == null) return NotFound();

            if (!string.IsNullOrEmpty(subject.ImageName))
                Upload.DeletImage(subject.ImageName);

            subject.CurrentState = 0;
            _unitOfWork.Subjects.Update(subject);
            _unitOfWork.Complete();
            TempData["Success"] = "Subject deleted.";
            return RedirectToAction("Index");
        }
    }
}