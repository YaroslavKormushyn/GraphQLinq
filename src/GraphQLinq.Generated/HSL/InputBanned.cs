namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "InputBanned")]
    public partial class InputBanned
    {
        [GraphQL(Name = "routes")]
        public string Routes { get; set; }
        [GraphQL(Name = "agencies")]
        public string Agencies { get; set; }
        [GraphQL(Name = "trips")]
        public string Trips { get; set; }
        [GraphQL(Name = "stops")]
        public string Stops { get; set; }
        [GraphQL(Name = "stopsHard")]
        public string StopsHard { get; set; }
    }
}