/***************************************************************************************
 *                         PARTICIPANT EVENTS VIEW MODEL                               *
 * Carries the information required to display upcoming race events.                  *
 * It contains:                                                                        *
 *  1. Upcoming events retrieved from the RaceDay API.                                 *
 *  2. Search text entered by the participant.                                         *
 *  3. Selected event type and location filters.                                       *
 *  4. Available event types and locations for the filter dropdowns.                   *
 ***************************************************************************************/

namespace RaceDay.Mvc.ViewModels
{
    public class ParticipantEventsViewModel
    {
        public List<EventApiResponse> Events { get; set; } = new List<EventApiResponse>();

        public string SearchTerm { get; set; } = "";

        public string SelectedEventType { get; set; } = "";

        public string SelectedLocation { get; set; } = "";

        public List<string> EventTypes { get; set; } = new List<string>();

        public List<string> Locations { get; set; } = new List<string>();
    }
}