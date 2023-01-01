namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "placeAtDistance")]
    public partial class PlaceAtDistance : Node
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "place")]
        public PlaceInterface Place { get; set; }
        [GraphQL(Name = "distance")]
        public int? Distance { get; set; }
    }
}