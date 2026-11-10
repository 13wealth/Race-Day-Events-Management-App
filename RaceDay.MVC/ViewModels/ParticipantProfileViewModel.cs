namespace RaceDay.Mvc.ViewModels
{
    public class ParticipantProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? ProfileImageUrl { get; set; }

        public IFormFile? ProfileImage { get; set; }
    }
}