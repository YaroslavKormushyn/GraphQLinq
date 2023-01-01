namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "StoptimesInPattern")]
    public partial class StoptimesInPattern
    {
        [GraphQL(Name = "pattern")]
        public Pattern Pattern { get; set; }
        [GraphQL(Name = "stoptimes")]
        public List<Stoptime> Stoptimes { get; set; }
    }
}