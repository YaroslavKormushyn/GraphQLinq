namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "DepartureRow")]
    public partial class DepartureRow : Node, PlaceInterface
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "stop")]
        public Stop Stop { get; set; }
        [GraphQL(Name = "lat")]
        public float? Lat { get; set; }
        [GraphQL(Name = "lon")]
        public float? Lon { get; set; }
        [GraphQL(Name = "pattern")]
        public Pattern Pattern { get; set; }
        [GraphQL(Name = "stoptimes")]
        public List<Stoptime> Stoptimes { get; set; }
    }
}