namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "BikeRentalStation")]
    public partial class BikeRentalStation : Node, PlaceInterface
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "stationId")]
        public string StationId { get; set; }
        [GraphQL(Name = "name")]
        public string Name { get; set; }
        [GraphQL(Name = "bikesAvailable")]
        public int? BikesAvailable { get; set; }
        [GraphQL(Name = "spacesAvailable")]
        public int? SpacesAvailable { get; set; }
        [GraphQL(Name = "capacity")]
        public int? Capacity { get; set; }
        [GraphQL(Name = "state")]
        public string State { get; set; }
        [GraphQL(Name = "realtime")]
        public bool? Realtime { get; set; }
        [GraphQL(Name = "allowDropoff")]
        public bool? AllowDropoff { get; set; }
        [GraphQL(Name = "allowOverloading")]
        public bool? AllowOverloading { get; set; }
        [GraphQL(Name = "isFloatingBike")]
        public bool? IsFloatingBike { get; set; }
        [GraphQL(Name = "isCarStation")]
        public bool? IsCarStation { get; set; }
        [GraphQL(Name = "networks")]
        public List<string> Networks { get; set; }
        [GraphQL(Name = "lon")]
        public float? Lon { get; set; }
        [GraphQL(Name = "lat")]
        public float? Lat { get; set; }
    }
}