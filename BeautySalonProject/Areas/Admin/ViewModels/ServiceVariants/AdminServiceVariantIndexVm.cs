namespace BeautySalonProject.Areas.Admin.ViewModels.ServiceVariants
{
    public class AdminServiceVariantIndexVm
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = "";
        public string CategoryName { get; set; } = "";

        public List<Row> Items { get; set; } = new();

        public class Row
        {
            public int VariantId { get; set; }
            public string VariantName { get; set; } = "";
            public decimal Price { get; set; }
            public int DurationMinutes { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
