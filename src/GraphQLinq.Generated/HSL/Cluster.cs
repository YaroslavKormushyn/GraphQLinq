namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Cluster")]
    public partial class Cluster : Node
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "gtfsId")]
        public string GtfsId { get; set; }
        [GraphQL(Name = "name")]
        public string Name { get; set; }
        [GraphQL(Name = "lat")]
        public float Lat { get; set; }
        [GraphQL(Name = "lon")]
        public float Lon { get; set; }
        [GraphQL(Name = "stops")]
        public List<Stop> Stops { get; set; }
    }
}