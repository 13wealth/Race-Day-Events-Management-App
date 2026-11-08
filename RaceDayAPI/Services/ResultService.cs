using RaceDay.API.Data;
using RaceDay.API.DTO;
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

        public ResultCreateResponse RecordResult(int enrolmentId, ResultRequest request)
        {
            Organiser organiser = _authHelper.GetLoggedInOrganiser();
            if (organiser == null)
            {
                return new ResultCreateResponse { Error = "Organiser profile not found.", StatusCode = 404 };
            }

            Enrollment enrollment = _db.Enrollments.FirstOrDefault(en => en.EnrollmentId == enrolmentId);
            if (enrollment == null)
            {
                return new ResultCreateResponse { Error = "Enrolment not found.", StatusCode = 404 };
            }

            RaceEvent ev = _db.Events.FirstOrDefault(e => e.EventId == enrollment.EventId);
            if (ev == null)
            {
                return new ResultCreateResponse { Error = "Event not found.", StatusCode = 404 };
            }

            if (ev.OrganiserId != organiser.OrganiserId)
            {
                return new ResultCreateResponse { Error = "You can only record results for your own events.", StatusCode = 403 };
            }

            RaceResult existing = _db.Results.FirstOrDefault(r => r.EnrollmentId == enrolmentId);
            if (existing != null)
            {
                return new ResultCreateResponse { Error = "A result is already recorded for this enrolment.", StatusCode = 409 };
            }

            if (request.FinishTime < request.StartTime)
            {
                return new ResultCreateResponse { Error = "Finish time cannot be before start time.", StatusCode = 400 };
            }

            if (request.Position < 1)
            {
                return new ResultCreateResponse { Error = "Position must be 1 or more.", StatusCode = 400 };
            }

            RaceResult result = new RaceResult();
            result.EnrollmentId = enrolmentId;
            result.StartTime = request.StartTime;
            result.FinishTime = request.FinishTime;
            result.Position = request.Position;
            result.CreatedAt = DateTime.UtcNow;

            _db.Results.Add(result);
            _db.SaveChanges();

            ResultCreateResponse response = new ResultCreateResponse();
            response.ResultsId = result.ResultsId;
            response.EnrollmentId = result.EnrollmentId;
            response.StartTime = result.StartTime;
            response.FinishTime = result.FinishTime;
            response.Position = result.Position;
            response.StatusCode = 201;

            return response;
        }

        public ResultDetailResponse GetResult(int resultId)
        {
            RaceResult found = _db.Results.FirstOrDefault(r => r.ResultsId == resultId);
            if (found == null)
            {
                return new ResultDetailResponse { Error = "Result not found.", StatusCode = 404 };
            }

            Enrollment enrollment = _db.Enrollments.FirstOrDefault(en => en.EnrollmentId == found.EnrollmentId);

            string role = _authHelper.GetUserRole();

            // Participants may only see their own results
            if (role == "participant")
            {
                Participant participant = _authHelper.GetLoggedInParticipant();
                if (participant == null || enrollment.ParticipantId != participant.ParticipantId)
                {
                    return new ResultDetailResponse { Error = "You can only view your own results.", StatusCode = 403 };
                }
            }

            // Organisers may only see results for their own events
            if (role == "organiser")
            {
                Organiser organiser = _authHelper.GetLoggedInOrganiser();
                RaceEvent ev = _db.Events.FirstOrDefault(e => e.EventId == enrollment.EventId);

                if (organiser == null || ev == null || ev.OrganiserId != organiser.OrganiserId)
                {
                    return new ResultDetailResponse { Error = "You can only view results for your own events.", StatusCode = 403 };
                }
            }

            ResultDetailResponse response = new ResultDetailResponse();
            response.ResultsId = found.ResultsId;
            response.EnrollmentId = found.EnrollmentId;
            response.StartTime = found.StartTime;
            response.FinishTime = found.FinishTime;
            response.Position = found.Position;
            response.StatusCode = 200;

            return response;
        }
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
