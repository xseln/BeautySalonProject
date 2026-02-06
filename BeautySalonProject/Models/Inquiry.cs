using System;
using System.Collections.Generic;

namespace BeautySalonProject.Models;

public partial class Inquiry
{
    public int InquiryId { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string ServiceText { get; set; } = null!;

    public DateTime? PreferredDateTime { get; set; }

    public string? Message { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ServiceVariantId { get; set; }

    public int? AppointmentId { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? AdminNote { get; set; }

    public int? VariantId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ServiceVariant? ServiceVariant { get; set; }
}
