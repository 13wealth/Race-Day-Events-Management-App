using System;

namespace RaceDay.API.Dtos
{
    public class ResultRequest
    {
        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }

        public int Position { get; set; }
    }
}