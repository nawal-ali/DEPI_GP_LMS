using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.Instructor.ViewModel;
using LMSProject.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class MaterialsController : BaseController
    {
        private readonly AppDbContext _context;

        public MaterialsController(AppDbContext context, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _context = context;
        }

        private async Task<int?> GetInstructorIdAsync()
        {
            var userId = CurrentUserId;
            return await _context.Instructors
                .Where(i => i.UserId == userId)
                .Select(i => (int?)i.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<List<int>> GetMyCourseIdsAsync(int instructorId)
        {
            return await _context.Courses
                .Where(c => c.InstructorId == instructorId && c.CurrentState == 1)
                .Select(c => c.Id)
                .ToListAsync();
        }

        private async Task<List<CourseDropItem>> GetMyCoursesDropAsync(int instructorId)
        {
            return await _context.Courses
                .Where(c => c.InstructorId == instructorId && c.CurrentState == 1)
                .Select(c => new CourseDropItem { Id = c.Id, Name = c.Name })
                .ToListAsync();
        }

        public async Task<IActionResult> Index()
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            // Guard: instructor has no assigned courses
            var hasCourses = await _context.Courses
                .AnyAsync(c => c.InstructorId == instructorId.Value && c.CurrentState == 1);
            if (!hasCourses)
            {
                ViewData["Title"] = "Materials";
                return View("~/Areas/Instructor/Views/Shared/_NoCourseAccess.cshtml", "Materials");
            }

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);

            var materials = await _context.CourseMaterials
                .Where(m => courseIds.Contains(m.CourseId) && m.CurrentState == 1)
                .Include(m => m.Course)
                .OrderByDescending(m => m.CreatedDate)
                .Select(m => new MaterialListItemVM
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    FileUrl = m.FileUrl,
                    FileName = m.FileName,
                    MaterialType = m.MaterialType,
                    CourseName = m.Course.Name,
                    CourseId = m.CourseId,
                    CreatedDate = m.CreatedDate
                })
                .ToListAsync();

            ViewData["Title"] = "Course Materials";
            return View(materials);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var vm = new CreateMaterialVM
            {
                Courses = await GetMyCoursesDropAsync(instructorId.Value)
            };
            ViewData["Title"] = "Add Material";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMaterialVM vm)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            if (vm.MaterialType != MaterialType.Link && vm.FileUpload == null)
                ModelState.AddModelError("FileUpload", "Please upload a file for this material type.");

            if (vm.MaterialType == MaterialType.Link && string.IsNullOrWhiteSpace(vm.ExternalUrl))
                ModelState.AddModelError("ExternalUrl", "Please enter an external URL.");

            if (!ModelState.IsValid)
            {
                vm.Courses = await GetMyCoursesDropAsync(instructorId.Value);
                ViewData["Title"] = "Add Material";
                return View(vm);
            }

            string fileUrl;
            string? fileName = null;

            if (vm.MaterialType == MaterialType.Link)
            {
                fileUrl = vm.ExternalUrl!;
            }
            else
            {
                string folder = "Images/materials/";
                fileUrl = Upload.UploadImage(folder, vm.FileUpload!);
                fileName = vm.FileUpload!.FileName;
            }

            var material = new TbCourseMaterial
            {
                Title = vm.Title,
                Description = vm.Description,
                FileUrl = fileUrl,
                FileName = fileName,
                MaterialType = vm.MaterialType,
                CourseId = vm.CourseId,
                CreatedBy = CurrentUserId,
                CurrentState = 1
            };

            _context.CourseMaterials.Add(material);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Material added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var material = await _context.CourseMaterials
                .FirstOrDefaultAsync(m => m.Id == id && courseIds.Contains(m.CourseId));

            if (material == null) return NotFound();

            var vm = new EditMaterialVM
            {
                Id = material.Id,
                Title = material.Title,
                Description = material.Description,
                ExistingFileUrl = material.FileUrl,
                ExistingFileName = material.FileName,
                MaterialType = material.MaterialType,
                CourseId = material.CourseId,
                ExternalUrl = material.MaterialType == MaterialType.Link ? material.FileUrl : null,
                Courses = await GetMyCoursesDropAsync(instructorId.Value)
            };
            ViewData["Title"] = "Edit Material";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditMaterialVM vm)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var material = await _context.CourseMaterials
                .FirstOrDefaultAsync(m => m.Id == vm.Id && courseIds.Contains(m.CourseId));

            if (material == null) return NotFound();

            if (vm.MaterialType == MaterialType.Link && string.IsNullOrWhiteSpace(vm.ExternalUrl))
                ModelState.AddModelError("ExternalUrl", "Please enter an external URL.");

            if (!ModelState.IsValid)
            {
                vm.Courses = await GetMyCoursesDropAsync(instructorId.Value);
                ViewData["Title"] = "Edit Material";
                return View(vm);
            }

            if (vm.MaterialType == MaterialType.Link)
            {
                material.FileUrl = vm.ExternalUrl!;
                material.FileName = null;
            }
            else if (vm.FileUpload != null)
            {
                if (!string.IsNullOrEmpty(material.FileUrl) && material.MaterialType != MaterialType.Link)
                    Upload.DeletImage(material.FileUrl);

                string folder = "Images/materials/";
                material.FileUrl = Upload.UploadImage(folder, vm.FileUpload);
                material.FileName = vm.FileUpload.FileName;
            }

            material.Title = vm.Title;
            material.Description = vm.Description;
            material.MaterialType = vm.MaterialType;
            material.CourseId = vm.CourseId;
            material.UpdatedBy = CurrentUserId;
            material.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Material updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == null) return NotFound();

            var courseIds = await GetMyCourseIdsAsync(instructorId.Value);
            var material = await _context.CourseMaterials
                .FirstOrDefaultAsync(m => m.Id == id && courseIds.Contains(m.CourseId));

            if (material == null) return NotFound();

            material.CurrentState = 0;
            material.UpdatedBy = CurrentUserId;
            material.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Material deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}