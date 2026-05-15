using AutoMapper;
using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Instructor.ViewModel;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MLSCore;
using MLSCore.IdentityModel;
using MLSCore.Models;

namespace LMSProject.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize]
    public class CourseScheduleController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public CourseScheduleController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        : base(userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

        }
        
        public async Task<int> GetInstructorId()
        {
            var userId = CurrentUserId;
            TbInstructor instructor = await _unitOfWork.Instructors
                .FindAsync(i => i.UserId == userId);
            var instructorId = instructor.Id;
            return instructorId;
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CourseGroupVM courseGroupVM = new CourseGroupVM();
            var instructorId = await GetInstructorId();

            List<SelectDropList> courseList = (await _unitOfWork.Courses.FindAllAsyncDroplist(a => a.CurrentState == 1 && a.InstructorId == instructorId,
                 x => new SelectDropList
                 { Id = x.Id, Name = x.Name })).ToList();
            courseGroupVM.Courses = courseList;
            return View(courseGroupVM);
        }

        [HttpPost]
        public async Task<ActionResult> Create(CourseGroupVM coursegroupVM)
        {

            if (ModelState.IsValid)
            {
              

                TbCourseGroup coursegroup = _mapper.Map<TbCourseGroup>(coursegroupVM);
              
                await _unitOfWork.CourseGroups.AddAsync(coursegroup);

                _unitOfWork.Complete();
                ViewBag.Messag = "group Added Successfully";


            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Index()
        {
            var instructorId = await GetInstructorId();


            IEnumerable<TbCourse> courses = await _unitOfWork.Courses.FindAllAsync(a => a.CurrentState == 1 && a.InstructorId == instructorId);
            // return Ok(stages);
            return View(courses);


        }

    }
}
