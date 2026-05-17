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
    public class TermController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public TermController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Terms";
            var terms = await _unitOfWork.Terms.FindAllAsync(t => t.CurrentState == 1);
            return View("~/Areas/Admin/Views/Term/Index.cshtml", terms);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Term";
            return View("~/Areas/Admin/Views/Term/Create.cshtml", new TermVM());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TermVM vm)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/Term/Create.cshtml", vm);

            await _unitOfWork.Terms.AddAsync(new TbTerm
            {
                Name = vm.Name,
                ImageName = "",
                CurrentState = 1,
                CreatedBy = User.Identity?.Name ?? "Admin",
                CreatedDate = DateTime.Now
            });
            _unitOfWork.Complete();
            TempData["Success"] = $"Term \"{vm.Name}\" added successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            ViewData["Title"] = "Edit Term";
            var term = await _unitOfWork.Terms.GetById(Id);
            if (term == null) return NotFound();
            return View("~/Areas/Admin/Views/Term/Edit.cshtml", _mapper.Map<TermEditVM>(term));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TermEditVM vm)
        {
            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/Term/Edit.cshtml", vm);

            var term = await _unitOfWork.Terms.GetById(vm.Id);
            if (term == null) return NotFound();

            term.Name = vm.Name;
            term.UpdatedBy = User.Identity?.Name ?? "Admin";
            term.UpdatedDate = DateTime.Now;

            _unitOfWork.Terms.Update(term);
            _unitOfWork.Complete();
            TempData["Success"] = "Term updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            var term = await _unitOfWork.Terms.GetById(Id);
            if (term == null) return NotFound();

            if (!string.IsNullOrEmpty(term.ImageName))
                Upload.DeletImage(term.ImageName);

            term.CurrentState = 0;
            _unitOfWork.Terms.Update(term);
            _unitOfWork.Complete();
            TempData["Success"] = "Term deleted.";
            return RedirectToAction("Index");
        }
    }
}