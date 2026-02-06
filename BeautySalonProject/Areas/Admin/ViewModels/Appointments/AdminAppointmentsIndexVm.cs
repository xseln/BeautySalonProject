namespace BeautySalonProject.Areas.Admin.ViewModels.Appointments
{
    public class AdminAppointmentsIndexVm
    {
        public DateTime Date { get; set; }
        public byte? StatusFilter { get; set; }
        public List<AdminAppointmentRowVm> Rows { get; set; } = new();
    }

    public class AdminAppointmentRowVm
    {
        public int AppointmentId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string EmployeeName { get; set; } = "";
        public string ServiceName { get; set; } = "";
        public string VariantName { get; set; } = "";
        public string ClientUserId { get; set; } = "";
        public byte Status { get; set; }
    }
}
