namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "TicketType")]
    public partial class TicketType : Node
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "fareId")]
        public string FareId { get; set; }
        [GraphQL(Name = "price")]
        public float? Price { get; set; }
        [GraphQL(Name = "currency")]
        public string Currency { get; set; }
        [GraphQL(Name = "zones")]
        public List<string> Zones { get; set; }
    }
}