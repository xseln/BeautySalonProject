using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BeautySalonProject.Areas.Admin.ViewModels.Services
{
    public class AdminServiceFormVm
    {
        public int? ServiceId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = "";

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
		
        [ValidateNever]
        public List<SelectListItem> Categories { get; set; } = new();
        [ValidateNever]
        public List<SelectListItem> Employees { get; set; } = new();
    }
}
