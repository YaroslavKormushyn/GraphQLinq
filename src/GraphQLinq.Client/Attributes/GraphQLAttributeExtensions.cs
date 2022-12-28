using GraphQLinq.Client.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace PokeApiGraphQLinq.Client.Attributes
{
    public static class GraphQLAttributeExtensions
    {
        public static string GetGraphQLNameFromType(this Type type)
        {
            var t = type;
            var memberInfo = t;
            if (Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLTypeAttribute)) is GraphQLTypeAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMember(this Type type, string memberName)
        {
            var t = type;
            var memberInfo = t.GetProperty(memberName);
            if (memberInfo != null && Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLMemberAttribute)) is GraphQLMemberAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMethod(this Type type, string methodName)
        {
            var t = type;
            var memberInfo = t.GetMethod(methodName);
            if (memberInfo != null && Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLMethodAttribute)) is GraphQLMethodAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMember(this PropertyInfo memberInfo)
        {
            if (Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLMemberAttribute)) is GraphQLMemberAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMethod(this MemberInfo memberInfo)
        {
            if (Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLMethodAttribute)) is GraphQLMethodAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMethod(this MethodInfo memberInfo)
        {
            if (Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLMethodAttribute)) is GraphQLMethodAttribute attribute)
                return attribute.Name;
            return memberInfo.Name;
        }

        public static string GetGraphQLNameFromMember(this ParameterInfo memberInfo)
        {
            var t = memberInfo.ParameterType;
            if (Attribute.GetCustomAttribute(t, typeof(GraphQLMemberAttribute)) is GraphQLMemberAttribute attribute)
                return attribute.Name;
            else if (Attribute.GetCustomAttribute(t, typeof(GraphQLTypeAttribute)) is GraphQLTypeAttribute typeAttr)
                return typeAttr.Name;
            else if (Attribute.GetCustomAttribute(t, typeof(GraphQLMethodAttribute)) is GraphQLMethodAttribute methAttr)
                return methAttr.Name;
            return memberInfo.Name;
        }
    }
}
