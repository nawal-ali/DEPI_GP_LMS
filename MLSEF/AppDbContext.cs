using Microsoft.EntityFrameworkCore;
using MLSCore.Configuration;
using MLSCore.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MLSCore.IdentityModel;

namespace MLSEF
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Latin1_General_CI_AS");
            base.OnModelCreating(modelBuilder);   // ← called ONCE only

            new StageConfiguration().Configure(modelBuilder.Entity<TbStage>());
            new ChoiceConfiguration().Configure(modelBuilder.Entity<TbChoice>());
            new ContentConfiguration().Configure(modelBuilder.Entity<TbCourseContent>());
            new CourseConfiguration().Configure(modelBuilder.Entity<TbCourse>());
            new DiscountConfiguration().Configure(modelBuilder.Entity<TbCourseDiscount>());
            new GradeConfiguration().Configure(modelBuilder.Entity<TbGrade>());
            new InstructorConfiguration().Configure(modelBuilder.Entity<TbInstructor>());
            new MaterialConfiguration().Configure(modelBuilder.Entity<TbMterials>());
            new ReviewConfiguration().Configure(modelBuilder.Entity<TbCourseReview>());
            new SbContentConfiguration().Configure(modelBuilder.Entity<TbSubContent>());
            new StudentConfiguration().Configure(modelBuilder.Entity<TbStudent>());
            new SubjectConfiguration().Configure(modelBuilder.Entity<TbSubject>());
            new SubSubjectConfiguration().Configure(modelBuilder.Entity<TbSubSubject>());
            new TermConfiguration().Configure(modelBuilder.Entity<TbTerm>());
            new ParentConfiguration().Configure(modelBuilder.Entity<TbParent>());
            new AnnouncementConfiguration().Configure(modelBuilder.Entity<TbAnnouncement>());
            new CourseMaterialConfiguration().Configure(modelBuilder.Entity<TbCourseMaterial>());
            new AssignmentConfiguration().Configure(modelBuilder.Entity<TbAssignment>());
            new AssignmentSubmissionConfiguration().Configure(modelBuilder.Entity<TbAssignmentSubmission>());

            modelBuilder.Entity<IdentityPasskeyData>().HasNoKey();

            foreach (var foreignKey in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        // ── DbSets ─────────────────────────────────────────────────────────
        public DbSet<TbChoice> Choices { get; set; }
        public DbSet<TbCourse> Courses { get; set; }
        public DbSet<TbCourseContent> CourseContents { get; set; }
        public DbSet<TbCourseDiscount> CourseDiscounts { get; set; }
        public DbSet<TbCourseReview> CourseReviews { get; set; }
        public DbSet<TbGrade> Grades { get; set; }
        public DbSet<TbInstructor> Instructors { get; set; }
        public DbSet<TbMterials> Mterials { get; set; }
        public DbSet<TbSelectedChoice> SelectedChoices { get; set; }
        public DbSet<TbSessionAttend> SessionAttends { get; set; }
        public DbSet<TbStage> Stages { get; set; }
        public DbSet<TbStudent> Students { get; set; }
        public DbSet<TbStudentAnswer> StudentAnswers { get; set; }
        public DbSet<TbStudentCourse> StudentCourses { get; set; }
        public DbSet<TbStudentTest> StudentTests { get; set; }
        public DbSet<TbSubContent> SubContents { get; set; }
        public DbSet<TbSubject> Subjects { get; set; }
        public DbSet<TbSubSubject> SubSubjects { get; set; }
        public DbSet<TbTask> Tasks { get; set; }
        public DbSet<TbTaskAnswers> TaskAnswers { get; set; }
        public DbSet<TbTerm> Terms { get; set; }
        public DbSet<TbTest> Tests { get; set; }
        public DbSet<TbTestQuestion> TestsQuestion { get; set; }
        public DbSet<TbParent> Parents { get; set; }
        public DbSet<TbAnnouncement> Announcements { get; set; }
        public DbSet<TbCourseMaterial> CourseMaterials { get; set; }
        public DbSet<TbAssignment> Assignments { get; set; }
        public DbSet<TbAssignmentSubmission> AssignmentSubmissions { get; set; }
    }
}