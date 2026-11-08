using RaceDay.API.Data;
using RaceDay.Contracts;
using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public class EnrollmentService
    {
        private readonly RaceDayContext _db;
        private readonly AuthenticationHelper _authHelper;
        private readonly ValidationHelper _validationHelper;

        public EnrollmentService(RaceDayContext db, AuthenticationHelper authHelper, ValidationHelper validationHelper)
        {
            _db = db;
            _authHelper = authHelper;
            _validationHelper = validationHelper;
        }

        /*
         * Enrolls a participant in a specific category for an event.
         * Retrieves the logged-in participant and searches for the specified category.
         * Verifies that the category exists and that the participant is not already enrolled in that event.
         * Creates an Enrollment record linking the participant to the category.
         * Returns an EnrollmentCreateResponse with enrollment details and 201 status code on success.
         * Returns error responses with appropriate status codes (404, 400) for validation or duplicate enrollment failures.
         */
        public EnrollmentCreateResponse EnrolInEvent(EnrollmentRequest request)
        {
            // Get the logged-in participant from the session and authentication helper
            Participant participant = _authHelper.GetLoggedInParticipant();
            if (participant == null)                                                    // Check if user is not a participant
            {
                EnrollmentCreateResponse participantErrorResponse = new EnrollmentCreateResponse();
                participantErrorResponse.Error = "Participant profile not found.";
                participantErrorResponse.StatusCode = 404;
                return participantErrorResponse;
            }

            // Search for the category to enrol in by its category ID
            Category category = null;                                                   // Initialize category to null
            foreach (Category c in _db.Categories)
            {
                if (c.CategoryId == request.CategoryId)                                 // Check if category ID matches
                {
                    category = c;                                                       // Found the category
                    break;                                                              // Exit the loop immediately
                }
            }

            if (category == null)                                                       // If no category was found with that ID
            {
                EnrollmentCreateResponse categoryErrorResponse = new EnrollmentCreateResponse();
                categoryErrorResponse.Error = "Category not found.";
                categoryErrorResponse.StatusCode = 404;
                return categoryErrorResponse;
            }

            /*
             * Check if the participant is already enrolled in this event.
             * Prevent duplicate enrollments by verifying that no enrollment record exists.
             * If one is found, return a 400 Bad Request error.
             */
            Enrollment existing = null;                                                 // Initialize existing enrollment to null
            foreach (Enrollment en in _db.Enrollments)
            {
                if (en.ParticipantId == participant.ParticipantId && en.EventId == category.EventId)  // Check if enrollment already exists for this event
                {
                    existing = en;                                                      // Found existing enrollment
                    break;                                                              // Exit the loop immediately
                }
            }

            if (existing != null)                                                       // If an existing enrollment was found
            {
                EnrollmentCreateResponse alreadyEnrolledResponse = new EnrollmentCreateResponse();
                alreadyEnrolledResponse.Error = "You are already enrolled in this event.";
                alreadyEnrolledResponse.StatusCode = 409;
                return alreadyEnrolledResponse;
            }

            // Create a new enrollment record linking the participant to the category
            Enrollment enrollment = new Enrollment();
            enrollment.ParticipantId = participant.ParticipantId;
            enrollment.CategoryId = request.CategoryId;
            enrollment.EventId = category.EventId;
            enrollment.EntryDate = DateTime.UtcNow;

            // Add the new enrollment to the database and save changes
            _db.Enrollments.Add(enrollment);
            _db.SaveChanges();

            // Create success response with the created enrollment details
            EnrollmentCreateResponse response = new EnrollmentCreateResponse();
            response.EnrollmentId = enrollment.EnrollmentId;
            response.ParticipantId = enrollment.ParticipantId;
            response.CategoryId = enrollment.CategoryId;
            response.EventId = enrollment.EventId;
            response.EntryDate = enrollment.EntryDate;
            response.Status = enrollment.Status;
            response.StatusCode = 201;

            return response;
        }

        public EnrollmentListResponse GetMyEnrollments()
        {
            Participant participant = _authHelper.GetLoggedInParticipant();
            if (participant == null)
            {
                EnrollmentListResponse participantErrorResponse = new EnrollmentListResponse();
                participantErrorResponse.Error = "Participant profile not found.";
                participantErrorResponse.StatusCode = 404;
                return participantErrorResponse;
            }

            List<Enrollment> mine = new List<Enrollment>();
            foreach (Enrollment en in _db.Enrollments)
            {
                if (en.ParticipantId == participant.ParticipantId)
                {
                    mine.Add(en);
                }
            }

            EnrollmentListResponse response = new EnrollmentListResponse();
            response.Enrollments = new List<object>();

            foreach (Enrollment en in mine)
            {
                object enrollmentObj = new
                {
                    enrolment_id = en.EnrollmentId,
                    participant_id = en.ParticipantId,
                    event_id = en.EventId,
                    category_id = en.CategoryId,
                    entry_date = en.EntryDate,
                    status = en.Status
                };
                response.Enrollments.Add(enrollmentObj);
            }

            response.StatusCode = 200;
            return response;
        }

        public EnrollmentEventListResponse GetEventEnrollments(int eventId)
        {
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)
            {
                EnrollmentEventListResponse organiserErrorResponse = new EnrollmentEventListResponse();
                organiserErrorResponse.Error = "Organiser profile not found.";
                organiserErrorResponse.StatusCode = 404;
                return organiserErrorResponse;
            }

            RaceEvent found = null;
            foreach (RaceEvent e in _db.Events)
            {
                if (e.EventId == eventId)
                {
                    found = e;
                    break;
                }
            }

            if (found == null)
            {
                EnrollmentEventListResponse eventErrorResponse = new EnrollmentEventListResponse();
                eventErrorResponse.Error = "Event not found.";
                eventErrorResponse.StatusCode = 404;
                return eventErrorResponse;
            }

            if (found.OrganiserId != organiser.OrganiserId)
            {
                EnrollmentEventListResponse forbiddenResponse = new EnrollmentEventListResponse();
                forbiddenResponse.Error = "You can only view enrolments for your own events.";
                forbiddenResponse.StatusCode = 403;
                return forbiddenResponse;
            }

            List<Enrollment> enrollments = new List<Enrollment>();
            foreach (Enrollment en in _db.Enrollments)
            {
                if (en.EventId == eventId)
                {
                    enrollments.Add(en);
                }
            }

            EnrollmentEventListResponse response = new EnrollmentEventListResponse();
            response.Enrollments = new List<object>();

            foreach (Enrollment en in enrollments)
            {
                object enrollmentObj = new
                {
                    enrolment_id = en.EnrollmentId,
                    participant_id = en.ParticipantId,
                    event_id = en.EventId,
                    category_id = en.CategoryId,
                    entry_date = en.EntryDate,
                    status = en.Status
                };
                response.Enrollments.Add(enrollmentObj);
            }

            response.StatusCode = 200;
            return response;
        }

        public class EnrollmentCreateResponse
        {
            public int EnrollmentId { get; set; }
            public int ParticipantId { get; set; }
            public int EventId { get; set; }
            public int CategoryId { get; set; }
            public DateTime EntryDate { get; set; }
            public string Status { get; set; }
            public string Error { get; set; }
            public int StatusCode { get; set; }
        }

        public class EnrollmentListResponse
        {
            public List<object> Enrollments { get; set; }
            public string Error { get; set; }
            public int StatusCode { get; set; }
        }

        public class EnrollmentEventListResponse
        {
            public List<object> Enrollments { get; set; }
            public string Error { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
