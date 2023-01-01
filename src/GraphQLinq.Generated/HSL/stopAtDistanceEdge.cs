namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "stopAtDistanceEdge")]
    public partial class StopAtDistanceEdge
    {
        [GraphQL(Name = "node")]
        public StopAtDistance Node { get; set; }
        [GraphQL(Name = "cursor")]
        public string Cursor { get; set; }
    }
}