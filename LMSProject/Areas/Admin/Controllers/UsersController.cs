using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Admin.ViewModel;
using LMSProject.Areas.Admin.ViewModels;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSEF;

namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UsersController : BaseController
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userMgr;

        public UsersController(AppDbContext db, UserManager<ApplicationUser> um) : base(um)
        {
            _db = db;
            _userMgr = um;
        }

        // ── Instructors ────────────────────────────────────────────────────
        public async Task<IActionResult> Instructors(string search = "", string status = "", int page = 1)
        {
            ViewData["Title"] = "Users — Instructors";

            var query = _db.Instructors
                .Include(i => i.Courses)
                .Include(i => i.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(i => i.FullName.Contains(search) || i.User.Email.Contains(search));
            if (status == "active") query = query.Where(i => i.CurrentState == 1);
            if (status == "inactive") query = query.Where(i => i.CurrentState == 0);

            var all = await query.ToListAsync();
            int pageSize = 10;

            var vm = new UserListVM<InstructorListItemVM>
            {
                SearchTerm = search,
                FilterStatus = status,
                TotalCount = all.Count,
                CurrentPage = page,
                PageSize = pageSize,
                Items = all.Select(i => new InstructorListItemVM
                {
                    Id = i.Id,
                    FullName = i.FullName,
                    Email = i.User?.Email ?? "",
                    Phone = i.User?.PhoneNumber ?? "",
                    Specialization = i.Specialization,
                    ExperienceYears = i.ExperienceYears,
                    CourseCount = i.Courses?.Count ?? 0,
                    ImageName = i.ImageName,
                    CurrentState = i.CurrentState
                }).ToList()
            };
            vm.Paged = vm.Items.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> EditInstructor(int id)
        {
            ViewData["Title"] = "Edit Instructor";
            var i = await _db.Instructors.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (i == null) return NotFound();

            var vm = new LMSProject.Areas.Admin.ViewModel.InstructorEditVM
            {
                Id = i.Id,
                FullName = i.FullName,
                UserName = i.User?.UserName ?? "",
                Email = i.User?.Email ?? "",
                PhoneNumber = i.User?.PhoneNumber ?? "",
                Bio = i.Bio,
                Specialization = i.Specialization,
                ExperienceYears = i.ExperienceYears,
                ShowInHomePage = i.ShowInHomePage,
                ImageName = i.ImageName
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInstructor(LMSProject.Areas.Admin.ViewModel.InstructorEditVM vm)
        {
            if (!ModelState.IsValid) { ViewData["Title"] = "Edit Instructor"; return View(vm); }

            var instr = await _db.Instructors.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == vm.Id);
            if (instr == null) return NotFound();

            // Update Identity user
            if (instr.User != null)
            {
                instr.User.UserName = vm.UserName;
                instr.User.Email = vm.Email;
                instr.User.PhoneNumber = vm.PhoneNumber;
                await _userMgr.UpdateAsync(instr.User);
                if (!string.IsNullOrEmpty(vm.Password))
                {
                    var token = await _userMgr.GeneratePasswordResetTokenAsync(instr.User);
                    await _userMgr.ResetPasswordAsync(instr.User, token, vm.Password);
                }
            }

            // Update image
            if (vm.Image != null)
            {
                Upload.DeletImage(instr.ImageName);
                instr.ImageName = Upload.UploadImage("Images/images/", vm.Image);
            }

            instr.FullName = vm.FullName;
            instr.Bio = vm.Bio;
            instr.Specialization = vm.Specialization;
            instr.ExperienceYears = vm.ExperienceYears;
            instr.ShowInHomePage = vm.ShowInHomePage;
            instr.UpdatedBy = CurrentUserId;
            instr.UpdatedDate = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Instructor updated successfully.";
            return RedirectToAction("Instructors");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateInstructor(int id)
        {
            var i = await _db.Instructors.FindAsync(id);
            if (i != null) { i.CurrentState = i.CurrentState == 1 ? 0 : 1; await _db.SaveChangesAsync(); }
            TempData["Success"] = "Instructor status updated.";
            return RedirectToAction("Instructors");
        }

        // ── Students ───────────────────────────────────────────────────────
        public async Task<IActionResult> Students(string search = "", string grade = "", int page = 1)
        {
            ViewData["Title"] = "Users — Students";

            var query = _db.Students
                .Include(s => s.Grade)
                .Include(s => s.User)
                .Include(s => s.Parent)
                .Include(s => s.StudentCourses)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.FullName.Contains(search) || s.User.Email.Contains(search));
            if (!string.IsNullOrWhiteSpace(grade))
                query = query.Where(s => s.Grade.Name == grade);

            var all = await query.ToListAsync();
            int pageSize = 10;

            var vm = new UserListVM<StudentListItemVM>
            {
                SearchTerm = search,
                FilterExtra = grade,
                TotalCount = all.Count,
                CurrentPage = page,
                PageSize = pageSize,
                Items = all.Select(s => new StudentListItemVM
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.User?.Email ?? "",
                    Phone = s.User?.PhoneNumber ?? "",
                    Grade = s.Grade?.Name ?? "",
                    CourseCount = s.StudentCourses?.Count ?? 0,
                    ImageName = s.ImageName,
                    CurrentState = s.CurrentState,
                    ParentName = s.Parent?.FullName
                }).ToList()
            };
            vm.Paged = vm.Items.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.Grades = await _db.Grades.Where(g => g.CurrentState == 1).Select(g => g.Name).ToListAsync();
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            ViewData["Title"] = "Edit Student";
            var s = await _db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound();
            var vm = new EditStudentVM
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.User?.Email ?? "",
                Phone = s.User?.PhoneNumber ?? "",
                GradeId = s.GradeId,
                ImageName = s.ImageName,
                Grades = await _db.Grades.Where(g => g.CurrentState == 1)
                    .Select(g => new SelectDropList { Id = g.Id, Name = g.Name }).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(EditStudentVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Grades = await _db.Grades.Where(g => g.CurrentState == 1)
                    .Select(g => new SelectDropList { Id = g.Id, Name = g.Name }).ToListAsync();
                ViewData["Title"] = "Edit Student";
                return View(vm);
            }
            var s = await _db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == vm.Id);
            if (s == null) return NotFound();
            if (s.User != null)
            {
                s.User.Email = vm.Email;
                s.User.PhoneNumber = vm.Phone;
                await _userMgr.UpdateAsync(s.User);
            }
            if (vm.Image != null) { Upload.DeletImage(s.ImageName); s.ImageName = Upload.UploadImage("Images/images/", vm.Image); }
            s.FullName = vm.FullName;
            s.GradeId = vm.GradeId;
            s.UpdatedBy = CurrentUserId;
            s.UpdatedDate = DateTime.Now;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Student updated successfully.";
            return RedirectToAction("Students");
        }

        // ── Parents ────────────────────────────────────────────────────────
        public async Task<IActionResult> Parents(string search = "", int page = 1)
        {
            ViewData["Title"] = "Users — Parents";

            var query = _db.Parents
                .Include(p => p.Children)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.FullName.Contains(search));

            var all = await query.ToListAsync();
            int pageSize = 10;

            var vm = new UserListVM<ParentListItemVM>
            {
                SearchTerm = search,
                TotalCount = all.Count,
                CurrentPage = page,
                PageSize = pageSize,
                Items = all.Select(p => new ParentListItemVM
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    Email = p.Email ?? "",
                    Phone = p.PhoneNumber ?? "",
                    ChildrenCount = p.Children?.Count ?? 0,
                    ChildNames = p.Children?.Select(s => s.FullName).ToList() ?? new(),
                    CurrentState = 1
                }).ToList()
            };
            vm.Paged = vm.Items.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> EditParent(int id)
        {
            ViewData["Title"] = "Edit Parent";
            var p = await _db.Parents.FindAsync(id);
            if (p == null) return NotFound();
            return View(new EditParentVM { Id = p.Id, FullName = p.FullName, Email = p.Email ?? "", Phone = p.PhoneNumber });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditParent(EditParentVM vm)
        {
            if (!ModelState.IsValid) { ViewData["Title"] = "Edit Parent"; return View(vm); }
            var p = await _db.Parents.FindAsync(vm.Id);
            if (p == null) return NotFound();
            p.FullName = vm.FullName;
            p.Email = vm.Email;
            p.PhoneNumber = vm.Phone;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Parent updated successfully.";
            return RedirectToAction("Parents");
        }
    }
}