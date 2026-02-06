namespace BeautySalonProject.Areas.Admin.ViewModels
{
    public class InquiryListRowVm
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
}
