namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Trip")]
    public partial class Trip : Node
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "gtfsId")]
        public string GtfsId { get; set; }
        [GraphQL(Name = "route")]
        public Route Route { get; set; }
        [GraphQL(Name = "serviceId")]
        public string ServiceId { get; set; }
        [GraphQL(Name = "activeDates")]
        public List<string> ActiveDates { get; set; }
        [GraphQL(Name = "tripShortName")]
        public string TripShortName { get; set; }
        [GraphQL(Name = "tripHeadsign")]
        public string TripHeadsign { get; set; }
        [GraphQL(Name = "routeShortName")]
        public string RouteShortName { get; set; }
        [GraphQL(Name = "directionId")]
        public string DirectionId { get; set; }
        [GraphQL(Name = "blockId")]
        public string BlockId { get; set; }
        [GraphQL(Name = "shapeId")]
        public string ShapeId { get; set; }
        [GraphQL(Name = "wheelchairAccessible")]
        public WheelchairBoarding WheelchairAccessible { get; set; }
        [GraphQL(Name = "bikesAllowed")]
        public BikesAllowed BikesAllowed { get; set; }
        [GraphQL(Name = "pattern")]
        public Pattern Pattern { get; set; }
        [GraphQL(Name = "stops")]
        public List<Stop> Stops { get; set; }
        [GraphQL(Name = "semanticHash")]
        public string SemanticHash { get; set; }
        [GraphQL(Name = "stoptimes")]
        public List<Stoptime> Stoptimes { get; set; }
        [GraphQL(Name = "departureStoptime")]
        public Stoptime DepartureStoptime { get; set; }
        [GraphQL(Name = "arrivalStoptime")]
        public Stoptime ArrivalStoptime { get; set; }
        [GraphQL(Name = "stoptimesForDate")]
        public List<Stoptime> StoptimesForDate { get; set; }
        [GraphQL(Name = "geometry")]
        public List<List<float>> Geometry { get; set; }
        [GraphQL(Name = "tripGeometry")]
        public Geometry TripGeometry { get; set; }
        [GraphQL(Name = "alerts")]
        public List<Alert> Alerts { get; set; }
    }
}