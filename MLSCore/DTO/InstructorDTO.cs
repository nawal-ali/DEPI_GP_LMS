using System;
using System.Collections.Generic;
using System.Text;

namespace MLSCore.DTO
{
    public class InstructorDTO
    {
    }
    public class InstructorCoursesDTO
    {
        public int Id {  get; set; }
        public string FullName { get; set; }
        public string Image { get; set; }
        public string Specialization { set; get; }
        public int CurrentState { get; set; }
        public int Courses { get; set; }

    }
    public class InstructorEditDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public bool ShowInHomePage { get; set; }
        public string? ImageName { get; set; } = null!;
    }
}
