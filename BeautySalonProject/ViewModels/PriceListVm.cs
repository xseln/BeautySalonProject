namespace BeautySalonProject.ViewModels
{

    public class ServicesCategoryVm
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public List<PriceGroupVm> Groups { get; set; } = new();
    }

    public class PriceGroupVm
    {
        public string ServiceName { get; set; } = "";
        public List<PriceItemVm> Items { get; set; } = new();
    }

    public class PriceItemVm
    {
        public int VariantId { get; set; }
        public string VariantName { get; set; } = "";
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
    }


}
