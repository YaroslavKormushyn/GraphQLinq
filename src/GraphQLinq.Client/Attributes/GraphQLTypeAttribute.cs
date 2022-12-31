using System;

namespace GraphQLinq.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class GraphQLAttribute : Attribute
    {
        public string Name { get; set; }
    }
}