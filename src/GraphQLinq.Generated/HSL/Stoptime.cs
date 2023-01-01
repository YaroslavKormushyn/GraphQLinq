namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Stoptime")]
    public partial class Stoptime
    {
        [GraphQL(Name = "stop")]
        public Stop Stop { get; set; }
        [GraphQL(Name = "scheduledArrival")]
        public int? ScheduledArrival { get; set; }
        [GraphQL(Name = "realtimeArrival")]
        public int? RealtimeArrival { get; set; }
        [GraphQL(Name = "arrivalDelay")]
        public int? ArrivalDelay { get; set; }
        [GraphQL(Name = "scheduledDeparture")]
        public int? ScheduledDeparture { get; set; }
        [GraphQL(Name = "realtimeDeparture")]
        public int? RealtimeDeparture { get; set; }
        [GraphQL(Name = "departureDelay")]
        public int? DepartureDelay { get; set; }
        [GraphQL(Name = "timepoint")]
        public bool? Timepoint { get; set; }
        [GraphQL(Name = "realtime")]
        public bool? Realtime { get; set; }
        [GraphQL(Name = "realtimeState")]
        public RealtimeState RealtimeState { get; set; }
        [GraphQL(Name = "pickupType")]
        public PickupDropoffType PickupType { get; set; }
        [GraphQL(Name = "dropoffType")]
        public PickupDropoffType DropoffType { get; set; }
        [GraphQL(Name = "serviceDay")]
        public long? ServiceDay { get; set; }
        [GraphQL(Name = "trip")]
        public Trip Trip { get; set; }
        [GraphQL(Name = "headsign")]
        public string Headsign { get; set; }
        [GraphQL(Name = "stopSequence")]
        public int? StopSequence { get; set; }
    }
}