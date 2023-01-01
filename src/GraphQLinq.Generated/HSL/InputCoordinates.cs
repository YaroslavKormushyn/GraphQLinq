namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "InputCoordinates")]
    public partial class InputCoordinates
    {
        [GraphQL(Name = "lat")]
        public float Lat { get; set; }
        [GraphQL(Name = "lon")]
        public float Lon { get; set; }
        [GraphQL(Name = "address")]
        public string Address { get; set; }
        [GraphQL(Name = "locationSlack")]
        public int? LocationSlack { get; set; }
    }
}