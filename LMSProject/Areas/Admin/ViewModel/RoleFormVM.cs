using System.ComponentModel.DataAnnotations;

namespace LMSProject.Areas.Admin.ViewModel
{
    public class RoleFormVM
    {
        [Required, StringLength(256)]
        public string Name { get; set; }
        public string? Id {  get; set; }
    }
}
