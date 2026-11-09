namespace RaceDay.Mvc.ViewModels
{
    public class ParticipantEventDetailsViewModel
    {
        public EventApiResponse Event { get; set; }

        public List<CategoryApiResponse> Categories { get; set; }
            = new List<CategoryApiResponse>();

        public int SelectedCategoryId { get; set; }
    }
}