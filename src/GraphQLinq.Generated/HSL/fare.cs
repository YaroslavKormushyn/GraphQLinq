namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "fare")]
    public partial class Fare
    {
        [GraphQL(Name = "type")]
        public string Type { get; set; }
        [GraphQL(Name = "currency")]
        public string Currency { get; set; }
        [GraphQL(Name = "cents")]
        public int? Cents { get; set; }
        [GraphQL(Name = "components")]
        public List<FareComponent> Components { get; set; }
    }
}