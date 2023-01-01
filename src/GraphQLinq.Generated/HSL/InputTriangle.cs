namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "InputTriangle")]
    public partial class InputTriangle
    {
        [GraphQL(Name = "safetyFactor")]
        public float? SafetyFactor { get; set; }
        [GraphQL(Name = "slopeFactor")]
        public float? SlopeFactor { get; set; }
        [GraphQL(Name = "timeFactor")]
        public float? TimeFactor { get; set; }
    }
}