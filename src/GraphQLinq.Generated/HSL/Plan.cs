namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Plan")]
    public partial class Plan
    {
        [GraphQL(Name = "date")]
        public long? Date { get; set; }
        [GraphQL(Name = "from")]
        public Place From { get; set; }
        [GraphQL(Name = "to")]
        public Place To { get; set; }
        [GraphQL(Name = "itineraries")]
        public List<Itinerary> Itineraries { get; set; }
        [GraphQL(Name = "messageEnums")]
        public List<string> MessageEnums { get; set; }
        [GraphQL(Name = "messageStrings")]
        public List<string> MessageStrings { get; set; }
        [GraphQL(Name = "debugOutput")]
        public DebugOutput DebugOutput { get; set; }
    }
}