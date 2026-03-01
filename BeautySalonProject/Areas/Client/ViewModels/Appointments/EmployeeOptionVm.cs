namespace BeautySalonProject.Areas.Client.ViewModels.Appointments
{
    public class EmployeeOptionVm
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = "";
        public string? JobTitle { get; set; }
        public bool IsActive { get; set; }
    }
}
