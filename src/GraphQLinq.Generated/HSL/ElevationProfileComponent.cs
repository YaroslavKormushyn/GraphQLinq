namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "elevationProfileComponent")]
    public partial class ElevationProfileComponent
    {
        [GraphQL(Name = "distance")]
        public float? Distance { get; set; }
        [GraphQL(Name = "elevation")]
        public float? Elevation { get; set; }
    }
}