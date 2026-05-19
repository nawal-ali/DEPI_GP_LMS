using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Admin.ViewModels;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class CoursesController : BaseController
    {
        private readonly AppDbContext _db;

        public CoursesController(AppDbContext db, UserManager<ApplicationUser> um) : base(um) => _db = db;

        // ── Index ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(string search = "", string grade = "",
                                               string status = "", int page = 1)
        {
            ViewData["Title"] = "Course Management";

            var query = _db.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Grade)
                .Include(c => c.SubSubject).ThenInclude(ss => ss != null ? ss.Subject : null)
                .Include(c => c.Term)
                .Include(c => c.StudentCourses)
                .Include(c => c.Assignments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));
            if (!string.IsNullOrWhiteSpace(grade))
                query = query.Where(c => c.Grade != null && c.Grade.Name == grade);
            if (status == "active") query = query.Where(c => c.CurrentState == 1);
            if (status == "inactive") query = query.Where(c => c.CurrentState == 0);

            var all = await query.ToListAsync();
            int ps = 10;

            var examCounts = await _db.Tests
                .Where(t => t.CurrentState == 1)
                .GroupBy(t => t.CourseId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => g.Count);

            var vmList = all.Select(c => new CourseListVM
            {
                Id = c.Id,
                Name = c.Name,
                ImageName = c.ImageName,
                InstructorName = c.Instructor?.FullName ?? "—",
                InstructorId = c.InstructorId,
                Grade = c.Grade?.Name ?? "",
                Subject = c.SubSubject?.Subject?.Name ?? "",
                Term = c.Term?.Name ?? "",
                Price = c.Price,
                Status = c.status,
                StudentCount = c.StudentCourses?.Count ?? 0,
                MaterialCount = 0,
                ExamCount = examCounts.GetValueOrDefault(c.Id),
                AssignmentCount = c.Assignments?.Count(a => a.CurrentState == 1) ?? 0,
                CurrentState = c.CurrentState
            }).ToList();

            ViewBag.Search = search;
            ViewBag.Grade = grade;
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)vmList.Count / ps);
            ViewBag.TotalCount = vmList.Count;
            ViewBag.Grades = await _db.Grades.Where(g => g.CurrentState == 1).Select(g => g.Name).ToListAsync();

            return View(vmList.Skip((page - 1) * ps).Take(ps).ToList());
        }

        // ── Create GET ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Create Course";
            return View(await BuildCreateVM(new CreateCourseVM()));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseVM vm)
        {
            if (!ModelState.IsValid) return View(await BuildCreateVM(vm));

            string? img = null;
            if (vm.Image != null) img = Upload.UploadImage("Images/images/", vm.Image);

            _db.Courses.Add(new TbCourse
            {
                Name = vm.Name,
                status = vm.Status,
                Price = vm.Price,
                ShowInHomePage = vm.ShowInHomePage,
                TermId = vm.TermId,
                GradeId = vm.GradeId,
                SubSubjId = vm.SubSubjId,
                InstructorId = vm.InstructorId,
                ImageName = img ?? "",
                CreatedBy = CurrentUserId,
                CreatedDate = DateTime.Now,
                CurrentState = 1
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Course created successfully.";
            return RedirectToAction("Index");
        }

        // ── Edit GET ───────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Edit Course";
            var c = await _db.Courses.Include(x => x.SubSubject).FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return NotFound();

            var vm = new EditCourseVM
            {
                Id = c.Id,
                Name = c.Name,
                Status = c.status,
                Price = c.Price,
                ShowInHomePage = c.ShowInHomePage,
                TermId = c.TermId,
                GradeId = c.GradeId,
                SubjId = c.SubSubject?.SubjectId ?? 0,
                SubSubjId = c.SubSubjId,
                InstructorId = c.InstructorId,
                ImageName = c.ImageName
            };
            return View(await BuildEditVM(vm));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCourseVM vm)
        {
            if (!ModelState.IsValid) return View(await BuildEditVM(vm));

            var c = await _db.Courses.FindAsync(vm.Id);
            if (c == null) return NotFound();

            if (vm.Image != null)
            {
                Upload.DeletImage(c.ImageName);
                c.ImageName = Upload.UploadImage("Images/images/", vm.Image);
            }

            c.Name = vm.Name; c.status = vm.Status; c.Price = vm.Price;
            c.ShowInHomePage = vm.ShowInHomePage; c.TermId = vm.TermId;
            c.GradeId = vm.GradeId; c.SubSubjId = vm.SubSubjId;
            c.InstructorId = vm.InstructorId;
            c.UpdatedBy = CurrentUserId; c.UpdatedDate = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Course updated.";
            return RedirectToAction("Index");
        }

        // ── Delete ─────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Courses.FindAsync(id);
            if (c != null) { c.CurrentState = 0; await _db.SaveChangesAsync(); }
            TempData["Success"] = "Course deactivated.";
            return RedirectToAction("Index");
        }

        // ── SubSubject cascade ─────────────────────────────────────────────
        public async Task<IActionResult> GetSubSubjects(int subjectId)
        {
            var list = await _db.SubSubjects
                .Where(ss => ss.SubjectId == subjectId && ss.CurrentState == 1)
                .Select(ss => new { id = ss.Id, name = ss.Name }).ToListAsync();
            return Json(list);
        }


        // ── Assign Instructor ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> AssignInstructor(int courseId)
        {
            ViewData["Title"] = "Assign Instructor";
            var course = await _db.Courses.Include(c => c.Instructor).FirstOrDefaultAsync(c => c.Id == courseId);
            if (course is null) return NotFound();

            ViewBag.Course = course;
            ViewBag.Instructors = await _db.Instructors
                .Where(i => i.CurrentState == 1)
                .OrderBy(i => i.FullName)
                .ToListAsync();
            return View("~/Areas/SuperAdmin/Views/Courses/AssignInstructor.cshtml");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignInstructor(int courseId, int instructorId)
        {
            var course = await _db.Courses.FindAsync(courseId);
            if (course is null) return NotFound();
            course.InstructorId = instructorId;
            course.UpdatedDate = DateTime.Now;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Instructor assigned successfully.";
            return RedirectToAction("Index");
        }

        // ── Enroll / Unenroll Students ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Enroll(int courseId, string search = "")
        {
            ViewData["Title"] = "Enroll Students";
            var course = await _db.Courses.Include(c => c.Grade).FirstOrDefaultAsync(c => c.Id == courseId);
            if (course is null) return NotFound();

            var enrolledIds = await _db.StudentCourses
                .Where(sc => sc.CourseId == courseId)
                .Select(sc => sc.StId).ToListAsync();

            var studentsQ = _db.Students
                .Include(s => s.Grade)
                .Include(s => s.User)   // Email lives on ApplicationUser
                .Where(s => s.CurrentState == 1);

            if (!string.IsNullOrEmpty(search))
                studentsQ = studentsQ.Where(s => s.FullName.Contains(search)
                    || (s.User != null && s.User.Email != null && s.User.Email.Contains(search)));

            var students = await studentsQ.OrderBy(s => s.FullName).ToListAsync();

            ViewBag.Course = course;
            ViewBag.EnrolledIds = enrolledIds;
            ViewBag.Students = students;
            ViewBag.Search = search;
            return View("~/Areas/SuperAdmin/Views/Courses/Enroll.cshtml");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudent(int courseId, int studentId)
        {
            var exists = await _db.StudentCourses.AnyAsync(sc => sc.CourseId == courseId && sc.StId == studentId);
            if (!exists)
            {
                _db.StudentCourses.Add(new TbStudentCourse { CourseId = courseId, StId = studentId });
                await _db.SaveChangesAsync();
                TempData["Success"] = "Student enrolled.";
            }
            else TempData["Error"] = "Student is already enrolled.";
            return RedirectToAction("Enroll", new { courseId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UnenrollStudent(int courseId, int studentId)
        {
            var sc = await _db.StudentCourses.FirstOrDefaultAsync(x => x.CourseId == courseId && x.StId == studentId);
            if (sc != null) { _db.StudentCourses.Remove(sc); await _db.SaveChangesAsync(); TempData["Success"] = "Student unenrolled."; }
            return RedirectToAction("Enroll", new { courseId });
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private async Task<CreateCourseVM> BuildCreateVM(CreateCourseVM vm)
        {
            vm.Terms = await _db.Terms.Where(t => t.CurrentState == 1)
                .Select(t => new SelectDropList { Id = t.Id, Name = t.Name }).ToListAsync();
            vm.Grades = await _db.Grades.Where(g => g.CurrentState == 1)
                .Select(g => new SelectDropList { Id = g.Id, Name = g.Name }).ToListAsync();
            vm.Subjects = await _db.Subjects.Where(s => s.CurrentState == 1)
                .Select(s => new SelectDropList { Id = s.Id, Name = s.Name }).ToListAsync();
            vm.SubSubjects = await _db.SubSubjects.Where(ss => ss.CurrentState == 1)
                .Select(ss => new SelectDropList { Id = ss.Id, Name = ss.Name }).ToListAsync();
            vm.Instructors = await _db.Instructors.Where(i => i.CurrentState == 1)
                .Select(i => new SelectDropList { Id = i.Id, Name = i.FullName }).ToListAsync();
            return vm;
        }

        private async Task<EditCourseVM> BuildEditVM(EditCourseVM vm)
        {
            var base_ = await BuildCreateVM(vm);
            base_.SubSubjects = await _db.SubSubjects
                .Where(ss => ss.SubjectId == vm.SubjId && ss.CurrentState == 1)
                .Select(ss => new SelectDropList { Id = ss.Id, Name = ss.Name }).ToListAsync();
            return (EditCourseVM)base_;
        }
    }
}