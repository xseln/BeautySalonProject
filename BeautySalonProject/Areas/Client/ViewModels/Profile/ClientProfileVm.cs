using System.ComponentModel.DataAnnotations;

namespace BeautySalonProject.Areas.Client.ViewModels.Profile
{
    public class ClientProfileVm
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Phone]
        public string? PhoneNumber { get; set; }

        public string Email { get; set; } = "";
    }
}
