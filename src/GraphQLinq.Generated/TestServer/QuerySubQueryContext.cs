namespace TestServer
{
    using GraphQLinq;
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    public class QuerySubQueryContext : SubQueryContext
    {
        public QuerySubQueryContext(GraphContext context) : base(context)
        {
        }
    }
}