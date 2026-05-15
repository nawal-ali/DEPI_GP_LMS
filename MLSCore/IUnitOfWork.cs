using MLSCore.Interfaces;
using MLSCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLSCore
{
    public interface IUnitOfWork : IDisposable
    {
        //----------------add refrence to each repository
       public   IBaseRepository<TbChoice> Choices { get; }
        public   IBaseRepository<TbCourse> Courses { get; }
        public   IBaseRepository<TbCourseContent> Contents { get; }
        public    IBaseRepository<TbCourseDiscount> Discounts { get; }
        public  IBaseRepository<TbCourseReview> Reviews { get; }
        public  IBaseRepository<TbGrade> Grades { get; }
        public  IInstructorRepository Instructors { get; }
        public  IBaseRepository<TbMterials> Materials { get; }
        public  IBaseRepository<TbSelectedChoice> SelectedChoices { get; }
        public  IBaseRepository<TbSessionAttend> Attendance { get; }
        public  IBaseRepository<TbStage> Stages { get; }
        public  IBaseRepository<TbStudent> Students { get; }
        public  IBaseRepository<TbStudentAnswer> StudentAnswers { get; }
        public  IBaseRepository<TbStudentCourse> StudentCourses { get; }
        public  IBaseRepository<TbStudentTest> StudentTests { get; }
        public  IBaseRepository<TbSubContent> SubContents { get; }
        public  IBaseRepository<TbSubject> Subjects { get; }
        public  IBaseRepository<TbSubSubject> SubSubjects { get; }
        public  IBaseRepository<TbTask> Tasks { get; }
        public  IBaseRepository<TbTaskAnswers> TaskAnswers { get; }
        public  IBaseRepository<TbTerm> Terms { get; }
        public  IBaseRepository<TbTest> Tests { get; }
        public  IBaseRepository<TbTestQuestion> TestQuestions { get; }
        public IBaseRepository<TbCourseGroup> CourseGroups { get; }
        public IBaseRepository<TbGroupSchedule> GroupSchedules { get; }
        public IBaseRepository<TbCourseMaterial> CourseMaterials { get; }
        public IBaseRepository<TbAssignment> Assignments { get; }
        public IBaseRepository<TbAssignmentSubmission> AssignmentSubmissions { get; }

        int Complete();
    }
}