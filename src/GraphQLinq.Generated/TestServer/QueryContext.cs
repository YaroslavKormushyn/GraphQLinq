namespace TestServer
{
    using GraphQLinq;
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;
    using System.Net.Http;

    public partial class QueryContext : GraphContext
    {
        public QueryContext() : this("http://localhost:10000/graphql")
        {
        }

        public QueryContext(string baseUrl) : base(baseUrl, "")
        {
            SubQueryContext = new QuerySubQueryContext(this);
        }

        public QueryContext(HttpClient httpClient) : base(httpClient)
        {
            SubQueryContext = new QuerySubQueryContext(this);
        }

        public GraphItemQuery<User> UserTemporaryFixForNullable(int? id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<User>(parameterValues, "userTemporaryFixForNullable");
        }

        public GraphItemQuery<User> User(int id)
        {
            var parameterValues = new object[] { id };
            return BuildItemQuery<User>(parameterValues, "user");
        }

        public GraphItemQuery<User> FailUser()
        {
            var parameterValues = new object[] { };
            return BuildItemQuery<User>(parameterValues, "failUser");
        }
    }
}