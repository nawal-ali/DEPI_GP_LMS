using MLSCore.DTO;
using MLSCore.Models;
namespace MLSCore.Interfaces
{
    public interface IInstructorRepository:IBaseRepository<TbInstructor>
    {
        public Task<IEnumerable<InstructorCoursesDTO>> FindAllInstCourses();
        public Task<InstructorEditDTO> GetInstructorForEdit(int id);
    }
}
