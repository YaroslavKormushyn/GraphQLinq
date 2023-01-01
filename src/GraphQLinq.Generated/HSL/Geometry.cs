namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Geometry")]
    public partial class Geometry
    {
        [GraphQL(Name = "length")]
        public int? Length { get; set; }
        [GraphQL(Name = "points")]
        public string Points { get; set; }
    }
}