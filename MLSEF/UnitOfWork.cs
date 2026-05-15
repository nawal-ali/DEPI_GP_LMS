using MLSCore;
using MLSCore.Interfaces;
using MLSCore.Models;
using MLSEF.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLSEF
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
         public  IBaseRepository<TbChoice> Choices { get; private set; }
         public  IBaseRepository<TbCourse> Courses { get; private set; }
         public  IBaseRepository<TbCourseContent> Contents { get; private set; }
         public  IBaseRepository<TbCourseDiscount> Discounts { get; private set; }
         public  IBaseRepository<TbCourseReview> Reviews { get; private set; }
         public  IBaseRepository<TbGrade> Grades { get; private set; }
         public  IInstructorRepository Instructors { get; private set; }
         public  IBaseRepository<TbMterials> Materials { get; private set; }
         public  IBaseRepository<TbSelectedChoice> SelectedChoices { get; private set; }
         public  IBaseRepository<TbSessionAttend> Attendance { get; private set; }
         public  IBaseRepository<TbStage> Stages { get; private set; }
         public  IBaseRepository<TbStudent> Students { get; private set; }
         public  IBaseRepository<TbStudentAnswer> StudentAnswers { get; private set; }
         public  IBaseRepository<TbStudentCourse> StudentCourses { get; private set; }
         public  IBaseRepository<TbStudentTest> StudentTests { get; private set; }
         public  IBaseRepository<TbSubContent> SubContents { get; private set; }
         public  IBaseRepository<TbSubject> Subjects { get; private set; }
         public  IBaseRepository<TbSubSubject> SubSubjects { get; private set; }
         public  IBaseRepository<TbTask> Tasks { get; private set; }
         public  IBaseRepository<TbTaskAnswers> TaskAnswers { get; private set; }
         public  IBaseRepository<TbTerm> Terms { get; private set; }
         public  IBaseRepository<TbTest> Tests { get; private set; }
         public  IBaseRepository<TbTestQuestion> TestQuestions { get; private set; }
        public IBaseRepository<TbCourseGroup> CourseGroups { get; private set; }
        public IBaseRepository<TbGroupSchedule> GroupSchedules { get; private set; }
        public IBaseRepository<TbCourseMaterial> CourseMaterials { get; private set; }
        public IBaseRepository<TbAssignment> Assignments { get; private set; }
        public IBaseRepository<TbAssignmentSubmission> AssignmentSubmissions { get; private set; }
        //       public IBookRepository Books { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Choices = new BaseRepository<TbChoice>(_context);
            Courses = new BaseRepository<TbCourse>(_context);
            Contents = new BaseRepository<TbCourseContent>(_context);
            Discounts = new BaseRepository<TbCourseDiscount>(_context);
            Reviews = new BaseRepository<TbCourseReview>(_context);
            Grades = new BaseRepository<TbGrade>(_context);
            Instructors = new InstructorRepository(_context);
            Materials = new BaseRepository<TbMterials>(_context);
            SelectedChoices = new BaseRepository<TbSelectedChoice>(_context);
            Attendance = new BaseRepository<TbSessionAttend>(_context);
            Stages = new BaseRepository<TbStage>(_context);
            Students = new BaseRepository<TbStudent>(_context);
            StudentAnswers = new BaseRepository<TbStudentAnswer>(_context);
            StudentCourses = new BaseRepository<TbStudentCourse>(_context);
            StudentTests = new BaseRepository<TbStudentTest>(_context);
            SubContents = new BaseRepository<TbSubContent>(_context);
            Subjects = new BaseRepository<TbSubject>(_context);
            SubSubjects = new BaseRepository<TbSubSubject>(_context);
            Tasks = new BaseRepository<TbTask>(_context);
            TaskAnswers = new BaseRepository<TbTaskAnswers>(_context);
            Terms = new BaseRepository<TbTerm>(_context);
            Tests = new BaseRepository<TbTest>(_context);
            TestQuestions = new BaseRepository<TbTestQuestion>(_context);
            CourseGroups = new BaseRepository<TbCourseGroup>(_context);
            GroupSchedules = new BaseRepository<TbGroupSchedule>(_context);
            CourseMaterials = new BaseRepository<TbCourseMaterial>(_context);
            Assignments = new BaseRepository<TbAssignment>(_context);
            AssignmentSubmissions = new BaseRepository<TbAssignmentSubmission>(_context);
            //       Authors = new BaseRepository<Author>(_context);
            // Books = new BaseRepository<Book>(_context);
            //        Books = new BookRepository(_context);

        }
        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
