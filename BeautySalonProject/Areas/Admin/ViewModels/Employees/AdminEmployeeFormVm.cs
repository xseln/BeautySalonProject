using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BeautySalonProject.Areas.Admin.ViewModels.Employees
{
    public class AdminEmployeeFormVm
    {
        public int? EmployeeId { get; set; }

        [Required, StringLength(60)]
        public string FirstName { get; set; } = "";

        [Required, StringLength(60)]
        public string LastName { get; set; } = "";

        [StringLength(60)]
        public string? JobTitle { get; set; }

        [StringLength(30)]
        public string? Phone { get; set; }

        [EmailAddress, StringLength(255)]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;

        public List<string> JobTitleOptions { get; set; } = new();
        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public int? PrimaryCategoryId { get; set; }
       
    }
}
