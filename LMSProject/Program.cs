using Microsoft.EntityFrameworkCore;
using MLSCore;
using MLSEF;
using Microsoft.AspNetCore.Identity;
using MLSCore.IdentityModel;
using LMSProject.Areas.Admin.Helpers;
using MLSCore.Interfaces;
using MLSEF.Repositories;
using LMSProject.Areas.SuperAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("conString"))
);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<IInstructorRepository, InstructorRepository>();
builder.Services.AddScoped<SuperAdminDataService>();
builder.Services.AddScoped<LMSProject.Services.TicketService>();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

var app = builder.Build();

// ── Seed roles + accounts ──────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Seed all roles
    string[] roles = { "Admin", "Instructor", "Student", "Parent", "SuperAdmin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed SuperAdmin account
    const string superAdminEmail = "superadmin@totc.edu";
    const string superAdminPass = "SuperAdmin@123";
    var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
    if (superAdmin == null)
    {
        var saUser = new ApplicationUser
        {
            UserName = superAdminEmail,
            Email = superAdminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(saUser, superAdminPass);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(saUser, "SuperAdmin");
    }

    // Seed Admin account
    const string adminEmail = "admin@totc.edu";
    const string adminPass = "Admin@1234";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Admin",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(newAdmin, adminPass);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(newAdmin, "Admin");
    }
}

// ── HTTP pipeline ──────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();