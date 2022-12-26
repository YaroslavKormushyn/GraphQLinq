using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeApiGraphQLinq.Scaffolding
{
    internal class SyntaxTest
    {

    }

    [GraphQLType(Name = "pokemon_v2_pokemonspecies")]
    public partial class Pokemon_v2_pokemonspecies
    {
        public int? Base_happiness { get; set; }

        [GraphQLMember(Name = "pokemon_v2_pokemonspecies")]
        public List<Pokemon_v2_pokemonspecies> Pokemon_v2_pokemonspeciesMember { get; set; }
        public Pokemon_v2_pokemonspecies Pokemon_v2_pokemonspecy { get; set; }
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
