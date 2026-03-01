namespace BeautySalonProject.Areas.Staff.ViewModels.Clients
{
    public class StaffClientHistoryVm
    {
        public string? Q { get; set; }
        public string? NameQ { get; set; }
        public string? PhoneQ { get; set; }
        public string? EmailQ { get; set; }
        public List<Row> Rows { get; set; } = new();

        public class Row
        {
            public int AppointmentId { get; set; }
            public DateTime StartAt { get; set; }

            public string ServiceName { get; set; } = "";
            public string VariantName { get; set; } = "";

            public string ClientName { get; set; } = "";
            public string? ClientPhone { get; set; }
            public string? ClientEmail { get; set; }
            public bool IsGuest { get; set; }

            public decimal FinalPrice { get; set; }
        }
    }
}
