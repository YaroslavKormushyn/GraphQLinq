namespace TestServer
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "User")]
    public partial class User
    {
        [GraphQL(Name = "firstName")]
        public string FirstName { get; set; }
        [GraphQL(Name = "lastName")]
        public string LastName { get; set; }
    }
}