using System;

namespace GraphQLinq.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GraphQLTypeAttribute : Attribute
    {
        public string Name { get; set; }
    }
}