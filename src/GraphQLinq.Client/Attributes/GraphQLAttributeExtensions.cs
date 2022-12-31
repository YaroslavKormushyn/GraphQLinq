using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace GraphQLinq.Attributes
{
    public static class GraphQLAttributeExtensions
    {
        public static string GetGraphQLNameFromType(this Type type)
        {
            var t = type;
            var memberInfo = t;
            if (Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLAttribute)) is GraphQLAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMember(this Type type, string memberName)
        {
            var t = type;
            var memberInfo = t.GetProperty(memberName);
            if (memberInfo != null && Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLAttribute)) is GraphQLAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMethod(this Type type, string methodName)
        {
            var t = type;
            var memberInfo = t.GetMethod(methodName);
            if (memberInfo != null && Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLAttribute)) is GraphQLAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMember(this PropertyInfo memberInfo)
        {
            if (Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLAttribute)) is GraphQLAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMethod(this MemberInfo memberInfo)
        {
            if (Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLAttribute)) is GraphQLAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMethod(this MethodInfo memberInfo)
        {
            if (Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLAttribute)) is GraphQLAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMember(this ParameterInfo memberInfo)
        {
            var t = memberInfo.ParameterType;
            if (Attribute.GetCustomAttribute(t, typeof(GraphQLAttribute)) is GraphQLAttribute attribute)
                return attribute.Name;
          
            return memberInfo.Name;
        }
    }
}
