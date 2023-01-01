namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "InputPreferred")]
    public partial class InputPreferred
    {
        [GraphQL(Name = "routes")]
        public string Routes { get; set; }
        [GraphQL(Name = "agencies")]
        public string Agencies { get; set; }
        [GraphQL(Name = "otherThanPreferredRoutesPenalty")]
        public int? OtherThanPreferredRoutesPenalty { get; set; }
    }
}