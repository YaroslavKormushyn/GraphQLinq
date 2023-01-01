namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "stopAtDistanceConnection")]
    public partial class StopAtDistanceConnection
    {
        [GraphQL(Name = "edges")]
        public List<StopAtDistanceEdge> Edges { get; set; }
        [GraphQL(Name = "pageInfo")]
        public PageInfo PageInfo { get; set; }
    }
}