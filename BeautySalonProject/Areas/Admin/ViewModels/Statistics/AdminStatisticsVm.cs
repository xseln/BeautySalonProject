using System.Text.Json.Serialization;
namespace BeautySalonProject.Areas.Admin.ViewModels.Statistics
{
    public class AdminStatisticsVm
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public int VisitsCompleted { get; set; }
        public int UniqueClientsCompleted { get; set; }
        public decimal RevenueCompleted { get; set; }
        public List<string> Labels { get; set; } = new();
        public List<int> VisitsPerDay { get; set; } = new();
        public List<decimal> RevenuePerDay { get; set; } = new();
        public List<TopServiceRow> TopServices { get; set; } = new();

        public class TopServiceRow
        {
            public string ServiceName { get; set; } = "";
            public int Count { get; set; }
            public decimal Revenue { get; set; }
        }
    }
}
