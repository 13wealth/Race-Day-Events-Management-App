/***************************************************************************************
 *                    ORGANISER ENROLLMENTS VIEW MODEL                                 *
 * Holds all enrollment information for an organiser's events.                         *
 * It includes:                                                                        *
 *  1. All enrollments across events                                                   *
 *  2. Associated event information for each enrollment                                *
 *  3. Search functionality                                                            *
 ***************************************************************************************/

namespace RaceDay.Mvc.ViewModels
{
    public class OrganiserEnrollmentsViewModel
    {
        public List<EnrollmentWithEventInfo> Enrollments { get; set; }
            = new List<EnrollmentWithEventInfo>();

        public string SearchTerm { get; set; } = "";
    }

    public class EnrollmentWithEventInfo
    {
        public int EnrollmentId { get; set; }

        public int ParticipantId { get; set; }

        public string ParticipantName { get; set; } = "";

        public int EventId { get; set; }

        public string EventName { get; set; } = "";

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = "";

        public DateTime EntryDate { get; set; }

        public string Status { get; set; } = "";
    }
}
