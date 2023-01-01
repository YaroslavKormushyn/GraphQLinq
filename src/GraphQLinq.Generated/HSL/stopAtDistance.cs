namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "stopAtDistance")]
    public partial class StopAtDistance : Node
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "stop")]
        public Stop Stop { get; set; }
        [GraphQL(Name = "distance")]
        public int? Distance { get; set; }
    }
}