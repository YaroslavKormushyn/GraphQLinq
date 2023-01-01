namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "debugOutput")]
    public partial class DebugOutput
    {
        [GraphQL(Name = "totalTime")]
        public long? TotalTime { get; set; }
        [GraphQL(Name = "pathCalculationTime")]
        public long? PathCalculationTime { get; set; }
        [GraphQL(Name = "precalculationTime")]
        public long? PrecalculationTime { get; set; }
        [GraphQL(Name = "renderingTime")]
        public long? RenderingTime { get; set; }
        [GraphQL(Name = "timedOut")]
        public bool? TimedOut { get; set; }
    }
}