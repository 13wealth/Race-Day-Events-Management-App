using System;

namespace RaceDay.Contracts
{
    public class ResultResponse
    {
        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }

        public int Position { get; set; }
    }
}