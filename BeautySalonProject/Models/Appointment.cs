using System;
using System.Collections.Generic;

namespace BeautySalonProject.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int VariantId { get; set; }

    public int EmployeeId { get; set; }

    public string? ClientUserId { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string? Notes { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? GuestFullName { get; set; }

    public string? GuestPhone { get; set; }

    public string? GuestEmail { get; set; }

    public decimal FinalPrice { get; set; }

    public int? InquiryId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Inquiry? Inquiry { get; set; }

    public virtual ServiceVariant Variant { get; set; } = null!;
    public ApplicationUser? ClientUser { get; set; }
}
