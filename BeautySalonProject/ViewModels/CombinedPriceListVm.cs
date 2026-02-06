namespace BeautySalonProject.ViewModels
{
    public class CombinedPriceListVm
    {
       
            public string Title { get; set; } = "";
            public List<CategoryBlockVm> Categories { get; set; } = new();
        

        public class CategoryBlockVm
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = "";
            public List<ServiceBlockVm> Services { get; set; } = new();
        }

        public class ServiceBlockVm
        {
            public int ServiceId { get; set; }
            public string ServiceName { get; set; } = "";
            public List<VariantRowVm> Variants { get; set; } = new();
        }

        public class VariantRowVm
        {
            public int VariantId { get; set; }
            public string VariantName { get; set; } = "";
            public decimal Price { get; set; }
            public int DurationMinutes { get; set; }
        }
    }
}
