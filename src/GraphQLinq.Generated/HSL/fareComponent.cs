namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "fareComponent")]
    public partial class FareComponent
    {
        [GraphQL(Name = "fareId")]
        public string FareId { get; set; }
        [GraphQL(Name = "currency")]
        public string Currency { get; set; }
        [GraphQL(Name = "cents")]
        public int? Cents { get; set; }
        [GraphQL(Name = "routes")]
        public List<Route> Routes { get; set; }
    }
}