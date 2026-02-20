using System.ComponentModel.DataAnnotations;

namespace BeautySalonProject.Areas.Admin.ViewModels.Employees
{
    public class AdminCreateEmployeeAccountVm
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = "";

        [Required]
        public string UserName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public string? TempPassword { get; set; }
    }
}
