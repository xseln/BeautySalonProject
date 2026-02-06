namespace BeautySalonProject.Areas.Admin.ViewModels
{
    public class DashboardVm
    {
        public int NewInquiriesCount { get; set; }
        public int TodayAppointmentsCount { get; set; }

        public List<InquiryRow> LatestInquiries { get; set; } = new();
        public List<AppointmentRow> TodaysSchedule { get; set; } = new();
        public List<TopServiceRow> TopServices { get; set; } = new();

        public class InquiryRow
        {
            public int InquiryId { get; set; }
            public string FullName { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Email { get; set; } = "";
            public string ServiceText { get; set; } = "";
            public DateTime? PreferredDateTime { get; set; }
            public byte Status { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class AppointmentRow
        {
            public int AppointmentId { get; set; }
            public DateTime StartAt { get; set; }
            public DateTime EndAt { get; set; }
            public string EmployeeName { get; set; } = "";
            public string ServiceName { get; set; } = "";
            public string VariantName { get; set; } = "";
            public byte Status { get; set; }
        }

        public class TopServiceRow
        {
            public string Title { get; set; } = "";
            public int Count { get; set; }
        }
    }
}
