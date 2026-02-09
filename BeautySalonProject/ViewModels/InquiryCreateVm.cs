using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BeautySalonProject.ViewModels
{
    public class InquiryCreateVm
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; } = "";

        [Required, StringLength(50)]
        public string LastName { get; set; } = "";

        [Required, Phone]
        public string Phone { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public int ServiceVariantId { get; set; }

        [Required]
        public DateTime? PreferredDateTime { get; set; } = DateTime.Today.AddDays(1).AddHours(10);

        [StringLength(500)]
        public string? Note { get; set; }
        public int CategoryId { get; set; }
        public int? VariantId { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> ServiceVariants { get; set; } = new();
        public List<SelectListItem> ServiceVariant { get; set; } = new();
    }
}
