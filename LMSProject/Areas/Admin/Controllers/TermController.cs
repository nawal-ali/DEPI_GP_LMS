using AutoMapper;
using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Admin.ViewModel;
using Microsoft.AspNetCore.Mvc;
using MLSCore;
using MLSCore.Models;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            IEnumerable<TbTerm> terms = await _unitOfWork.Terms.FindAllAsync(a => a.CurrentState == 1);
            // return Ok(stages);
            return View(terms);
        }
        [HttpGet]
        public async Task<ActionResult> Create()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(TermVM termVM)
        {

            if (ModelState.IsValid)
            {
                if (termVM.Image != null)
                {
                    string folder = "Images/images/";
                    termVM.ImageName = Upload.UploadImage(folder, termVM.Image);
                }

                TbTerm term = _mapper.Map<TbTerm>(termVM);
                await _unitOfWork.Terms.AddAsync(term);

                _unitOfWork.Complete();
                ViewBag.Messag = "Term Added Successfully";

            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            var term = await _unitOfWork.Terms.GetById(Id);
            TermEditVM termVM = _mapper.Map<TermEditVM>(term);
            return View(termVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TermEditVM termvm)
        {
            if (ModelState.IsValid)
            {
                if (termvm.Image != null)
                {
                    string prevImage = termvm.ImageName;
                    //--------------------------delete image from folder
                    Upload.DeletImage(prevImage);
                    string folder = "Images/images/";
                    termvm.ImageName = Upload.UploadImage(folder, termvm.Image);
                }
                var term = await _unitOfWork.Terms.GetById(termvm.Id);
                _mapper.Map(termvm, term);

                _unitOfWork.Terms.Update(term);

                _unitOfWork.Complete();
                ViewBag.Messag = "Term Added Successfully";

            }
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int Id)

        {
            var term = await _unitOfWork.Terms.GetById(Id);
            //--------------------------delete image from folder
            Upload.DeletImage(term.ImageName);

            term.CurrentState = 0;
            _unitOfWork.Terms.Update(term);

            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int Id)
        {
            return View();
        }
    }
}