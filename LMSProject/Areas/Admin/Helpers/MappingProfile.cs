using AutoMapper;
using LMSProject.Areas.Admin.ViewModel;
using LMSProject.Areas.Instructor.ViewModel;
using MLSCore.DTO;
using MLSCore.Models;

namespace LMSProject.Areas.Admin.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //------------------stage Mapping----------------------
            CreateMap<StageVM, TbStage>();
            CreateMap<StageEditVM, TbStage>();
            CreateMap<TbStage,StageEditVM>();
            //-----------------term Mapping------------------------
            CreateMap<TermVM, TbTerm>();
            CreateMap<TermEditVM, TbTerm>();
            CreateMap<TbTerm, TermEditVM>();
            //-----------------Grade Mapping------------------------
            CreateMap<GradeVM, TbGrade>();
            CreateMap<GradeEditVM, TbGrade>();
            //-----------------instructor-----------------------
            CreateMap<InstructorEditDTO,InstructorEditVM>();
            //-----------------sub Mapping------------------------
            CreateMap<SubjectVM, TbSubject>();
            CreateMap<SubjectEditVM, TbSubject>();
            CreateMap<TbSubject, SubjectEditVM>();
            CreateMap<SubSubjectVM, TbSubSubject>();
            CreateMap<SubSubjectEditVM, TbSubSubject>();
            CreateMap<TbSubSubject, SubSubjectEditVM>();
            //-----------------course Mapping------------------------
            CreateMap<CourseVM, TbCourse>();
            CreateMap<CourseEditVM, TbCourse>();
            CreateMap<TbCourse, CourseEditVM>();
            //-----------------course groups
            CreateMap<CourseGroupVM, TbCourseGroup>();
            CreateMap<CourseGroupEditVM, TbCourseGroup>();
            CreateMap<TbCourseGroup, CourseGroupEditVM>();


        }
    }
}

