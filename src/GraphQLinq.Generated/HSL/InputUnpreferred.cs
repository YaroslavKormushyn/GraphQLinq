namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "InputUnpreferred")]
    public partial class InputUnpreferred
    {
        [GraphQL(Name = "routes")]
        public string Routes { get; set; }
        [GraphQL(Name = "agencies")]
        public string Agencies { get; set; }
        [GraphQL(Name = "useUnpreferredRoutesPenalty")]
        public int? UseUnpreferredRoutesPenalty { get; set; }
    }
}