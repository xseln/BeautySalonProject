namespace BeautySalonProject.Areas.Client.ViewModels.Appointments
{
    public class ClientMyAppointmentsVm
    {
        public List<ClientAppointmentRowVm> Upcoming { get; set; } = new();
        public List<ClientAppointmentRowVm> Past { get; set; } = new();

        public string? Q { get; set; }
        public byte? Status { get; set; }
        public int Tab { get; set; } = 0;
    }

    public class ClientAppointmentRowVm
    {
        public int AppointmentId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public string ServiceName { get; set; } = "";
        public string VariantName { get; set; } = "";
        public string EmployeeName { get; set; } = "";

        public decimal FinalPrice { get; set; }
        public byte Status { get; set; }
        public string StatusText { get; set; } = "";
        public bool CanCancel { get; set; }
    }
}
