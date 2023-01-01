namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Feed")]
    public partial class Feed
    {
        [GraphQL(Name = "feedId")]
        public string FeedId { get; set; }
        [GraphQL(Name = "agencies")]
        public List<Agency> Agencies { get; set; }
    }
}