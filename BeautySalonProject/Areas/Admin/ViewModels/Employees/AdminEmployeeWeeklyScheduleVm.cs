namespace BeautySalonProject.Areas.Admin.ViewModels.Employees
{
	public class AdminEmployeeWeeklyScheduleVm
	{
		public int EmployeeId { get; set; }
		public string EmployeeName { get; set; } = "";
		public DateOnly WeekStart { get; set; }

		public List<DayRow> Days { get; set; } = new();

		public class DayRow
		{
			public DateOnly Date { get; set; }
			public int DayOfWeek { get; set; }
            public string DayName { get; set; } = "";
		    public bool IsWorking { get; set; }
	    	public TimeOnly? StartTime { get; set; }
		    public TimeOnly? EndTime { get; set; }
		}
		
	}
}

