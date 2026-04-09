namespace BeautySalonProject.Areas.Staff.ViewModels.Profile
{
    public class EditProfileVm
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
