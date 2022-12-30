using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace GraphQLinq
{
    public abstract class GraphQuery
    {
        public Dictionary<string, (string alternateKey, object value)> Arguments { get; set; } = new Dictionary<string, (string, object)>();
        public abstract string Query { get; }
    }

    public abstract class GraphQuery<T> : GraphQuery
    {
        public readonly GraphContext context;
        public readonly SubQueryContext subQueryContext;
        private readonly bool _isSubQuery;
        private Lazy<GraphQLQuery> lazyQuery;
        private readonly GraphQueryBuilder<T> queryBuilder = new GraphQueryBuilder<T>();
        internal string QueryName { get; private set; }
        internal LambdaExpression Selector { get; private set; }
        internal List<IncludeDetails> Includes { get; private set; } = new List<IncludeDetails>();


        internal GraphQuery(GraphContext graphContext, string queryName, bool isSubQuery)
        {
            QueryName = queryName;
            context = graphContext;
            _isSubQuery = isSubQuery;

            lazyQuery = new Lazy<GraphQLQuery>(() => isSubQuery ? queryBuilder.BuildSubQuery(this, Includes): queryBuilder.BuildQuery(this, Includes)); // TODO is subquery
        }

        public override string ToString()
        {
            return lazyQuery.Value.FullQuery;
        }

        public override string Query => lazyQuery.Value.Query;

        public IReadOnlyDictionary<string, object> QueryVariables => lazyQuery.Value.Variables;

        protected GraphQuery<TR> Clone<TR>()
        {
            var genericQueryType = GetType().GetGenericTypeDefinition();
            var genericArguments = GetType().GetGenericArguments();
            var cloneType = genericQueryType.MakeGenericType(typeof(TR), genericArguments[1]);

            var instance = (GraphQuery<TR>)Activator.CreateInstance(cloneType, context, QueryName, _isSubQuery);

            instance.Arguments = Arguments;
            instance.Selector = Selector;
            instance.Includes = Includes.ToList();

            return instance;
        }

        internal static IncludeDetails ParseIncludePath(Expression expression)
        {
            string path = null;
            var withoutConvert = expression.RemoveConvert(); // Removes boxing
            var memberExpression = withoutConvert as MemberExpression;
            var callExpression = withoutConvert as MethodCallExpression;

            if (memberExpression != null)
            {
                var parentPath = ParseIncludePath(memberExpression.Expression);
                if (parentPath == null)
                {
                    return null;
                }

                var thisPart = memberExpression.Member.Name;
                path = parentPath.Path == null ? thisPart : (parentPath.Path + "." + thisPart);
            }
            else if (callExpression != null)
            {
                if (callExpression.Method.Name == "Select" && callExpression.Arguments.Count == 2)
                {
                    var parentPath = ParseIncludePath(callExpression.Arguments[0]);
                    if (parentPath == null)
                    {
                        return null;
                    }

                    if (parentPath.Path != null)
                    {
                        var subExpression = callExpression.Arguments[1] as LambdaExpression;
                        if (subExpression != null)
                        {
                            var thisPath = ParseIncludePath(subExpression.Body);
                            if (thisPath == null)
                            {
                                return null;
                            }

                            if (thisPath.Path != null)
                            {
                                path = parentPath.Path + "." + thisPath.Path;
                                var result = new IncludeDetails(parentPath.MethodIncludes.Union(thisPath.MethodIncludes)) { Path = path };

                                return result;
                            }
                        }
                    }
                }
                if (callExpression.Method.DeclaringType?.Name == "QueryExtensions")
                {
                    var parentPath = ParseIncludePath(callExpression.Arguments[0]);
                    if (parentPath == null)
                    {
                        return null;
                    }

                    path = parentPath.Path == null ? callExpression.Method.Name : parentPath.Path + "." + callExpression.Method.Name;

                    var arguments = callExpression.Arguments.Zip(callExpression.Method.GetParameters(), (argument, parameter) => new
                    {
                        Argument = argument,
                        parameter.Name
                    }).Skip(1).ToDictionary(arg => arg.Name, arg => ("", (arg.Argument as ConstantExpression).Value));

                    var result = new IncludeDetails { Path = path };
                    result.MethodIncludes.Add(new IncludeMethodDetails
                    {
                        Method = callExpression.Method,
                        Parameters = arguments
                    });
                    result.MethodIncludes.AddRange(parentPath.MethodIncludes);
                    return result;
                }

                return null;
            }

            return new IncludeDetails { Path = path };
        }

        protected GraphQuery<T> BuildInclude<TProperty>(Expression<Func<T, TProperty>> path)
        {
            var include = ParseIncludePath(path.Body);
            if (include?.Path == null)
            {
                throw new ArgumentException("Invalid Include Path Expression", nameof(path));
            }

            var graphQuery = Clone<T>();
            graphQuery.Includes.Add(include);

            return graphQuery;
        }

        protected GraphQuery<TResult> BuildSelect<TResult>(Expression<Func<T, TResult>> resultSelector)
        {
            if (resultSelector.NodeType != ExpressionType.Lambda)
            {
                throw new ArgumentException($"{resultSelector} must be lambda expression", nameof(resultSelector));
            }

            var graphQuery = Clone<TResult>();
            graphQuery.Selector = resultSelector;

            return graphQuery;
        }

        internal GraphQueryExecutor<T, TSource> BuildExecutor<TSource>(QueryType queryType)
        {
            var query = lazyQuery.Value;

            var mapper = (Func<TSource, T>)Selector?.Compile();

            return new GraphQueryExecutor<T, TSource>(context, query.FullQuery, queryType, mapper);
        }
    }

    public abstract class GraphItemQuery<T> : GraphQuery<T>
    {
        protected GraphItemQuery(GraphContext graphContext, string queryName, bool isSubQuery) : base(graphContext, queryName, isSubQuery) { }

        public GraphItemQuery<T> Include<TProperty>(Expression<Func<T, TProperty>> path)
        {
            return (GraphItemQuery<T>)BuildInclude(path);
        }

        public GraphItemQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> resultSelector)
        {
            return (GraphItemQuery<TResult>)BuildSelect(resultSelector);
        }

        public abstract Task<T> ToItem();
    }

    public abstract class GraphCollectionQuery<T> : GraphQuery<T>
    {
        protected GraphCollectionQuery(GraphContext graphContext, string queryName, bool isSubQuery) : base(graphContext, queryName, isSubQuery) { }

        public abstract Task<IEnumerable<T>> ToEnumerable();

        public GraphCollectionQuery<T> Include<TProperty>(Expression<Func<T, TProperty>> path)
        {
            return (GraphCollectionQuery<T>)BuildInclude(path);
        }

        public GraphCollectionQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> resultSelector)
        {
            return (GraphCollectionQuery<TResult>)BuildSelect(resultSelector);
        }
    }

    public class GraphItemQuery<T, TSource> : GraphItemQuery<T>
    {
        public GraphItemQuery(GraphContext graphContext,  string queryName, bool isSubQuery = false) : base(graphContext, queryName, isSubQuery)
        {
        }

        public override async Task<T> ToItem()
        {
            var (item, _) = await BuildExecutor<TSource>(QueryType.Item).Execute();
            return item;
        }
    }

    public class GraphCollectionQuery<T, TSource> : GraphCollectionQuery<T>
    {
        public GraphCollectionQuery(GraphContext graphContext, string queryName, bool isSubQuery = false) : base(graphContext, queryName, isSubQuery)
        {
        }

        public override async Task<IEnumerable<T>> ToEnumerable()
        {
            var (_, enumerable) = await BuildExecutor<TSource>(QueryType.Collection).Execute();
            return enumerable;
        }
    }

    internal enum QueryType
    {
        Item,
        Collection
    }

    class IncludeDetails
    {
        public IncludeDetails()
        {

        }

        public IncludeDetails(IEnumerable<IncludeMethodDetails> methodIncludes)
        {
            MethodIncludes = methodIncludes.ToList();
        }

        public string Path { get; set; }
        public List<IncludeMethodDetails> MethodIncludes { get; } = new List<IncludeMethodDetails>();
    }

    class IncludeMethodDetails
    {
        public MethodInfo Method { get; set; }
        public Dictionary<string, (string alternateKey, object value)> Parameters { get; set; }

        public override string ToString()
        {
            return Method.Name;
        }
    }
}