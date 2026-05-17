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
    public class SubSubjectController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public SubSubjectController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Sub-Subjects";
            var subs = await _unitOfWork.SubSubjects
                .FindAllAsync(s => s.CurrentState == 1, new[] { "Subject" });

            var vm = subs.Select(s => new SubSubjectIndexVM
            {
                Id = s.Id,
                Name = s.Name,
                ImageName = s.ImageName,
                CurrentState = s.CurrentState,
                Subject = s.Subject?.Name ?? "—"
            }).ToList();

            return View("~/Areas/Admin/Views/SubSubject/Index.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Add Sub-Subject";
            var subjects = (await _unitOfWork.Subjects.FindAllAsyncDroplist(
                x => x.CurrentState == 1,
                x => new SelectDropList { Id = x.Id, Name = x.Name })).ToList();
            return View("~/Areas/Admin/Views/SubSubject/Create.cshtml",
                        new SubSubjectVM { Subjects = subjects });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubSubjectVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Subjects = (await _unitOfWork.Subjects.FindAllAsyncDroplist(
                    x => x.CurrentState == 1,
                    x => new SelectDropList { Id = x.Id, Name = x.Name })).ToList();
                return View("~/Areas/Admin/Views/SubSubject/Create.cshtml", vm);
            }

            await _unitOfWork.SubSubjects.AddAsync(new TbSubSubject
            {
                Name = vm.Name,
                SubjectId = vm.SubjectId,
                ImageName = "",
                CurrentState = 1,
                CreatedBy = User.Identity?.Name ?? "Admin",
                CreatedDate = DateTime.Now
            });
            _unitOfWork.Complete();
            TempData["Success"] = $"Sub-Subject \"{vm.Name}\" added successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            ViewData["Title"] = "Edit Sub-Subject";
            var sub = await _unitOfWork.SubSubjects.GetById(Id);
            if (sub == null) return NotFound();

            var vm = _mapper.Map<SubSubjectEditVM>(sub);
            vm.Subjects = (await _unitOfWork.Subjects.FindAllAsyncDroplist(
                x => x.CurrentState == 1,
                x => new SelectDropList { Id = x.Id, Name = x.Name })).ToList();

            return View("~/Areas/Admin/Views/SubSubject/Edit.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubSubjectEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Subjects = (await _unitOfWork.Subjects.FindAllAsyncDroplist(
                    x => x.CurrentState == 1,
                    x => new SelectDropList { Id = x.Id, Name = x.Name })).ToList();
                return View("~/Areas/Admin/Views/SubSubject/Edit.cshtml", vm);
            }

            // BUG FIX: original code updated _unitOfWork.Stages instead of SubSubjects
            var sub = await _unitOfWork.SubSubjects.GetById(vm.Id);
            if (sub == null) return NotFound();

            sub.Name = vm.Name;
            sub.SubjectId = vm.SubjectId;
            sub.UpdatedBy = User.Identity?.Name ?? "Admin";
            sub.UpdatedDate = DateTime.Now;

            await _unitOfWork.SubSubjects.Update(sub);
            _unitOfWork.Complete();
            TempData["Success"] = "Sub-Subject updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            var sub = await _unitOfWork.SubSubjects.GetById(Id);
            if (sub == null) return NotFound();

            if (!string.IsNullOrEmpty(sub.ImageName))
                Upload.DeletImage(sub.ImageName);

            sub.CurrentState = 0;
            _unitOfWork.SubSubjects.Update(sub);
            _unitOfWork.Complete();
            TempData["Success"] = "Sub-Subject deleted.";
            return RedirectToAction("Index");
        }
    }
}