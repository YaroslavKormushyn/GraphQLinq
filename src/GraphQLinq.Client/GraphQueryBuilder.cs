using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQLinq
{
    class GraphQueryBuilder<T>
    {
        private const string QueryTemplate = @"query {0} {{ {1}: {2} {3} {{ {4} }} }}";
        private const string ScalarQueryTemplate = @"query {0} {{ {1}: {2} {3} {4} }}";

        internal const string ResultAlias = "result";
        internal const string QueryExtensionsTypeName = "QueryExtensions";

        private static GraphContext _context;
        private static List<KeyValuePair<string, object>> additionalArguments;

        public GraphQLQuery BuildQuery(GraphQuery<T> graphQuery, List<IncludeDetails> includes)
        {
            _context = graphQuery.context;
            var selectClause = "";

            var passedArguments = graphQuery.Arguments.Where(pair => pair.Value != null).ToList();
            var queryVariables = passedArguments.ToDictionary(pair => pair.Key, pair => pair.Value);

            if (graphQuery.Selector != null)
            {
                var body = graphQuery.Selector.Body;

                var padding = new string(' ', 4);

                var fields = new List<string>();

                switch (body)
                {
                    case MemberExpression memberExpression:
                        var member = memberExpression.Member;
                        selectClause = BuildMemberAccessSelectClause(body, selectClause, padding, member.Name);
                        break;

                    case NewExpression newExpression:
                        foreach (var argument in newExpression.Arguments.OfType<MemberExpression>())
                        {
                            var selectField = BuildMemberAccessSelectClause(argument, selectClause, padding, argument.Member.Name);
                            fields.Add(selectField);
                        }

                        foreach (var argument in newExpression.Arguments.OfType<MethodCallExpression>())
                        {
                            var selectQuery = BuildMemberAccessSelectClauseForNestedQueries(argument, selectClause, padding, out _, graphQuery.context);
                            fields.Add(selectQuery);
                        }
                        selectClause = string.Join(Environment.NewLine, fields);
                        break;

                    case MemberInitExpression memberInitExpression:
                        foreach (var argument in memberInitExpression.Bindings.OfType<MemberAssignment>())
                        {
                            var selectField = BuildMemberAccessSelectClause(argument.Expression, selectClause, padding, argument.Member.Name);
                            fields.Add(selectField);
                        }
                        selectClause = string.Join(Environment.NewLine, fields);
                        break;
                    default:
                        throw new NotSupportedException($"Selector of type {body.NodeType} is not implemented yet");
                }
            }
            else
            {
                var select = BuildSelectClauseForType(typeof(T), includes);
                selectClause = select.SelectClause;

                foreach (var item in select.IncludeArguments)
                {
                    queryVariables.Add(item.Key, item.Value);
                }
            }

            var isScalarQuery = string.IsNullOrEmpty(selectClause);
            selectClause = Environment.NewLine + selectClause + Environment.NewLine;

            var queryParameters = passedArguments.Any() ? $"({string.Join(", ", passedArguments.Select(pair => $"{pair.Key}: ${pair.Key}"))})" : "";
            var queryParameterTypes = queryVariables.Any() ? $"({string.Join(", ", queryVariables.Select(pair => $"${pair.Key}: {pair.Value.GetType().ToGraphQlType()}"))})" : "";

            var graphQLQuery = string.Format(isScalarQuery ? ScalarQueryTemplate : QueryTemplate, queryParameterTypes, ResultAlias, graphQuery.QueryName, queryParameters, selectClause);

            var dictionary = new Dictionary<string, object> { { "query", graphQLQuery }, { "variables", queryVariables } };

            var json = JsonSerializer.Serialize(dictionary, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            });

            return new GraphQLQuery(graphQLQuery, queryVariables, json);
        }

        private static string BuildMemberAccessSelectClause(Expression body, string selectClause, string padding, string alias)
        {
            if (body is MemberExpression memberExpression)
            {
                var member = memberExpression.Member as PropertyInfo;

                if (member != null)
                {
                    if (string.IsNullOrEmpty(selectClause))
                    {
                        selectClause = $"{padding}{alias}: {member.Name.ToCamelCase()}";

                        if (!member.PropertyType.GetTypeOrListType().IsValueTypeOrString())
                        {
                            var fieldForProperty = BuildSelectClauseForType(member.PropertyType.GetTypeOrListType(), 3);
                            selectClause = $"{selectClause} {{{Environment.NewLine}{fieldForProperty}{Environment.NewLine}{padding}}}";
                        }
                    }
                    else
                    {
                        selectClause = $"{member.Name.ToCamelCase()} {{ {Environment.NewLine}{selectClause}{Environment.NewLine}}}";
                    }
                    return BuildMemberAccessSelectClause(memberExpression.Expression, selectClause, padding, "");
                }
                return selectClause;
            }
            return selectClause;
        }

        private static string BuildMemberAccessSelectClauseForNestedQueries(Expression body, string selectClause,
            string padding, out string aliasName, GraphContext context = null)
        {
            switch (body)
            {
                case MethodCallExpression methodCallExpression when methodCallExpression.Method.Name.Equals("Select"):
                    var sMethod =
                        BuildMemberAccessSelectClauseForNestedQueriesMethodCall(methodCallExpression, "", padding, out var alias);
                    aliasName = alias;
                    selectClause = $"{selectClause}{(!string.IsNullOrEmpty(selectClause) ? Environment.NewLine : string.Empty)}{sMethod}";
                    return selectClause;
                case NewExpression newExpression:
                    var nMethod =
                        BuildMemberAccessSelectClauseForNestedQueriesNew(newExpression, "", padding, out var nAlias);
                    aliasName = nAlias;
                    selectClause = $"{selectClause}{(!string.IsNullOrEmpty(selectClause) ? Environment.NewLine : string.Empty)}{nMethod}";
                    return selectClause;
                case MemberExpression memberExpression:
                    var s=
                        BuildMemberAccessSelectClauseForNestedQueriesMember(memberExpression, "", padding, memberExpression.Member.Name);
                    aliasName = memberExpression.Member.Name;
                    selectClause = $"{selectClause}{(!string.IsNullOrEmpty(selectClause) ? Environment.NewLine : string.Empty)}{s}";
                    return selectClause;
                case MethodCallExpression methodCallExpression when methodCallExpression.Method.ReflectedType.Name.Equals(QueryExtensionsTypeName):
                    //return BuildCollectionQuery<Pokemon_v2_pokemonspeciesType>(parameterValues, "pokemon_v2_pokemonspecies");
                    //return GraphItemQuery<Pokemon_v2_pokemonspeciesType>(parameterValues, "pokemon_v2_pokemonspecies");
                    var methodName = methodCallExpression.Method.Name;
                    var parameters = methodCallExpression.Arguments;

                    Type genericType;
                    var returnType = methodCallExpression.Method.ReturnType;
                    var isCollection = returnType.GetInterface(nameof(IEnumerable)) != null;

                    if (returnType.IsGenericType)
                    {
                        genericType = returnType.GetGenericTypeDefinition();

                        if (isCollection)
                        {
                            genericType = returnType.GetGenericArguments().First();
                        }
                    }
                    else
                    {
                        genericType = returnType;
                    }

                    var parameterObjects = new List<object>();

                    foreach (var parameter in parameters)
                    {
                        switch (parameter)
                        {
                            case MemberInitExpression memberInitExpression:

                                var test = Expression.Lambda<Func<object>>(memberInitExpression).Compile()();
                                parameterObjects.Add(test);
                                break;
                            case ConstantExpression constantExpression:
                                parameterObjects.Add(constantExpression.Value);
                                break;

                            default:
                                // Skip others, first would be ParameterExpression, but this is not needed
                                continue;
                        }
                    }

                    var buildCollectionQueryBaseMethod = typeof(QueryBuilders)
                        .GetMethod(nameof(QueryBuilders.BuildCollectionQuery));
                    var buildCollectionQueryGenericMethod = buildCollectionQueryBaseMethod?.MakeGenericMethod(genericType);
                    var result = buildCollectionQueryGenericMethod?.Invoke(null, new object[] { _context, parameterObjects.ToArray(), genericType.Name.ToLower() } );
                    var graphQueryType = typeof(GraphCollectionQuery<>).MakeGenericType(returnType);

                    var renameQueryBaseMethod = typeof(QueryBuilders)
                        .GetMethod(nameof(QueryBuilders.RenameQueryArguments));
                    var renameQueryGenericMethod = renameQueryBaseMethod?.MakeGenericMethod(genericType);

                    (string q, List<KeyValuePair<string, object>> arguments) = ((string, List<KeyValuePair<string, object>>))renameQueryGenericMethod?.Invoke(null, new object[] { result, methodName});

                    additionalArguments = arguments;
                    aliasName = string.Empty;
                    selectClause = $"{selectClause}{(!string.IsNullOrEmpty(selectClause) ? Environment.NewLine : string.Empty)}{q}";
                    return selectClause;

                // Attempt to support subqueries
                default:
                    throw new NotSupportedException($"Selector of type {body.NodeType} is not implemented yet");
            }
        }

        public static string BuildMemberAccessSelectClauseForNestedQueriesMethodCall(MethodCallExpression body,
            string selectClause, string padding, out string alias)
        {
            var typeMember = body.Arguments.OfType<MemberExpression>().First();
            alias = typeMember.Member.Name;
            if (body.Method.Name.Equals("Select")) // Only select quires are supported now
            {
                foreach (var lambdaExpression in body.Arguments.OfType<LambdaExpression>())
                {
                    var a = typeMember.Member.Name;
                    var s = BuildMemberAccessSelectClauseForNestedQueries(lambdaExpression.Body, "", padding, out a);
                    selectClause = $"{selectClause}{(!string.IsNullOrEmpty(selectClause) ? Environment.NewLine : string.Empty)}{s}";
                }
                return BuildMemberAccessSelectClause(
                    typeMember, selectClause,
                    padding, "");
            }
            return selectClause;
        }

        private static string BuildMemberAccessSelectClauseForNestedQueriesMember(MemberExpression body, string selectClause,
            string padding, string alias)
        {
            return BuildMemberAccessSelectClause(body, "", padding, alias);
        }

        private static string BuildMemberAccessSelectClauseForNestedQueriesNew(NewExpression body, string selectClause,
            string padding, out string alias)
        {
            alias = body.Members?.FirstOrDefault()?.Name;
            var tempFields = new List<(string key, string value)>();

            foreach (var bodyArgument in body.Arguments)
            {
                var s = BuildMemberAccessSelectClauseForNestedQueries(bodyArgument, string.Empty, padding, out var elementAlias);

                tempFields.Add((elementAlias, s));
            }

            // Quick and diry solution to support multiple selects on same type
            // Does not check if fields are duplicated
            var lookup = tempFields.ToLookup(f => f.key, f => f.value);
            var fields = new List<string>();
            foreach (var l in lookup)
            {
                var left = l.First();
                foreach (var r in l.Skip(1))
                {
                    left = Merge(left, r);
                }
                fields.Add(left);
            }

            selectClause = $"{selectClause}{(!string.IsNullOrEmpty(selectClause) ? Environment.NewLine : string.Empty)}{string.Join(Environment.NewLine, fields)}";

            return selectClause;
        }

        private static string Merge(string left, string right) 
        {
            var closingBracket = "}";
            var indexOfLastBracketLeft = left.LastIndexOf(closingBracket);
            var indexOfFirstNewLineRight = right.IndexOf(Environment.NewLine) + Environment.NewLine.Length;
            var indexOfLastBracketRight = right.LastIndexOf(closingBracket) - indexOfFirstNewLineRight;
            right = right.Remove(0, indexOfFirstNewLineRight).Remove(indexOfLastBracketRight, 1);
            left = left.Insert(indexOfLastBracketLeft, right);

            return left;
        }

        private static string BuildSelectClauseForType(Type targetType, int depth = 1)
        {
            var propertyInfos = targetType.GetProperties();

            var propertiesToInclude = propertyInfos.Where(info => !info.PropertyType.HasNestedProperties());

            var selectClause = string.Join(Environment.NewLine, propertiesToInclude.Select(info => new string(' ', depth * 2) + info.Name.ToCamelCase()));

            return selectClause;
        }

        private static SelectClauseDetails BuildSelectClauseForType(Type targetType, List<IncludeDetails> includes)
        {
            var selectClause = BuildSelectClauseForType(targetType);
            var includeVariables = new Dictionary<string, object>();

            for (var index = 0; index < includes.Count; index++)
            {
                var include = includes[index];
                var prefix = includes.Count == 1 ? "" : index.ToString();

                var fieldsFromInclude = BuildSelectClauseForInclude(targetType, include, includeVariables, prefix);
                selectClause = selectClause + Environment.NewLine + fieldsFromInclude;
            }
            return new SelectClauseDetails { SelectClause = selectClause, IncludeArguments = includeVariables };
        }

        private static string BuildSelectClauseForInclude(Type targetType, IncludeDetails includeDetails, Dictionary<string, object> includeVariables, string parameterPrefix = "", int parameterIndex = 0, int depth = 1)
        {
            var include = includeDetails.Path;
            if (string.IsNullOrEmpty(include))
            {
                return BuildSelectClauseForType(targetType, depth);
            }
            var leftPadding = new string(' ', depth * 2);

            var dotIndex = include.IndexOf(".", StringComparison.InvariantCultureIgnoreCase);

            var currentIncludeName = dotIndex >= 0 ? include.Substring(0, dotIndex) : include;

            Type propertyType;
            var propertyInfo = targetType.GetProperty(currentIncludeName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            var includeName = currentIncludeName.ToCamelCase();

            var includeMethodInfo = includeDetails.MethodIncludes.Count > parameterIndex ? includeDetails.MethodIncludes[parameterIndex].Method : null;
            var includeByMethod = includeMethodInfo != null && currentIncludeName == includeMethodInfo.Name && propertyInfo.PropertyType == includeMethodInfo.ReturnType;

            if (includeByMethod)
            {
                var methodDetails = includeDetails.MethodIncludes[parameterIndex];
                parameterIndex++;

                propertyType = methodDetails.Method.ReturnType.GetTypeOrListType();

                var includeMethodParams = methodDetails.Parameters.Where(pair => pair.Value != null).ToList();
                includeName = methodDetails.Method.Name.ToCamelCase();

                if (includeMethodParams.Any())
                {
                    var includeParameters = string.Join(", ", includeMethodParams.Select(pair => pair.Key + ": $" + pair.Key + parameterPrefix + parameterIndex));
                    includeName = $"{includeName}({includeParameters})";

                    foreach (var item in includeMethodParams)
                    {
                        includeVariables.Add(item.Key + parameterPrefix + parameterIndex, item.Value);
                    }
                }
            }
            else
            {
                propertyType = propertyInfo.PropertyType.GetTypeOrListType();
            }

            if (propertyType.IsValueTypeOrString())
            {
                return leftPadding + includeName;
            }

            var restOfTheInclude = new IncludeDetails(includeDetails.MethodIncludes) { Path = dotIndex >= 0 ? include.Substring(dotIndex + 1) : "" };

            var fieldsFromInclude = BuildSelectClauseForInclude(propertyType, restOfTheInclude, includeVariables, parameterPrefix, parameterIndex, depth + 1);
            fieldsFromInclude = $"{leftPadding}{includeName} {{{Environment.NewLine}{fieldsFromInclude}{Environment.NewLine}{leftPadding}}}";
            return fieldsFromInclude;
        }


    }

    static class QueryBuilders
    {
        public  static GraphCollectionQuery<T> BuildCollectionQuery<T>(GraphContext context, object[] parameterValues, [CallerMemberName] string queryName = null)
        {
            var arguments = BuildDictionary(context, parameterValues, queryName);
            return new GraphCollectionQuery<T, T>(context, queryName) { Arguments = arguments };
        }

        public static GraphItemQuery<T> BuildItemQuery<T>(GraphContext context, object[] parameterValues, [CallerMemberName] string queryName = null)
        {
            var arguments = BuildDictionary(context, parameterValues, queryName);
            return new GraphItemQuery<T, T>(context, queryName) { Arguments = arguments };
        }

        private static Dictionary<string, object> BuildDictionary(GraphContext context, object[] parameterValues, string queryName)
        {
            var parameters = context.GetType().GetMethod(queryName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance).GetParameters();
            var arguments = parameters.Zip(parameterValues, (info, value) => new { info.Name, Value = value }).ToDictionary(arg => arg.Name, arg => arg.Value);
            return arguments;
        }

        public static (string query, List<KeyValuePair<string,object>> arguments) RenameQueryArguments<T>(GraphCollectionQuery<T> query, string prefix)
        {
            var argumentList = new List<KeyValuePair<string, object>>();

            foreach (var arg in query.Arguments.Where(pair => pair.Value != null).ToList())
            {
                var newKey = prefix + arg.Key;
                argumentList.Add(new KeyValuePair<string, object>(newKey, arg.Value));
            }

            query.Update(argumentList.ToDictionary(d => d.Key, d => d.Value));

            return (query.Query, argumentList);
        }
    }

    class GraphQLQuery
    {
        public GraphQLQuery(string query, IReadOnlyDictionary<string, object> variables, string fullQuery)
        {
            Query = query;
            Variables = variables;
            FullQuery = fullQuery;
        }

        public string Query { get; }
        public string FullQuery { get; }
        public IReadOnlyDictionary<string, object> Variables { get; }
    }

    class SelectClauseDetails
    {
        public string SelectClause { get; set; }
        public Dictionary<string, object> IncludeArguments { get; set; }
    }
}