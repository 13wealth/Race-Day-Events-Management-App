using RaceDay.API.Data;
using RaceDay.Contracts;
using RaceDay.API.Models;
using RaceResult = RaceDay.API.Models.RaceResult;

namespace RaceDay.API.Services
{
    public class ResultService
    {
        private readonly RaceDayContext _db;
        private readonly AuthenticationHelper _authHelper;

        public ResultService(RaceDayContext db, AuthenticationHelper authHelper)
        {
            _db = db;
            _authHelper = authHelper;
        }

        /*
         * Records a result for a completed enrollment.
         * Retrieves the logged-in organiser and verifies ownership of the event.
         * Finds the enrollment and associated event, then creates a result record.
         * Returns a ResultCreateResponse with result details and 201 status code on success.
         * Returns error responses with appropriate status codes (404, 403, 400) for authorization or validation failures.
         */
        public ResultCreateResponse RecordResult(int enrolmentId, ResultRequest request)
        {
            // Get the logged-in organiser from the session and authentication helper
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)                                                      // Check if user is not an organiser
            {
                ResultCreateResponse organiserErrorResponse = new ResultCreateResponse();
                organiserErrorResponse.Error = "Organiser profile not found.";
                organiserErrorResponse.StatusCode = 404;
                return organiserErrorResponse;
            }

            // Search for the enrollment record by its enrollment ID
            Enrollment enrollment = null;                                               // Initialize enrollment to null
            foreach (Enrollment en in _db.Enrollments)
            {
                if (en.EnrollmentId == enrolmentId)                                     // Check if enrollment ID matches
                {
                    enrollment = en;                                                    // Found the enrollment
                    break;                                                              // Exit the loop immediately
                }
            }

            if (enrollment == null)                                                     // If no enrollment was found with that ID
            {
                ResultCreateResponse enrollmentErrorResponse = new ResultCreateResponse();
                enrollmentErrorResponse.Error = "Enrolment not found.";
                enrollmentErrorResponse.StatusCode = 404;
                return enrollmentErrorResponse;
            }

            // Search for the event associated with the enrollment
            RaceEvent ev = null;                                                        // Initialize event to null
            foreach (RaceEvent e in _db.Events)
            {
                if (e.EventId == enrollment.EventId)                                    // Check if event ID matches enrollment's event ID
                {
                    ev = e;                                                             // Found the event
                    break;                                                              // Exit the loop immediately
                }
            }

            if (ev == null)                                                             // If no event was found
            {
                ResultCreateResponse eventErrorResponse = new ResultCreateResponse();
                eventErrorResponse.Error = "Event not found.";
                eventErrorResponse.StatusCode = 404;
                return eventErrorResponse;
            }

            /*
             * Verify that the logged-in organiser is the owner of the event.
             * This prevents organisers from recording results for other organisers' events.
             */
            if (ev.OrganiserId != organiser.OrganiserId)                               // Check if organiser does not own this event
            {
                ResultCreateResponse forbiddenResponse = new ResultCreateResponse();
                forbiddenResponse.Error = "You can only record results for your own events.";
                forbiddenResponse.StatusCode = 403;
                return forbiddenResponse;
            }

            // Check if a result already exists for this enrollment
            RaceResult existing = null;                                                 // Initialize existing result to null
            foreach (RaceResult r in _db.Results)
            {
                if (r.EnrollmentId == enrolmentId)                                      // Check if result exists for this enrollment
                {
                    existing = r;                                                       // Found existing result
                    break;                                                              // Exit the loop immediately
                }
            }

            if (existing != null)                                                       // If a result already exists for this enrollment
            {
                ResultCreateResponse alreadyExistsResponse = new ResultCreateResponse();
                alreadyExistsResponse.Error = "A result is already recorded for this enrolment.";
                alreadyExistsResponse.StatusCode = 409;
                return alreadyExistsResponse;
            }

            // Create a new result record for the enrollment
            RaceResult result = new RaceResult();
            result.EnrollmentId = enrolmentId;
            result.StartTime = request.StartTime;
            result.FinishTime = request.FinishTime;
            result.Position = request.Position;
            result.CreatedAt = DateTime.UtcNow;

            // Add the new result to the database and save changes
            _db.Results.Add(result);
            _db.SaveChanges();

            // Create success response with the created result details
            ResultCreateResponse response = new ResultCreateResponse();
            response.ResultsId = result.ResultsId;
            response.EnrollmentId = result.EnrollmentId;
            response.Position = result.Position;
            response.StatusCode = 201;

