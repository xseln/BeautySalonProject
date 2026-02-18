using System;
using System.Collections.Generic;

namespace BeautySalonProject.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public string? IdentityUserId { get; set; }

    public string? JobTitle { get; set; }
    public int? PrimaryCategoryId { get; set; }
    public virtual ServiceCategory? PrimaryCategory { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
	public virtual ICollection<EmployeeWorkDay> WorkDays { get; set; } = new List<EmployeeWorkDay>();

}
