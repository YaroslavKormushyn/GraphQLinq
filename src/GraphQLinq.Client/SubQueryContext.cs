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
            return new GraphCollectionQuery<T, T>(_context, this, queryName, isSubQuery:true) { Arguments = arguments }; // TODO rework prefix
        }

        public GraphItemQuery<T> BuildItemQuery<T>(object[] parameterValues,
            [CallerMemberName] string queryName = null)
        {
            var arguments = BuildDictionary(parameterValues, queryName);
            return new GraphItemQuery<T, T>(_context, this, queryName, isSubQuery: true)
                { Arguments = arguments }; // TODO rework prefix
        }

        private Dictionary<string, (string alternateKey, object value)> BuildDictionary(object[] parameterValues, string queryName)
        {
            var parameters = GetType()
                .GetMethod(queryName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance)
                .GetParameters().Where(p => p.ParameterType != typeof(GraphContext)); // TODO FIX
            var arguments = parameters.Zip(parameterValues, (info, value) => new { info.Name, Value = value })
                .ToDictionary(arg =>  queryName + arg.Name,
                    arg => (arg.Name, arg.Value));
            return arguments;
        }

        // TODO Improve?
        public static MethodInfo GetQueryContextMethod(GraphContext context, string methodName)
        {
            return context.GetType().GetMethod(methodName,
                BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
        }
    }
}
