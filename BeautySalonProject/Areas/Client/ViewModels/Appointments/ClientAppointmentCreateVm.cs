using System.ComponentModel.DataAnnotations;

namespace BeautySalonProject.Areas.Client.ViewModels.Appointments
{
    public class ClientAppointmentCreateVm
    {
        [Required]
        public int VariantId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        public string StartTime { get; set; } = "";

        [MaxLength(500)]
        public string? Notes { get; set; }
        public string VariantTitle { get; set; } = "";
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }

        public List<EmployeeOptionVm> Employees { get; set; } = new();
        public List<TimeSlotVm> Slots { get; set; } = new();
    }

}
