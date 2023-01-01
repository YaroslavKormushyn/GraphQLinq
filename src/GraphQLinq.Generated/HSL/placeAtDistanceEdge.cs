namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "placeAtDistanceEdge")]
    public partial class PlaceAtDistanceEdge
    {
        [GraphQL(Name = "node")]
        public PlaceAtDistance Node { get; set; }
        [GraphQL(Name = "cursor")]
        public string Cursor { get; set; }
    }
}