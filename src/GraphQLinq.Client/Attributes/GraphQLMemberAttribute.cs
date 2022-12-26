using System;

namespace GraphQLinq.Client.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GraphQLMemberAttribute : Attribute
    {
        public string Name { get; set; }
    }
}