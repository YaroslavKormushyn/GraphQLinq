namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "InputModeWeight")]
    public partial class InputModeWeight
    {
        [GraphQL(Name = "TRAM")]
        public float? TRAM { get; set; }
        [GraphQL(Name = "SUBWAY")]
        public float? SUBWAY { get; set; }
        [GraphQL(Name = "RAIL")]
        public float? RAIL { get; set; }
        [GraphQL(Name = "BUS")]
        public float? BUS { get; set; }
        [GraphQL(Name = "FERRY")]
        public float? FERRY { get; set; }
        [GraphQL(Name = "CABLE_CAR")]
        public float? CABLE_CAR { get; set; }
        [GraphQL(Name = "GONDOLA")]
        public float? GONDOLA { get; set; }
        [GraphQL(Name = "FUNICULAR")]
        public float? FUNICULAR { get; set; }
        [GraphQL(Name = "AIRPLANE")]
        public float? AIRPLANE { get; set; }
    }
}