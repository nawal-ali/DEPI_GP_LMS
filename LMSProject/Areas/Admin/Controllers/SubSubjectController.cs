using AutoMapper;
using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Admin.ViewModel;
using Microsoft.AspNetCore.Mvc;
using MLSCore;
using MLSCore.Models;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            var subsubjects = await _unitOfWork.SubSubjects.FindAllAsync(a => a.CurrentState == 1, new[] {"Subject"});
          List<SubSubjectIndexVM> subsubjectsListVM = new List<SubSubjectIndexVM>();
            foreach(var x in subsubjects)
            {
                subsubjectsListVM.Add(
                new SubSubjectIndexVM()
                {
                    Id = x.Id,
                    Name = x.Name,
                    CurrentState = x.CurrentState,
                    ImageName = x.ImageName,
                    Subject = x.Subject.Name
                }
                );
            }
            // return Ok(stages);
            return View(subsubjectsListVM);
        }
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            List<SelectDropList> subList = (await _unitOfWork.Subjects.FindAllAsyncDroplist(x => x.CurrentState == 1,
                x => new SelectDropList
            { Id=x.Id,Name=x.Name }   )).ToList();
            SubSubjectVM subVM = new SubSubjectVM();
            subVM.Subjects = subList;

            return View(subVM);
        }
        [HttpPost]
        public async Task<ActionResult> Create(SubSubjectVM subsubjectVM)
        {

            if (ModelState.IsValid)
            {
               // List<SelectDropList> subList = subsubjectVM.Subjects;
                if (subsubjectVM.Image != null)
                {
                    string folder = "Images/images/";
                    subsubjectVM.ImageName = Upload.UploadImage(folder, subsubjectVM.Image);
                }

                TbSubSubject subsubject = _mapper.Map<TbSubSubject>(subsubjectVM);
                await _unitOfWork.SubSubjects.AddAsync(subsubject);

                _unitOfWork.Complete();
                ViewBag.Messag = "subsubject Added Successfully";
               
               
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            List<SelectDropList> subList = (await _unitOfWork.Subjects.FindAllAsyncDroplist(x => x.CurrentState == 1,
                x => new SelectDropList
                { Id = x.Id, Name = x.Name })).ToList();

            var subsubject = await _unitOfWork.SubSubjects.GetById(Id);
            SubSubjectEditVM subsubjectVM = _mapper.Map<SubSubjectEditVM>(subsubject);
            subsubjectVM.Subjects = subList;
            return View(subsubjectVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SubSubjectEditVM subsubjectvm)
        {
            if (ModelState.IsValid)
            {
                if (subsubjectvm.Image != null)
                {
                    string prevImage = subsubjectvm.ImageName;
                    //--------------------------delete image from folder
                    Upload.DeletImage(prevImage);
                    string folder = "Images/images/";
                    subsubjectvm.ImageName = Upload.UploadImage(folder, subsubjectvm.Image);
                }
                var subsubject = await _unitOfWork.SubSubjects.GetById(subsubjectvm.Id);
                _mapper.Map(subsubjectvm, subsubject);

                await _unitOfWork.SubSubjects.Update(subsubject);

                _unitOfWork.Complete();
                ViewBag.Messag = "subsubject Updated Successfully";

            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int Id)

        {
            var subsubject = await _unitOfWork.SubSubjects.GetById(Id);
            //--------------------------delete image from folder
            Upload.DeletImage(subsubject.ImageName);

            subsubject.CurrentState = 0;
            _unitOfWork.SubSubjects.Update(subsubject);

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