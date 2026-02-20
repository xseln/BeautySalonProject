namespace BeautySalonProject.Areas.Admin.ViewModels.Employees
{
    public class AdminEmployeeDetailsVm
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = "";
        public string? JobTitle { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public string? IdentityUserId { get; set; }
        public string? AccountEmail { get; set; }
        public string? AccountUserName { get; set; }

        public List<string> Services { get; set; } = new();

        public List<UpcomingRow> UpcomingAppointments { get; set; } = new();

        public class UpcomingRow
        {
            public int AppointmentId { get; set; }
            public DateTime StartAt { get; set; }
            public DateTime EndAt { get; set; }
            public string ServiceName { get; set; } = "";
            public string VariantName { get; set; } = "";
            public string ClientName { get; set; } = "";
            public string? ClientPhone { get; set; }
        }
    }
}
