namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Agency")]
    public partial class Agency : Node
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "gtfsId")]
        public string GtfsId { get; set; }
        [GraphQL(Name = "name")]
        public string Name { get; set; }
        [GraphQL(Name = "url")]
        public string Url { get; set; }
        [GraphQL(Name = "timezone")]
        public string Timezone { get; set; }
        [GraphQL(Name = "lang")]
        public string Lang { get; set; }
        [GraphQL(Name = "phone")]
        public string Phone { get; set; }
        [GraphQL(Name = "fareUrl")]
        public string FareUrl { get; set; }
        [GraphQL(Name = "routes")]
        public List<Route> Routes { get; set; }
        [GraphQL(Name = "alerts")]
        public List<Alert> Alerts { get; set; }
    }
}