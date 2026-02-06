using System;
using System.Collections.Generic;

namespace BeautySalonProject.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public int CategoryId { get; set; }

    public int EmployeeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ServiceCategory Category { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<ServiceVariant> ServiceVariants { get; set; } = new List<ServiceVariant>();
}
