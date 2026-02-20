namespace BeautySalonProject.Areas.Staff.ViewModels.Dashboard
{
    public class StaffDashboardVm
    {
        public int TodayCount { get; set; }
        public int UpcomingCount { get; set; }
        public int CompletedTodayCount { get; set; }

        public List<TodayRow> TodaySchedule { get; set; } = new();

        public class TodayRow
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
