namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Leg")]
    public partial class Leg
    {
        [GraphQL(Name = "startTime")]
        public long? StartTime { get; set; }
        [GraphQL(Name = "endTime")]
        public long? EndTime { get; set; }
        [GraphQL(Name = "departureDelay")]
        public int? DepartureDelay { get; set; }
        [GraphQL(Name = "arrivalDelay")]
        public int? ArrivalDelay { get; set; }
        [GraphQL(Name = "mode")]
        public Mode Mode { get; set; }
        [GraphQL(Name = "duration")]
        public float? Duration { get; set; }
        [GraphQL(Name = "legGeometry")]
        public Geometry LegGeometry { get; set; }
        [GraphQL(Name = "agency")]
        public Agency Agency { get; set; }
        [GraphQL(Name = "realTime")]
        public bool? RealTime { get; set; }
        [GraphQL(Name = "realtimeState")]
        public RealtimeState RealtimeState { get; set; }
        [GraphQL(Name = "distance")]
        public float? Distance { get; set; }
        [GraphQL(Name = "transitLeg")]
        public bool? TransitLeg { get; set; }
        [GraphQL(Name = "rentedBike")]
        public bool? RentedBike { get; set; }
        [GraphQL(Name = "from")]
        public Place From { get; set; }
        [GraphQL(Name = "to")]
        public Place To { get; set; }
        [GraphQL(Name = "route")]
        public Route Route { get; set; }
        [GraphQL(Name = "trip")]
        public Trip Trip { get; set; }
        [GraphQL(Name = "serviceDate")]
        public string ServiceDate { get; set; }
        [GraphQL(Name = "interlineWithPreviousLeg")]
        public bool? InterlineWithPreviousLeg { get; set; }
        [GraphQL(Name = "intermediateStops")]
        public List<Stop> IntermediateStops { get; set; }
        [GraphQL(Name = "intermediatePlaces")]
        public List<Place> IntermediatePlaces { get; set; }
        [GraphQL(Name = "intermediatePlace")]
        public bool? IntermediatePlace { get; set; }
        [GraphQL(Name = "steps")]
        public List<Step> Steps { get; set; }
    }
}