using AutoMapper;
using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Admin.ViewModel;
using Microsoft.AspNetCore.Mvc;
using MLSCore;
using MLSCore.Models;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            IEnumerable<TbSubject> subjects = await _unitOfWork.Subjects.FindAllAsync(a => a.CurrentState == 1);
            // return Ok(stages);
            return View(subjects);
        }
        [HttpGet]
        public async Task<ActionResult> Create()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(SubjectVM subjectVM)
        {

            if (ModelState.IsValid)
            {
                if (subjectVM.Image != null)
                {
                    string folder = "Images/images/";
                    subjectVM.ImageName = Upload.UploadImage(folder, subjectVM.Image);
                }

                TbSubject subject = _mapper.Map<TbSubject>(subjectVM);
                await _unitOfWork.Subjects.AddAsync(subject);

                _unitOfWork.Complete();
                ViewBag.Messag = "Stage Added Successfully";

            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            var subject = await _unitOfWork.Subjects.GetById(Id);
            SubjectEditVM subjectvm = _mapper.Map<SubjectEditVM>(subject);
            return View(subjectvm);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(SubjectEditVM subjectvm)
        {
            if (ModelState.IsValid)
            {
                if (subjectvm.Image != null)
                {
                    string prevImage = subjectvm.ImageName;
                    //--------------------------delete image from folder
                    Upload.DeletImage(prevImage);
                    string folder = "Images/images/";
                    subjectvm.ImageName = Upload.UploadImage(folder, subjectvm.Image);
                }
                var Stage = await _unitOfWork.Stages.GetById(subjectvm.Id);
                _mapper.Map(subjectvm, Stage);

                _unitOfWork.Stages.Update(Stage);

                _unitOfWork.Complete();
                ViewBag.Messag = "Stage Updated Successfully";

            }
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> Delete(int Id)

        {
            var subject = await _unitOfWork.Subjects.GetById(Id);
            //--------------------------delete image from folder
            Upload.DeletImage(subject.ImageName);

            subject.CurrentState = 0;
            _unitOfWork.Subjects.Update(subject);

            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }


    }
}
