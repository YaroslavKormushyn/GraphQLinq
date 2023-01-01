namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Itinerary")]
    public partial class Itinerary
    {
        [GraphQL(Name = "startTime")]
        public long? StartTime { get; set; }
        [GraphQL(Name = "endTime")]
        public long? EndTime { get; set; }
        [GraphQL(Name = "duration")]
        public long? Duration { get; set; }
        [GraphQL(Name = "waitingTime")]
        public long? WaitingTime { get; set; }
        [GraphQL(Name = "walkTime")]
        public long? WalkTime { get; set; }
        [GraphQL(Name = "walkDistance")]
        public float? WalkDistance { get; set; }
        [GraphQL(Name = "legs")]
        public List<Leg> Legs { get; set; }
        [GraphQL(Name = "fares")]
        public List<Fare> Fares { get; set; }
        [GraphQL(Name = "elevationGained")]
        public float? ElevationGained { get; set; }
        [GraphQL(Name = "elevationLost")]
        public float? ElevationLost { get; set; }
    }
}