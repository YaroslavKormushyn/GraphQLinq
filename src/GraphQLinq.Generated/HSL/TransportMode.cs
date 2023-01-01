namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "TransportMode")]
    public partial class TransportMode
    {
        [GraphQL(Name = "mode")]
        public Mode Mode { get; set; }
        [GraphQL(Name = "qualifier")]
        public Qualifier Qualifier { get; set; }
    }
}