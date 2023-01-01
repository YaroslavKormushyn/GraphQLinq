namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "PageInfo")]
    public partial class PageInfo
    {
        [GraphQL(Name = "hasNextPage")]
        public bool HasNextPage { get; set; }
        [GraphQL(Name = "hasPreviousPage")]
        public bool HasPreviousPage { get; set; }
        [GraphQL(Name = "startCursor")]
        public string StartCursor { get; set; }
        [GraphQL(Name = "endCursor")]
        public string EndCursor { get; set; }
    }
}