using AutoMapper;
using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Admin.ViewModel;
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
    [Authorize(Roles = "Instructor")]
    public class CourseController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CourseController(IUnitOfWork unitOfWork, IMapper mapper,
                                UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        private async Task<int> GetInstructorId()
        {
            var instructor = await _unitOfWork.Instructors
                .FindAsync(i => i.UserId == CurrentUserId);
            return instructor?.Id ?? 0;
        }

        // ── View assigned courses (read-only) ─────────────────────────────
        public async Task<IActionResult> Index()
        {
            var instructorId = await GetInstructorId();
            var courses = await _unitOfWork.Courses
                .FindAllAsync(c => c.CurrentState == 1 && c.InstructorId == instructorId);
            return View(courses);
        }

        // ── JSON helper for SubSubject cascade (still needed by other pages) ─
        public async Task<IActionResult> GetBySubjectId(int subjectId)
        {
            var subList = (await _unitOfWork.SubSubjects.FindAllAsyncDroplist(
                x => x.CurrentState == 1 && x.SubjectId == subjectId,
                x => new SelectDropList { Id = x.Id, Name = x.Name })).ToList();
            return Json(subList);
        }

        // ── Create / Edit / Delete are Admin/SuperAdmin only ───────────────
        // Kept as stubs returning 403 so any bookmarked URL or
        // direct navigation attempt is handled gracefully.

        [HttpGet]
        public IActionResult Create() => Forbid();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CreatePost() => Forbid();

        [HttpGet]
        public IActionResult Edit(int id) => Forbid();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EditPost(int id) => Forbid();

        [HttpGet]
        public IActionResult Delete(int id) => Forbid();
    }
}