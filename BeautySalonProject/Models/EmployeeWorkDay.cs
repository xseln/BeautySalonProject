namespace BeautySalonProject.Models
{
	public class EmployeeWorkDay
	{
		public int EmployeeWorkDayId { get; set; }

		public int EmployeeId { get; set; }
		public virtual Employee Employee { get; set; } = null!;

		public DateOnly Date { get; set; }

		public bool IsWorking { get; set; } = true;

		public TimeOnly? StartTime { get; set; }
		public TimeOnly? EndTime { get; set; }
	}
}
