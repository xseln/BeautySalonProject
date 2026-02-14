using System.ComponentModel.DataAnnotations;

namespace BeautySalonProject.Areas.Admin.ViewModels.ServiceVariants
{
    public class AdminServiceVariantFormVm
    {
        public int? VariantId { get; set; }
        public int ServiceId { get; set; }

        public string ServiceName { get; set; } = "";
        public string CategoryName { get; set; } = "";

        [Required, StringLength(120)]
        public string VariantName { get; set; } = "";

        [Range(0, 99999)]
        public decimal Price { get; set; }

        [Range(1, 600)]
        public int DurationMinutes { get; set; } = 60;

        public bool IsActive { get; set; } = true;
    }
}
