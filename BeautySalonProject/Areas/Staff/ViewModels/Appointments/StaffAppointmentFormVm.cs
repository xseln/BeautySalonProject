using Microsoft.AspNetCore.Mvc.Rendering;

namespace BeautySalonProject.Areas.Staff.ViewModels.Appointments
{
    public class StaffAppointmentFormVm
    {
        public int? AppointmentId { get; set; }
        public int VariantId { get; set; }
        public DateTime StartAt { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
        public string? GuestFullName { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }
        public List<SelectListItem> VariantOptions { get; set; } = new();
        public int DurationMinutes { get; set; }
        public decimal VariantPrice { get; set; }
    }
}