            return response;
        }

        /*
         * Retrieves a specific result by its result ID.
         * Verifies that the logged-in user has permission to view the result based on their role.
         * Organisers can view results for their own events.
         * Participants can view results for their own enrollments.
         * Returns a ResultDetailResponse with result details and 200 status code on success.
         * Returns error responses with appropriate status codes (404, 403) for validation or authorization failures.
         */
        public ResultDetailResponse GetResult(int resultId)
        {
            // Search for the result record by its result ID
            RaceResult found = null;                                                    // Initialize found result to null
            foreach (RaceResult r in _db.Results)
            {
                if (r.ResultsId == resultId)                                            // Check if result ID matches
                {
                    found = r;                                                          // Found the result
                    break;                                                              // Exit the loop immediately
                }
            }

            if (found == null)                                                          // If no result was found with that ID
            {
                ResultDetailResponse notFoundResponse = new ResultDetailResponse();
                notFoundResponse.Error = "Result not found.";
                notFoundResponse.StatusCode = 404;
                return notFoundResponse;
            }

            // Search for the enrollment associated with this result
            Enrollment enrollment = null;                                               // Initialize enrollment to null
            foreach (Enrollment en in _db.Enrollments)
            {
                if (en.EnrollmentId == found.EnrollmentId)                              // Check if enrollment ID matches
                {
                    enrollment = en;                                                    // Found the enrollment
                    break;                                                              // Exit the loop immediately
                }
            }

            // Retrieve the logged-in user's role for authorization checks
            string role = _authHelper.GetUserRole();

            /*
             * Participants may only see their own results.
             * Verify that the logged-in participant is the owner of the enrollment.
             * If not, return 403 Forbidden.
             */
            if (role == "participant")                                                  // If user is a participant
            {
                // Get the logged-in participant and check if they own this enrollment
                Participant participant = _authHelper.GetLoggedInParticipant();
                if (participant == null || enrollment.ParticipantId != participant.ParticipantId)  // Check if participant does not own this enrollment
                {
                    ResultDetailResponse forbiddenResponse = new ResultDetailResponse();
                    forbiddenResponse.Error = "You can only view your own results.";
                    forbiddenResponse.StatusCode = 403;
                    return forbiddenResponse;
                }
            }

            /*
             * Organisers may only see results for their own events.
             * Search for the event and verify ownership before allowing result viewing.
             * If not, return 403 Forbidden.
             */
            if (role == "organiser")                                                    // If user is an organiser
            {
                // Get the logged-in organiser and find the associated event
                Organiser organiser = _authHelper.GetLoggedInOrganiser();
                RaceEvent ev = null;                                                    // Initialize event to null
                foreach (RaceEvent e in _db.Events)
                {
                    if (e.EventId == enrollment.EventId)                                // Check if event ID matches enrollment's event ID
                    {
                        ev = e;                                                         // Found the event
                        break;                                                          // Exit the loop immediately
                    }
                }

                /*
                 * Verify that the organiser is the owner of the event.
                 * If organiser or event is not found, or if organiser does not own the event, deny access.
                 */
                if (organiser == null || ev == null || ev.OrganiserId != organiser.OrganiserId)  // Check if organiser does not own this event
                {
                    ResultDetailResponse eventForbiddenResponse = new ResultDetailResponse();
                    eventForbiddenResponse.Error = "You can only view results for your own events.";
                    eventForbiddenResponse.StatusCode = 403;
                    return eventForbiddenResponse;
                }
            }

            // Create success response with the result details
            ResultDetailResponse response = new ResultDetailResponse();
            response.ResultsId = found.ResultsId;
            response.EnrollmentId = found.EnrollmentId;
            response.StartTime = found.StartTime;
            response.FinishTime = found.FinishTime;
            response.Position = found.Position;
            response.StatusCode = 200;

            return response;
        }

        public class ResultCreateResponse
        {
            public int ResultsId { get; set; }
            public int EnrollmentId { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime FinishTime { get; set; }
            public int Position { get; set; }
            public string Error { get; set; }
            public int StatusCode { get; set; }
        }

        public class ResultDetailResponse
        {
            public int ResultsId { get; set; }
            public int EnrollmentId { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime FinishTime { get; set; }
            public int Position { get; set; }
            public string Error { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
