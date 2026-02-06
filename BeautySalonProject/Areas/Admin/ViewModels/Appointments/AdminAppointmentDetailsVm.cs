namespace BeautySalonProject.Areas.Admin.ViewModels.Appointments
{
    public class AdminAppointmentDetailsVm
    {
        public int AppointmentId { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";

        public string ServiceName { get; set; } = "";
        public string VariantName { get; set; } = "";
        public bool IsGuest { get; set; }
        public string ClientName { get; set; } = "";
        public string? ClientPhone { get; set; }
        public string? ClientEmail { get; set; }

        public string? Notes { get; set; }
        public byte Status { get; set; }
        public List<EmployeeOption> Employees { get; set; } = new();

        public class EmployeeOption
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
        public decimal FinalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? InquiryId { get; set; }
    }
}
