namespace BeautySalonProject.Areas.Admin.ViewModels.Employees
{
    public class AdminEmployeeIndexVm
    {
        public bool? Active { get; set; }
        public List<Row> Items { get; set; } = new();

        public class Row
        {
            public int EmployeeId { get; set; }
            public string FullName { get; set; } = "";
            public string? JobTitle { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public bool IsActive { get; set; }
            public List<string> Services { get; set; } = new();
            public string? CategoryName { get; set; }

        }
    }
}
