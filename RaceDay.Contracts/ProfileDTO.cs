
namespace RaceDay.Contracts
{
    public class ProfileRequest
    {
        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public string Phone { get; set; } = "";
    }

    public class ProfileResponse
    {
        public int ProfileId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    public class ProfileGetResponse
    {
        public ProfileResponse? Profile { get; set; }
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }

    public class ProfileUpdateResponse
    {
        public ProfileResponse? Profile { get; set; }
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }
}