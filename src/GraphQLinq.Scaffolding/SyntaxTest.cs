using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PokeApiGraphQLinq.Scaffolding
{
    internal class SyntaxTest
    {
        public SyntaxTest()
        {
            var ps = new Pokemon_v2_pokemonspecies();

            //var x = ps.Get
        }
    }

    [GraphQLType(Name = "pokemon_v2_pokemonspecies")]
    public partial class Pokemon_v2_pokemonspecies
    {
        public int? Base_happiness { get; set; }

        [GraphQLMember(Name = "pokemon_v2_pokemonspecies")]
        public List<Pokemon_v2_pokemonspecies> Pokemon_v2_pokemonspeciesProp { get; set; }
        public Pokemon_v2_pokemonspecies Pokemon_v2_pokemonspecy { get; set; }


        public Pokemon_v2_pokemonspecies()
        {
            
        }

    }

    public static class QExt
    {
        [GraphQLMethodAttribute(Name = "pokemon_v2_pokemonspecies")]
        public static List<Pokemon_v2_pokemonspecies> Pokemon_v2_pokemonspecies(this Pokemon_v2_pokemonspecies pokemon_v2_pokemonspecies)
        {
            var classType = typeof(Pokemon_v2_pokemonspecies);
            var memberInfo =
                classType.GetProperty(nameof(Scaffolding.Pokemon_v2_pokemonspecies.Pokemon_v2_pokemonspeciesProp));

            if (memberInfo != null)
            {
                var memberAttribute = Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLMemberAttribute)) as GraphQLMemberAttribute;
            }

            return pokemon_v2_pokemonspecies.Pokemon_v2_pokemonspeciesProp;
        }    
        /*public static string GetGraphQLNameFromType(Type type)
        {
            if (Attribute.GetCustomAttribute(type, typeof(GraphQLTypeAttribute)) is GraphQLTypeAttribute attribute)
                return attribute.Name;
            else
            {
                return type.Name;
            }
        }*/

        public static string GetGraphQLNameFromType(Type type)
        {
            if (Attribute.GetCustomAttribute(type, typeof(GraphQLTypeAttribute)) is GraphQLTypeAttribute attribute)
                return attribute.Name;
            else
            {
                return type.Name;
            }
        }

        public static string GetGraphQLNameFromMember(Type type, string memberName)
        {
            var propertyInfo =
                type.GetProperty(memberName);
            if (propertyInfo != null && Attribute.GetCustomAttribute(propertyInfo, typeof(GraphQLMemberAttribute)) is GraphQLMemberAttribute attribute)
                return attribute.Name;
            else
            {
                return memberName;
            }
        }

        public static string GetGraphQLNameFromMethod(Type type, string methodName)
        {
            var memberInfo =
                type.GetMethod(methodName);
            if (memberInfo != null && Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLMethodAttribute)) is GraphQLMethodAttribute attribute)
                return attribute.Name;
            else
            {
                return methodName;
            }
        }

        public static string GetGraphQLNameFromType(this object type)
        {
            var t = type.GetType();

            if (Attribute.GetCustomAttribute(t, typeof(GraphQLTypeAttribute)) is GraphQLTypeAttribute attribute)
                return attribute.Name;
            else
            {
                return t.Name;
            }
        }

        public static string GetGraphQLNameFromMember(this object type, [CallerMemberName] string memberName = "")
        {
            var t = type.GetType();

            var memberInfo =
                t.GetProperty(memberName);
            if (memberInfo != null && Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLMemberAttribute)) is GraphQLMemberAttribute attribute)
                return attribute.Name;
            else
            {
                return memberName;
            }
        }

        public static string GetGraphQLNameFromMethod(this object type, [CallerMemberName] string methodName = "")
        {
            var t = type.GetType();

            var memberInfo =
                t.GetMethod(methodName);
            if (memberInfo != null && Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLMethodAttribute)) is GraphQLMethodAttribute attribute)
                return attribute.Name;
            else
            {
                return methodName;
            }
        }

        public static string GetGraphQLNameFromMember(this PropertyInfo propertyInfo)
        {
            if (Attribute.GetCustomAttribute(propertyInfo, typeof(GraphQLMemberAttribute)) is GraphQLMemberAttribute attribute)
                return attribute.Name;
            else
            {
                return propertyInfo.Name;
            }
        }

        public static string GetGraphQLNameFromMethod(this MemberInfo memberInfo)
        {
            if (Attribute.GetCustomAttribute(memberInfo, typeof(GraphQLMethodAttribute)) is GraphQLMethodAttribute attribute)
                return attribute.Name;
            else
            {
                return memberInfo.Name;
            }
        }


    }




        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class GraphQLMethodAttribute : Attribute
        {
            public string Name { get; set; }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public class GraphQLMemberAttribute : Attribute
        {
            public string Name { get; set; }
        }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GraphQLTypeAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
