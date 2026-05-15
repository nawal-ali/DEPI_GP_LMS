using Microsoft.EntityFrameworkCore;
using MLSCore.DTO;
using MLSCore.Interfaces;
using MLSCore.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MLSEF.Repositories
{
    public class InstructorRepository:BaseRepository<TbInstructor>,IInstructorRepository
    {
        protected AppDbContext _context { get; set; }
        public InstructorRepository(AppDbContext context):base(context)
        {
            _context = context;
            
        }
        public async Task<IEnumerable<InstructorCoursesDTO>> FindAllInstCourses()
        {
            var Instructors = await _context.Instructors.Where(x=>x.CurrentState==1)
    .Select(i => new InstructorCoursesDTO
    {
        Id = i.Id,
        FullName = i.FullName,
        Image = i.ImageName,
        Specialization = i.Specialization,
        CurrentState = i.CurrentState,
        Courses = i.Courses.Count(x => x.CurrentState == 1)
    })
    .ToListAsync();
            return Instructors;
        }
        public async Task<InstructorEditDTO> GetInstructorForEdit(int id)
        {
            var instructor = await _context.Instructors
                    .Where(i => i.Id == id)
                    .Select(i => new InstructorEditDTO
                    {Id=i.Id,
                    FullName=i.FullName,
                    UserName=i.User.UserName,
                    Email=i.User.Email,
                    PhoneNumber=i.User.PhoneNumber,
                    Password="",
                    Bio=i.Bio,
                    Specialization=i.Specialization,
                    ExperienceYears=i.ExperienceYears,
                    ImageName=i.ImageName

                    }).FirstOrDefaultAsync();
         return instructor;       
        }
    }
}
