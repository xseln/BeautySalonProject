namespace BeautySalonProject.Models
{
    public enum AppointmentStatus : byte
    {
          Pending = 0,     // Чака
          Booked = 1,      // Потвърден
          Completed = 2,   // Приключен
          Cancelled = 3    // Отказан
    }
}

