namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "serviceTimeRange")]
    public partial class ServiceTimeRange
    {
        [GraphQL(Name = "start")]
        public long? Start { get; set; }
        [GraphQL(Name = "end")]
        public long? End { get; set; }
    }
}