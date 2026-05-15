using AutoMapper;
using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Admin.ViewModel;
using Microsoft.AspNetCore.Mvc;
using MLSCore;
using MLSCore.Models;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            IEnumerable<TbGrade> grades = await _unitOfWork.Grades.FindAllAsync(a => a.CurrentState == 1);
            // return Ok(stages);
            return View(grades);
            
        }
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            List<SelectDropList> stageList = (await _unitOfWork.Stages.FindAllAsyncDroplist(x => x.CurrentState == 1,
                x => new SelectDropList
                { Id = x.Id, Name = x.Name })).ToList();
            GradeVM gradeVM = new GradeVM();
            gradeVM.Stages = stageList;

            return View(gradeVM);
        }
        [HttpPost]
        public async Task<ActionResult> Create(GradeVM gradeVM)
        {

            if (ModelState.IsValid)
            {
              //  List<SelectDropList> stageList = gradeVM.Stages;
                if (gradeVM.Image != null)
                {
                    string folder = "Images/images/";
                    gradeVM.ImageName = Upload.UploadImage(folder, gradeVM.Image);
                }

                TbGrade grade = _mapper.Map<TbGrade>(gradeVM);
                await _unitOfWork.Grades.AddAsync(grade);

                _unitOfWork.Complete();
                ViewBag.Messag = "subsubject Added Successfully";


            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            List<SelectDropList> stagelist = (await _unitOfWork.Stages.FindAllAsyncDroplist(x => x.CurrentState == 1,
                x => new SelectDropList
                { Id = x.Id, Name = x.Name })).ToList();

            var grade = await _unitOfWork.Grades.GetById(Id);
            GradeEditVM gradeVM = _mapper.Map<GradeEditVM>(grade);
            gradeVM.Stages = stagelist;
            return View(gradeVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(GradeEditVM gradevm)
        {
            if (ModelState.IsValid)
            {
                if (gradevm.Image != null)
                {
                    string prevImage = gradevm.ImageName;
                    //--------------------------delete image from folder
                    Upload.DeletImage(prevImage);
                    string folder = "Images/images/";
                    gradevm.ImageName = Upload.UploadImage(folder, gradevm.Image);
                }
                var grade = await _unitOfWork.Grades.GetById(gradevm.Id);
                _mapper.Map(gradevm, grade);

                await _unitOfWork.Grades.Update(grade);

                _unitOfWork.Complete();
                ViewBag.Messag = "subsubject Updated Successfully";

            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<ActionResult> Delete(int Id)

        {
            var grade = await _unitOfWork.Grades.GetById(Id);
            //--------------------------delete image from folder
            Upload.DeletImage(grade.ImageName);

            grade.CurrentState = 0;
            _unitOfWork.Grades.Update(grade);

            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
    }
}
