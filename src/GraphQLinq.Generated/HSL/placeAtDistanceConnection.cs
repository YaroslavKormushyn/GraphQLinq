namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "placeAtDistanceConnection")]
    public partial class PlaceAtDistanceConnection
    {
        [GraphQL(Name = "edges")]
        public List<PlaceAtDistanceEdge> Edges { get; set; }
        [GraphQL(Name = "pageInfo")]
        public PageInfo PageInfo { get; set; }
    }
}