using System;

namespace GraphQLinq.Client.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class GraphQLMethodAttribute : Attribute
    {
        public string Name { get; set; }
    }
}