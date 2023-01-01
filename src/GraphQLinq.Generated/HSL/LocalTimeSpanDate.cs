namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "LocalTimeSpanDate")]
    public partial class LocalTimeSpanDate
    {
        [GraphQL(Name = "timeSpans")]
        public List<LocalTimeSpan> TimeSpans { get; set; }
        [GraphQL(Name = "date")]
        public string Date { get; set; }
    }
}