namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Pattern")]
    public partial class Pattern : Node
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "route")]
        public Route Route { get; set; }
        [GraphQL(Name = "directionId")]
        public int? DirectionId { get; set; }
        [GraphQL(Name = "name")]
        public string Name { get; set; }
        [GraphQL(Name = "code")]
        public string Code { get; set; }
        [GraphQL(Name = "headsign")]
        public string Headsign { get; set; }
        [GraphQL(Name = "trips")]
        public List<Trip> Trips { get; set; }
        [GraphQL(Name = "tripsForDate")]
        public List<Trip> TripsForDate { get; set; }
        [GraphQL(Name = "stops")]
        public List<Stop> Stops { get; set; }
        [GraphQL(Name = "geometry")]
        public List<Coordinates> Geometry { get; set; }
        [GraphQL(Name = "patternGeometry")]
        public Geometry PatternGeometry { get; set; }
        [GraphQL(Name = "semanticHash")]
        public string SemanticHash { get; set; }
        [GraphQL(Name = "alerts")]
        public List<Alert> Alerts { get; set; }
    }
}