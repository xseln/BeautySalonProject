namespace BeautySalonProject.Areas.Staff.ViewModels.Appointments
{
    public class StaffAppointmentsIndexVm
    {
        public DateTime Date { get; set; }
        public string? Q { get; set; }
        public byte? Status { get; set; }

        public List<Row> Rows { get; set; } = new();

        public class Row
        {
            public int AppointmentId { get; set; }
            public DateTime StartAt { get; set; }
            public DateTime EndAt { get; set; }

            public string ServiceName { get; set; } = "";
            public string VariantName { get; set; } = "";

            public string ClientName { get; set; } = "";
            public string? ClientPhone { get; set; }

            public byte Status { get; set; }
        }
    }
}
