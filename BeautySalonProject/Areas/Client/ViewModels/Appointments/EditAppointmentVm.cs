namespace BeautySalonProject.Areas.Client.ViewModels.Appointments
{
	public class EditAppointmentVm
	{
		public int AppointmentId { get; set; }

		public int EmployeeId { get; set; }

		public DateTime Date { get; set; }

		public int VariantId { get; set; }

		public List<EmployeeOption> Employees { get; set; } = new();

		public List<string> FreeSlots { get; set; } = new();

		public class EmployeeOption
		{
			public int Id { get; set; }

			public string Name { get; set; } = "";
		}
	}
}