namespace BeautySalonProject.Areas.Admin.ViewModels.Services
{
    public class AdminServiceIndexVm
    {
        public int? CategoryId { get; set; }
        public bool? Active { get; set; }

        public List<CategoryFilter> Categories { get; set; } = new();
        public List<Row> Items { get; set; } = new();

        public class CategoryFilter
        {
            public int CategoryId { get; set; }
            public string Name { get; set; } = "";
        }

        public class Row
        {
            public int ServiceId { get; set; }
            public string CategoryName { get; set; } = "";
            public string ServiceName { get; set; } = "";
            public string EmployeeName { get; set; } = "";
            public bool IsActive { get; set; }
            public int VariantsCount { get; set; }
            public int TotalVariantsCount { get; set; }
        }
    }
}
