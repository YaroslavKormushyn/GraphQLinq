namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Route")]
    public partial class Route : Node
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "gtfsId")]
        public string GtfsId { get; set; }
        [GraphQL(Name = "agency")]
        public Agency Agency { get; set; }
        [GraphQL(Name = "shortName")]
        public string ShortName { get; set; }
        [GraphQL(Name = "longName")]
        public string LongName { get; set; }
        [GraphQL(Name = "mode")]
        public Mode Mode { get; set; }
        [GraphQL(Name = "type")]
        public int? Type { get; set; }
        [GraphQL(Name = "desc")]
        public string Desc { get; set; }
        [GraphQL(Name = "url")]
        public string Url { get; set; }
        [GraphQL(Name = "color")]
        public string Color { get; set; }
        [GraphQL(Name = "textColor")]
        public string TextColor { get; set; }
        [GraphQL(Name = "bikesAllowed")]
        public BikesAllowed BikesAllowed { get; set; }
        [GraphQL(Name = "patterns")]
        public List<Pattern> Patterns { get; set; }
        [GraphQL(Name = "stops")]
        public List<Stop> Stops { get; set; }
        [GraphQL(Name = "trips")]
        public List<Trip> Trips { get; set; }
        [GraphQL(Name = "alerts")]
        public List<Alert> Alerts { get; set; }
    }
}