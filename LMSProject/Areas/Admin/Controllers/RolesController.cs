using LMSProject.Areas.Admin.ViewModel;
using LMSProject.Areas.Admin.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LMSProject.Areas.Admin.Controllers
{
    [Area("Admin")]  //[Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }
        [HttpGet]
        public async Task<ActionResult> Create()
        {

            return View();
        }
        [HttpPost]

        public async Task<IActionResult> Create(RoleFormVM model)
        {
            if (!ModelState.IsValid)
                return View("Index", await _roleManager.Roles.ToListAsync());

            if (await _roleManager.RoleExistsAsync(model.Name))
            {
                ModelState.AddModelError("Name", "Role is exists!");
                return View("Index", await _roleManager.Roles.ToListAsync());
            }

            await _roleManager.CreateAsync(new IdentityRole(model.Name.Trim()));
            ViewBag.Messag = "Roles Added Successfully";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string? RolName)
        {
            IdentityRole Rol;RoleFormVM R;
            if (RolName != null)
            {
                Rol = _roleManager.Roles.FirstOrDefault(a => a.Name == RolName);
                R = new RoleFormVM()
                {
                    Name = Rol.Name,
                    Id = Rol.Id
                };
            }
            else
                R = new RoleFormVM();
            return View(R);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RoleFormVM Rol)
        {
            IdentityRole exist = await _roleManager.FindByNameAsync(Rol.Name);
            
            if (!ModelState.IsValid)
                return View("Edit", Rol);
            try
            {

                if (exist == null)
                {
                    exist = await _roleManager.FindByIdAsync(Rol.Id);
                    exist.Name = Rol.Name;
                    await _roleManager.UpdateAsync(exist);
                    ViewBag.Messag = "Stage Updated Successfully";
                }

            }
            catch (Exception ex) { }
            return RedirectToAction("List");
        }
    }
}