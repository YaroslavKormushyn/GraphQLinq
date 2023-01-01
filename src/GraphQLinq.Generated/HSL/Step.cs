namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "step")]
    public partial class Step
    {
        [GraphQL(Name = "distance")]
        public float? Distance { get; set; }
        [GraphQL(Name = "lon")]
        public float? Lon { get; set; }
        [GraphQL(Name = "lat")]
        public float? Lat { get; set; }
        [GraphQL(Name = "elevationProfile")]
        public List<ElevationProfileComponent> ElevationProfile { get; set; }
    }
}