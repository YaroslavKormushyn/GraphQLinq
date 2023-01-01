namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "LocalTimeSpan")]
    public partial class LocalTimeSpan
    {
        [GraphQL(Name = "from")]
        public int From { get; set; }
        [GraphQL(Name = "to")]
        public int To { get; set; }
    }
}