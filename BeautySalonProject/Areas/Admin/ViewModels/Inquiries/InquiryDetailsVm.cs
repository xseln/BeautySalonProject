namespace BeautySalonProject.Areas.Admin.ViewModels.Inquiries
{
    public class InquiryDetailsVm
    {
        public int InquiryId { get; set; }
        public int? VariantId { get; set; }
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string ServiceText { get; set; } = "";
        public DateTime? PreferredDateTime { get; set; }
        public string? Message { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
