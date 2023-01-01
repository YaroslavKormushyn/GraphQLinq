namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Coordinates")]
    public partial class Coordinates
    {
        [GraphQL(Name = "lat")]
        public float? Lat { get; set; }
        [GraphQL(Name = "lon")]
        public float? Lon { get; set; }
    }
}