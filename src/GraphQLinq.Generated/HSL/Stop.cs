namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Stop")]
    public partial class Stop : Node, PlaceInterface
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "stopTimesForPattern")]
        public List<Stoptime> StopTimesForPattern { get; set; }
        [GraphQL(Name = "gtfsId")]
        public string GtfsId { get; set; }
        [GraphQL(Name = "name")]
        public string Name { get; set; }
        [GraphQL(Name = "lat")]
        public float? Lat { get; set; }
        [GraphQL(Name = "lon")]
        public float? Lon { get; set; }
        [GraphQL(Name = "code")]
        public string Code { get; set; }
        [GraphQL(Name = "desc")]
        public string Desc { get; set; }
        [GraphQL(Name = "zoneId")]
        public string ZoneId { get; set; }
        [GraphQL(Name = "url")]
        public string Url { get; set; }
        [GraphQL(Name = "locationType")]
        public LocationType LocationType { get; set; }
        [GraphQL(Name = "parentStation")]
        public Stop ParentStation { get; set; }
        [GraphQL(Name = "wheelchairBoarding")]
        public WheelchairBoarding WheelchairBoarding { get; set; }
        [GraphQL(Name = "direction")]
        public string Direction { get; set; }
        [GraphQL(Name = "timezone")]
        public string Timezone { get; set; }
        [GraphQL(Name = "vehicleType")]
        public int? VehicleType { get; set; }
        [GraphQL(Name = "vehicleMode")]
        public Mode? VehicleMode { get; set; } // Manually set to nullable as nullable enum value is used here, not detectable in schema
        [GraphQL(Name = "platformCode")]
        public string PlatformCode { get; set; }
        [GraphQL(Name = "cluster")]
        public Cluster Cluster { get; set; }
        [GraphQL(Name = "stops")]
        public List<Stop> Stops { get; set; }
        [GraphQL(Name = "routes")]
        public List<Route> Routes { get; set; }
        [GraphQL(Name = "patterns")]
        public List<Pattern> Patterns { get; set; }
        [GraphQL(Name = "transfers")]
        public List<StopAtDistance> Transfers { get; set; }
        [GraphQL(Name = "stoptimesForServiceDate")]
        public List<StoptimesInPattern> StoptimesForServiceDate { get; set; }
        [GraphQL(Name = "stoptimesForPatterns")]
        public List<StoptimesInPattern> StoptimesForPatterns { get; set; }
        [GraphQL(Name = "stoptimesWithoutPatterns")]
        public List<Stoptime> StoptimesWithoutPatterns { get; set; }
        [GraphQL(Name = "alerts")]
        public List<Alert> Alerts { get; set; }
    }
}