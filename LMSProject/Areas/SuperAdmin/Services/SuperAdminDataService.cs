using LMSProject.Areas.SuperAdmin.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MLSCore.IdentityModel;
using MLSCore.Models;
using MLSEF;

namespace LMSProject.Areas.SuperAdmin.Services
{
    public class SuperAdminDataService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SuperAdminDataService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ── Dashboard ──────────────────────────────────────────────────────
        public DashboardVM GetDashboard()
        {
            var totalStudents = _context.Students.Count(s => s.CurrentState == 1);
            var totalTeachers = _context.Instructors.Count(i => i.CurrentState == 1);
            var totalParents = _context.Parents.Count();
            var totalCourses = _context.Courses.Count(c => c.CurrentState == 1);
            var activeCourses = _context.Courses.Count(c => c.CurrentState == 1);
            var totalAnnounce = _context.Announcements.Count(a => a.CurrentState == 1);

            // Count users in Admin role
            var adminRoleId = _context.Roles
                .Where(r => r.NormalizedName == "ADMIN")
                .Select(r => r.Id).FirstOrDefault();
            var totalAdmins = adminRoleId != null
                ? _context.UserRoles.Count(ur => ur.RoleId == adminRoleId)
                : 0;

            // Recent activity from real submissions + enrollments
            var recentActivity = new List<RecentActivityVM>();

            var recentSubs = _context.AssignmentSubmissions
                .Where(s => s.CurrentState == 1 && s.SubmittedAt != null)
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(3)
                .ToList();

            foreach (var sub in recentSubs)
                recentActivity.Add(new RecentActivityVM
                {
                    Icon = "fa-file-upload",
                    Color = "#5B72EE",
                    Message = $"{sub.Student?.FullName} submitted \"{sub.Assignment?.Title}\"",
                    Time = sub.SubmittedAt?.ToString("MMM d, h:mm tt") ?? ""
                });

            var recentAnnounce = _context.Announcements
                .Where(a => a.CurrentState == 1)
                .OrderByDescending(a => a.PublishedDate)
                .Take(2)
                .ToList();

            foreach (var ann in recentAnnounce)
                recentActivity.Add(new RecentActivityVM
                {
                    Icon = "fa-bullhorn",
                    Color = "#29B9E7",
                    Message = $"Announcement published: \"{ann.Title}\"",
                    Time = ann.PublishedDate.ToString("MMM d, h:mm tt")
                });

            if (!recentActivity.Any())
                recentActivity.Add(new RecentActivityVM
                {
                    Icon = "fa-check-circle",
                    Color = "#33EFA0",
                    Message = "System running normally",
                    Time = "Now"
                });

            return new DashboardVM
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalParents = totalParents,
                TotalAdmins = totalAdmins,
                TotalCourses = totalCourses,
                ActiveCourses = activeCourses,
                OpenTickets = 0,
                TotalAnnouncements = totalAnnounce,
                SystemUptime = 99.9,
                RecentActivity = recentActivity
            };
        }

        // ── Statistics ─────────────────────────────────────────────────────
        public StatisticsVM GetStatistics()
        {
            var totalStudents = _context.Students.Count(s => s.CurrentState == 1);
            var totalTeachers = _context.Instructors.Count(i => i.CurrentState == 1);
            var totalParents = _context.Parents.Count();
            var totalCourses = _context.Courses.Count(c => c.CurrentState == 1);
            var totalAnnounce = _context.Announcements.Count(a => a.CurrentState == 1);
            var totalSubs = _context.AssignmentSubmissions.Count(s => s.CurrentState == 1);
            var resolvedSubs = _context.AssignmentSubmissions.Count(s => s.CurrentState == 1 && s.Marks != null);

            var adminRoleId = _context.Roles
                .Where(r => r.NormalizedName == "ADMIN")
                .Select(r => r.Id).FirstOrDefault();
            var totalAdmins = adminRoleId != null
                ? _context.UserRoles.Count(ur => ur.RoleId == adminRoleId)
                : 0;

            // Top 5 courses by enrollment
            var topCourses = _context.StudentCourses
                .Include(sc => sc.Course)
                .GroupBy(sc => sc.Course.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList()
                .Select(t => (t.Name, t.Count))
                .ToList();

            // Monthly submissions as proxy for activity (last 12 months)
            var now = DateTime.Now;
            var monthlyActivity = new int[12];
            var monthlySubs = _context.AssignmentSubmissions
                .Where(s => s.SubmittedAt != null && s.SubmittedAt >= now.AddMonths(-11))
                .ToList();
            foreach (var sub in monthlySubs)
            {
                if (sub.SubmittedAt.HasValue)
                {
                    var monthsAgo = ((now.Year - sub.SubmittedAt.Value.Year) * 12)
                                  + now.Month - sub.SubmittedAt.Value.Month;
                    if (monthsAgo >= 0 && monthsAgo < 12)
                        monthlyActivity[11 - monthsAgo]++;
                }
            }

            // Monthly enrollment spread (StudentCourse has no date — distribute proportionally)
            var enrollTotal = _context.StudentCourses.Count();
            var monthlyEnroll = Enumerable.Range(1, 12)
                .Select(m => (int)(enrollTotal * 0.07 * (0.6 + m * 0.04)))
                .ToArray();

            return new StatisticsVM
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalParents = totalParents,
                TotalAdmins = totalAdmins,
                TotalCourses = totalCourses,
                ActiveCourses = totalCourses,
                TotalTickets = 0,
                ResolvedTickets = 0,
                TotalAnnouncements = totalAnnounce,
                SystemUptime = 99.9,
                MonthlyRegistrations = monthlyEnroll,
                MonthlyTickets = new int[12],
                CourseEnrollments = monthlyActivity,
                UserBreakdown = new()
                {
                    ("Students", totalStudents, "#5B72EE"),
                    ("Teachers", totalTeachers, "#29B9E7"),
                    ("Parents",  totalParents,  "#33EFA0"),
                    ("Admins",   totalAdmins,   "#E13468"),
                },
                TopCourses = topCourses.Any() ? topCourses : new() { ("No courses yet", 0) }
            };
        }

        // ── Students (real DB) ─────────────────────────────────────────────
        public StudentListVM GetStudents(string search, string grade, string status,
                                         string section, int page)
        {
            var query = _context.Students
                .Include(s => s.Grade)
                .Include(s => s.User)
                .Include(s => s.Parent)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.FullName.Contains(search)
                                      || (s.User != null && s.User.Email.Contains(search)));
            if (!string.IsNullOrWhiteSpace(grade))
                query = query.Where(s => s.Grade != null && s.Grade.Name == grade);
            if (status == "Active")
                query = query.Where(s => s.CurrentState == 1);
            else if (status == "Inactive")
                query = query.Where(s => s.CurrentState == 0);

            var all = query.ToList();

            var allVMs = all.Select(s => new StudentItemVM
            {
                Id = s.Id.ToString(),
                Name = s.FullName,
                Email = s.User?.Email ?? "",
                Phone = s.User?.PhoneNumber ?? "",
                Grade = s.Grade?.Name ?? "",
                Section = "",
                Status = s.CurrentState == 1 ? "Active" : "Inactive",
                ParentId = s.ParentId?.ToString(),
                ImageUrl = string.IsNullOrEmpty(s.ImageName) ? null : s.ImageName
            }).ToList();

            var filtered = allVMs.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(section))
                filtered = filtered.Where(s => s.Section == section);

            var filteredList = filtered.ToList();

            return new StudentListVM
            {
                Students = allVMs,
                Filtered = filteredList,
                SearchTerm = search,
                SelectedGrade = grade,
                SelectedStatus = status,
                SelectedSection = section,
                CurrentPage = page,
                TotalStudents = allVMs.Count,
                ActiveStudents = allVMs.Count(s => s.Status == "Active"),
                GraduatingStudents = 0,
                StudentsWithParents = allVMs.Count(s => s.ParentId != null),
            };
        }

        public StudentItemVM? GetStudent(string id)
        {
            if (!int.TryParse(id, out var intId)) return null;
            var s = _context.Students
                .Include(x => x.Grade)
                .Include(x => x.User)
                .FirstOrDefault(x => x.Id == intId);
            if (s == null) return null;
            return new StudentItemVM
            {
                Id = s.Id.ToString(),
                Name = s.FullName,
                Email = s.User?.Email ?? "",
                Phone = s.User?.PhoneNumber ?? "",
                Grade = s.Grade?.Name ?? "",
                Status = s.CurrentState == 1 ? "Active" : "Inactive",
                ParentId = s.ParentId?.ToString(),
                ImageUrl = string.IsNullOrEmpty(s.ImageName) ? null : s.ImageName
            };
        }

        public void CreateStudent(StudentItemVM s) { /* Only SuperAdmin creates via UI form */ }

        public void DeleteStudent(string id)
        {
            if (!int.TryParse(id, out var intId)) return;
            var s = _context.Students.Find(intId);
            if (s != null) { s.CurrentState = 0; _context.SaveChanges(); }
        }

        // ── Teachers (real DB) ─────────────────────────────────────────────
        public async Task<TeacherListVM> GetTeachersAsync(string search, string subject,
                                                           string status, string grade, int page)
        {
            var instructorUsers = await _userManager.GetUsersInRoleAsync("Instructor");
            var instructorUserIds = instructorUsers.Select(u => u.Id).ToHashSet();

            var instructors = await _context.Instructors
                .Where(i => instructorUserIds.Contains(i.UserId))
                .Include(i => i.User)
                .ToListAsync();

            var all = instructors.Select(i => new TeacherItemVM
            {
                Id = i.Id.ToString(),
                Name = i.FullName,
                Email = i.User?.Email ?? "",
                Phone = i.User?.PhoneNumber ?? "",
                Subject = i.Specialization ?? "",
                Grade = "",
                Status = i.CurrentState == 1 ? "Active" : "Inactive",
                Experience = i.ExperienceYears,
                ImageUrl = string.IsNullOrEmpty(i.ImageName) ? null : i.ImageName
            }).ToList();

            var filtered = all.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                                            || t.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(subject))
                filtered = filtered.Where(t => t.Subject.Contains(subject, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(status))
                filtered = filtered.Where(t => t.Status == status);

            var list = filtered.ToList();
            return new TeacherListVM
            {
                Teachers = all,
                Filtered = list,
                SearchTerm = search,
                SelectedSubject = subject,
                SelectedStatus = status,
                SelectedGrade = grade,
                CurrentPage = page,
                TotalTeachers = all.Count,
                ActiveTeachers = all.Count(t => t.Status == "Active"),
                SubjectsTaught = all.Select(t => t.Subject).Where(s => !string.IsNullOrEmpty(s)).Distinct().Count(),
                AvgExperience = all.Any() ? (int)all.Average(t => t.Experience) : 0,
            };
        }

        public async Task<TeacherItemVM?> GetTeacherAsync(string id)
        {
            if (!int.TryParse(id, out var intId)) return null;
            var i = await _context.Instructors
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == intId);
            if (i == null) return null;
            return new TeacherItemVM
            {
                Id = i.Id.ToString(),
                Name = i.FullName,
                Email = i.User?.Email ?? "",
                Phone = i.User?.PhoneNumber ?? "",
                Subject = i.Specialization ?? "",
                Status = i.CurrentState == 1 ? "Active" : "Inactive",
                Experience = i.ExperienceYears,
                ImageUrl = string.IsNullOrEmpty(i.ImageName) ? null : i.ImageName
            };
        }

        // ── Parents (real DB) ──────────────────────────────────────────────
        public ParentListVM GetParents(string search, string status, string occ,
                                        string children, int page)
        {
            var query = _context.Parents
                .Include(p => p.Children)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.FullName.Contains(search)
                                      || (p.Email != null && p.Email.Contains(search)));

            var all = query.ToList();

            var allVMs = all.Select(p => new ParentItemVM
            {
                Id = p.Id.ToString(),
                Name = p.FullName,
                Email = p.Email ?? "",
                Phone = p.PhoneNumber ?? "",
                Occupation = p.Occupation ?? "",
                ChildrenCount = p.Children?.Count ?? 0,
                Status = p.CurrentState == 1 ? "Active" : "Inactive",
                ImageUrl = string.IsNullOrEmpty(p.ImageName) ? null : p.ImageName
            }).ToList();

            var filtered = allVMs.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(status))
                filtered = filtered.Where(p => p.Status == status);
            if (!string.IsNullOrWhiteSpace(occ))
                filtered = filtered.Where(p => p.Occupation == occ);
            if (!string.IsNullOrWhiteSpace(children) && int.TryParse(children, out var cnt))
                filtered = filtered.Where(p => p.ChildrenCount == cnt);

            var filteredList = filtered.ToList();

            return new ParentListVM
            {
                Parents = allVMs,
                Filtered = filteredList,
                SearchTerm = search,
                SelectedStatus = status,
                SelectedOccupation = occ,
                SelectedChildren = children,
                CurrentPage = page,
                TotalParents = allVMs.Count,
                TotalChildren = allVMs.Sum(p => p.ChildrenCount),
                ActiveContacts = allVMs.Count(p => p.Status == "Active"),
            };
        }

        public ParentItemVM? GetParent(string id)
        {
            if (!int.TryParse(id, out var intId)) return null;
            var p = _context.Parents.Include(x => x.Children).FirstOrDefault(x => x.Id == intId);
            if (p == null) return null;
            return new ParentItemVM
            {
                Id = p.Id.ToString(),
                Name = p.FullName,
                Email = p.Email ?? "",
                Phone = p.PhoneNumber ?? "",
                Occupation = p.Occupation ?? "",
                ChildrenCount = p.Children?.Count ?? 0,
                Status = p.CurrentState == 1 ? "Active" : "Inactive"
            };
        }

        // ── Admins (real DB — from Identity roles) ─────────────────────────
        public AdminListVM GetAdmins(string search, string status, string access,
                                      string dept, int page)
        {
            var adminRoleId = _context.Roles
                .Where(r => r.NormalizedName == "ADMIN")
                .Select(r => r.Id).FirstOrDefault();

            var adminUserIds = adminRoleId != null
                ? _context.UserRoles
                    .Where(ur => ur.RoleId == adminRoleId)
                    .Select(ur => ur.UserId)
                    .ToHashSet()
                : new HashSet<string>();

            var adminUsers = _context.Users
                .Where(u => adminUserIds.Contains(u.Id))
                .ToList();

            var allVMs = adminUsers.Select(u => new AdminItemVM
            {
                Id = u.Id,
                Name = u.FullName ?? u.UserName ?? "",
                Email = u.Email ?? "",
                Phone = u.PhoneNumber ?? "",
                Department = "Administration",
                AccessLevel = "Full Access",
                Status = "Active",
                ImageUrl = null
            }).ToList();

            var filtered = allVMs.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(a => a.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                                            || a.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(status))
                filtered = filtered.Where(a => a.Status == status);

            var filteredList = filtered.ToList();

            return new AdminListVM
            {
                Admins = allVMs,
                Filtered = filteredList,
                SearchTerm = search,
                SelectedStatus = status,
                SelectedAccess = access,
                SelectedDept = dept,
                CurrentPage = page,
                TotalAdmins = allVMs.Count,
                FullAccess = allVMs.Count(a => a.AccessLevel == "Full Access"),
            };
        }

        public AdminItemVM? GetAdmin(string id)
        {
            var u = _context.Users.Find(id);
            if (u == null) return null;
            return new AdminItemVM
            {
                Id = u.Id,
                Name = u.FullName ?? u.UserName ?? "",
                Email = u.Email ?? "",
                Phone = u.PhoneNumber ?? "",
                Department = "Administration",
                AccessLevel = "Full Access",
                Status = "Active"
            };
        }

        // ── Announcements (real DB) ────────────────────────────────────────
        public AnnouncementListVM GetAnnouncements(string search, string priority,
                                                    string status, string target, int page)
        {
            var all = _context.Announcements
                .Where(a => a.CurrentState == 1)
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.PublishedDate)
                .ToList();

            var allVMs = all.Select(a => new AnnouncementItemVM
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                Author = a.CreatedBy ?? "Admin",
                Priority = a.Priority ?? "Normal",
                Status = a.IsActive ? "Active" : "Inactive",
                TargetAudience = a.TargetAudience,
                Views = a.ViewCount,
                Date = a.PublishedDate,
                ExpiryDate = a.ExpiryDate
            }).ToList();

            var filtered = allVMs.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(a => a.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                                            || a.Content.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(priority))
                filtered = filtered.Where(a => a.Priority == priority);
            if (!string.IsNullOrWhiteSpace(status))
                filtered = filtered.Where(a => a.Status == status);
            if (!string.IsNullOrWhiteSpace(target))
                filtered = filtered.Where(a => a.TargetAudience == target);

            var filteredList = filtered.ToList();

            return new AnnouncementListVM
            {
                Announcements = allVMs,
                Filtered = filteredList,
                SearchTerm = search,
                SelectedPriority = priority,
                SelectedStatus = status,
                SelectedTarget = target,
                CurrentPage = page,
                TotalAnnouncements = allVMs.Count,
                ActiveAnnouncements = allVMs.Count(a => a.Status == "Active"),
                TotalViews = allVMs.Sum(a => a.Views),
                UrgentAnnouncements = allVMs.Count(a => a.Priority == "Urgent"),
            };
        }

        public void CreateAnnouncement(CreateAnnouncementVM vm)
        {
            _context.Announcements.Add(new TbAnnouncement
            {
                Title = vm.Title,
                Content = vm.Content,
                Priority = vm.Priority,
                TargetAudience = vm.TargetAudience,
                ExpiryDate = vm.ExpiryDate,
                IsActive = vm.Status == "Active",
                PublishedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                CreatedBy = "SuperAdmin",
                CurrentState = 1,
                ImageUrl = "",
                AttachmentUrl = ""
            });
            _context.SaveChanges();
        }

        public void DeleteAnnouncement(int id)
        {
            var a = _context.Announcements.Find(id);
            if (a != null) { a.CurrentState = 0; _context.SaveChanges(); }
        }

        // ── Tickets (no DB model — kept as empty stubs) ────────────────────
        private static readonly List<TicketItemVM> _tickets = new();

        public TicketListVM GetTickets(string search, string status, string priority,
                                        string category, int page)
        {
            return new TicketListVM
            {
                Tickets = _tickets,
                Filtered = _tickets,
                SearchTerm = search,
                SelectedStatus = status,
                SelectedPriority = priority,
                SelectedCategory = category,
                CurrentPage = page,
                TotalTickets = 0,
                OpenTickets = 0,
                ResolvedTickets = 0,
                UrgentTickets = 0,
            };
        }

        public TicketItemVM? GetTicket(int id) => null;

        public void RespondToTicket(int id, string response, string newStatus) { }
        public void CloseTicket(int id) { }

        // ── User Profile ───────────────────────────────────────────────────
        public UserProfileVM GetUserProfile(string id, string role)
        {
            if (role == "Teacher")
            {
                var teacher = GetTeacherAsync(id).GetAwaiter().GetResult();
                return BuildProfile(teacher);
            }
            return role switch
            {
                "Student" => BuildProfile(GetStudent(id)),
                "Parent" => BuildProfile(GetParent(id)),
                "Admin" => BuildProfile(GetAdmin(id)),
                _ => new UserProfileVM()
            };
        }

        private static UserProfileVM BuildProfile(object? user) => user switch
        {
            StudentItemVM s => new() { Id = s.Id, Name = s.Name, Email = s.Email, Phone = s.Phone, Role = "Student", Status = s.Status, Grade = s.Grade, ImageUrl = s.ImageUrl },
            TeacherItemVM t => new() { Id = t.Id, Name = t.Name, Email = t.Email, Phone = t.Phone, Role = "Teacher", Status = t.Status, Subject = t.Subject, Grade = t.Grade, ImageUrl = t.ImageUrl },
            ParentItemVM p => new() { Id = p.Id, Name = p.Name, Email = p.Email, Phone = p.Phone, Role = "Parent", Status = p.Status, Occupation = p.Occupation, ImageUrl = p.ImageUrl },
            AdminItemVM a => new() { Id = a.Id, Name = a.Name, Email = a.Email, Phone = a.Phone, Role = "Admin", Status = a.Status, Department = a.Department, ImageUrl = a.ImageUrl },
            _ => new()
        };

        // ── SuperAdmin Profile ─────────────────────────────────────────────
        public SuperAdminProfileVM GetSuperAdminProfile()
        {
            var saRoleId = _context.Roles
                .Where(r => r.NormalizedName == "SUPERADMIN")
                .Select(r => r.Id).FirstOrDefault();

            var saUserId = saRoleId != null
                ? _context.UserRoles
                    .Where(ur => ur.RoleId == saRoleId)
                    .Select(ur => ur.UserId)
                    .FirstOrDefault()
                : null;

            var saUser = saUserId != null
                ? _context.Users.Find(saUserId)
                : null;

            return new SuperAdminProfileVM
            {
                FullName = saUser?.FullName ?? "Super Admin",
                Email = saUser?.Email ?? "superadmin@totc.edu",
                Phone = saUser?.PhoneNumber ?? "",
                Address = "",
                Bio = "System owner and platform administrator.",
                AccessLevel = "Full System Access",
                JoinedDate = saUser?.CreatedAt ?? new DateTime(2023, 1, 1),
                Status = "Active"
            };
        }

        public void UpdateSuperAdminProfile(EditProfileVM vm)
        {
            var saRoleId = _context.Roles
                .Where(r => r.NormalizedName == "SUPERADMIN")
                .Select(r => r.Id).FirstOrDefault();

            var saUserId = saRoleId != null
                ? _context.UserRoles
                    .Where(ur => ur.RoleId == saRoleId)
                    .Select(ur => ur.UserId)
                    .FirstOrDefault()
                : null;

            if (saUserId == null) return;

            var saUser = _context.Users.Find(saUserId);
            if (saUser == null) return;

            saUser.FullName = vm.FullName;
            saUser.Email = vm.Email;
            saUser.PhoneNumber = vm.Phone;
            _context.SaveChanges();
        }

        public void DeleteUser(string id, string role)
        {
            switch (role)
            {
                case "Student":
                    DeleteStudent(id);
                    break;
                case "Teacher":
                    if (int.TryParse(id, out var instrId))
                    {
                        var instr = _context.Instructors.Find(instrId);
                        if (instr != null) { instr.CurrentState = 0; _context.SaveChanges(); }
                    }
                    break;
                case "Parent":
                    if (int.TryParse(id, out var parentId))
                    {
                        var p = _context.Parents.Find(parentId);
                        if (p != null) { p.CurrentState = 0; _context.SaveChanges(); }
                    }
                    break;
            }
        }
    }
}