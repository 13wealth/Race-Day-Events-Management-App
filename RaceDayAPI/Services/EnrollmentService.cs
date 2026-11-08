using RaceDay.API.Data;
using RaceDay.API.DTO;
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

        public EnrollmentCreateResponse EnrolInEvent(EnrollmentRequest request)
        {
            Participant participant = _authHelper.GetLoggedInParticipant();
            if (participant == null)
            {
                return new EnrollmentCreateResponse { Error = "Participant profile not found.", StatusCode = 404 };
            }

            Category category = _db.Categories.FirstOrDefault(c => c.CategoryId == request.CategoryId);
            if (category == null)
            {
                return new EnrollmentCreateResponse { Error = "Category not found.", StatusCode = 404 };
            }

            // Check if participant is already enrolled in this event
            Enrollment existing = _db.Enrollments.FirstOrDefault(en => en.ParticipantId == participant.ParticipantId && en.EventId == category.EventId);
            if (existing != null)
            {
                return new EnrollmentCreateResponse { Error = "You are already enrolled in this event.", StatusCode = 409 };
            }

            Enrollment enrollment = new Enrollment();
            enrollment.ParticipantId = participant.ParticipantId;
            enrollment.EventId = category.EventId;
            enrollment.CategoryId = category.CategoryId;
            enrollment.EntryDate = DateTime.UtcNow;
            enrollment.Status = "registered";

            _db.Enrollments.Add(enrollment);
            _db.SaveChanges();

            EnrollmentCreateResponse response = new EnrollmentCreateResponse();
            response.EnrollmentId = enrollment.EnrollmentId;
            response.ParticipantId = enrollment.ParticipantId;
            response.EventId = enrollment.EventId;
            response.CategoryId = enrollment.CategoryId;
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
                return new EnrollmentListResponse { Error = "Participant profile not found.", StatusCode = 404 };
            }

            List<Enrollment> mine = _db.Enrollments.Where(en => en.ParticipantId == participant.ParticipantId).ToList();

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
                return new EnrollmentEventListResponse { Error = "Organiser profile not found.", StatusCode = 404 };
            }

            RaceEvent found = _db.Events.FirstOrDefault(e => e.EventId == eventId);
            if (found == null)
            {
                return new EnrollmentEventListResponse { Error = "Event not found.", StatusCode = 404 };
            }

            if (found.OrganiserId != organiser.OrganiserId)
            {
                return new EnrollmentEventListResponse { Error = "You can only view enrolments for your own events.", StatusCode = 403 };
            }

            List<Enrollment> enrollments = _db.Enrollments.Where(en => en.EventId == eventId).ToList();

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
