using GraphQLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace GraphQLinq
{
    public class SubQueryContext // TODO naming
    {
        private GraphContext _context;

        public SubQueryContext(GraphContext context)
        {
            _context = context;
        }

        public GraphCollectionQuery<T> BuildCollectionQuery<T>(object[] parameterValues,
            [CallerMemberName] string queryName = null)
        {
            var arguments = BuildDictionary(parameterValues, queryName);
            return new GraphCollectionQuery<T, T>(_context, queryName, isSubQuery:true) { Arguments = arguments };
        }

        public GraphItemQuery<T> BuildItemQuery<T>(object[] parameterValues,
            [CallerMemberName] string queryName = null)
        {
            var arguments = BuildDictionary(parameterValues, queryName);
            return new GraphItemQuery<T, T>(_context, queryName, isSubQuery: true)
                { Arguments = arguments };
        }

        // TODO improve readability
        private Dictionary<string, (string alternateKey, object value)> BuildDictionary(object[] parameterValues, string queryName)
        {
            var parameters = GetType()
                .GetMethod(queryName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance)
                .GetParameters();

            var arguments = parameters.Zip(parameterValues, (info, value) => new { info.Name, Value = value })// TODO still don't like the alternate name stuff here
                .ToDictionary(arg =>  queryName + arg.Name,
                    arg => (arg.Name, arg.Value));
            return arguments;
        }

        public MethodInfo GetQueryContextMethod(string methodName)
        {
            return GetType().GetMethod(methodName,
                BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
        }
    }
}
