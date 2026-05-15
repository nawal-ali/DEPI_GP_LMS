using AutoMapper;
using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Admin.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore;
using MLSCore.Models;
using System.Runtime.InteropServices;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StageController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public StageController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;


        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<TbStage> stages = await _unitOfWork.Stages.FindAllAsync(a => a.CurrentState == 1);
            // return Ok(stages);
            return View(stages);
        }
        [HttpGet]
        public async Task<ActionResult> Create()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(StageVM stageVM)
        {
         
            if (ModelState.IsValid)
            {
                if (stageVM.Image != null)
                {
                    string folder = "Images/images/";
                    stageVM.ImageName = Upload.UploadImage(folder, stageVM.Image);
                }

                TbStage stage = _mapper.Map<TbStage>(stageVM);
                await _unitOfWork.Stages.AddAsync(stage);

                _unitOfWork.Complete();
                ViewBag.Messag = "Stage Added Successfully";

            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            var Stage = await _unitOfWork.Stages.GetById(Id);
            StageEditVM stageVM = _mapper.Map<StageEditVM>(Stage);
            return View(stageVM); }

        [HttpPost]
        public async Task<IActionResult> Edit(StageEditVM stagevm)
        {
            if (ModelState.IsValid)
            {
                if (stagevm.Image != null)
                {
                    string prevImage = stagevm.ImageName;
                    //--------------------------delete image from folder
                    Upload.DeletImage(prevImage);
                    string folder = "Images/images/";
                    stagevm.ImageName = Upload.UploadImage(folder, stagevm.Image);
                }
                var Stage = await _unitOfWork.Stages.GetById(stagevm.Id);
                _mapper.Map(stagevm, Stage);

               await _unitOfWork.Stages.Update(Stage);

                _unitOfWork.Complete();
                ViewBag.Messag = "Stage Updated Successfully";

            }
            return View();
        }

         [HttpGet]
        public async Task<ActionResult> Delete(int Id)

        {
            var Stage = await _unitOfWork.Stages.GetById(Id);
            //--------------------------delete image from folder
            Upload.DeletImage(Stage.ImageName);

            Stage.CurrentState = 0;
            _unitOfWork.Stages.Update(Stage);

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