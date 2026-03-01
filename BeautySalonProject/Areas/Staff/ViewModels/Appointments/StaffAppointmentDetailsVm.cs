namespace BeautySalonProject.Areas.Staff.ViewModels.Appointments
{
    public class StaffAppointmentDetailsVm
    {
        public int AppointmentId { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public string ServiceName { get; set; } = "";
        public string VariantName { get; set; } = "";

        public byte Status { get; set; }
        public decimal FinalPrice { get; set; }
        public string? Notes { get; set; }

        public bool IsGuest { get; set; }
        public string ClientName { get; set; } = "";
        public string? ClientPhone { get; set; }
        public string? ClientEmail { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
