using BeautySalonProject.Models.Enums;

namespace BeautySalonProject.Areas.Admin.ViewModels.Inquiries
{
    public class InquiryIndexVm
    {
        public InquiryStatus? StatusFilter { get; set; } 
        public List<Row> Items { get; set; } = new();

        public class Row
        {
            public int InquiryId { get; set; }
            public int? VariantId { get; set; }
            public string FullName { get; set; } = "";
            public string Phone { get; set; } = "";
            public string ServiceText { get; set; } = "";
            public DateTime? PreferredDateTime { get; set; }
            public InquiryStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
