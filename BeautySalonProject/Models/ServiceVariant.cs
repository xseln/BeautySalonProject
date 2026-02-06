using System;
using System.Collections.Generic;

namespace BeautySalonProject.Models;

public partial class ServiceVariant
{
    public int VariantId { get; set; }

    public int ServiceId { get; set; }

    public string VariantName { get; set; } = null!;

    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();

    public virtual Service Service { get; set; } = null!;
}
