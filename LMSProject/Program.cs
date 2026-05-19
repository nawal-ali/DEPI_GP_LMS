using LMSProject.AI.Models;
using LMSProject.Areas.Admin.Helpers;
using LMSProject.Areas.SuperAdmin.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MLSCore;
using MLSCore.IdentityModel;
using MLSCore.Interfaces;
using MLSEF;
using MLSEF.Repositories;

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

builder.Services.AddSingleton<LMSProject.AI.Services.MongoDbService>();

// AI / GitHub Models HTTP client
builder.Services.AddHttpClient("GithubAI");
builder.Services.AddHttpClient("N8N");

// AI Services
builder.Services.AddScoped<LMSProject.AI.Services.GithubAiService>();
builder.Services.AddScoped<LMSProject.AI.Services.DocumentProcessingService>();
builder.Services.AddScoped<LMSProject.AI.Services.AiChatService>();
builder.Services.AddScoped<LMSProject.AI.Services.InstructorAiService>();

// Email
builder.Services.AddScoped<LMSProject.AI.Services.EmailService>();

// n8n + Reports
builder.Services.AddScoped<LMSProject.AI.Services.N8nService>();
builder.Services.AddScoped<LMSProject.AI.Services.WeeklyReportService>();



var app = builder.Build();

// Allow large file uploads (up to 50 MB) for AI document processing
app.Use(async (context, next) =>
{
    context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>()
        ?.MaxRequestBodySize = 52_428_800; // 50 MB
    await next();
});

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

app.MapControllers();   // ← required for [Route] attribute controllers like AiProxyController

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();