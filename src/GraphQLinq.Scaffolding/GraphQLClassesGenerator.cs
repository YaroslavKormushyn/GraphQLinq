using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CSharp;
using Spectre.Console;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Linq.Expressions;
using System.Reflection;
using GraphQLinq.Attributes;
using Scaffolding;

namespace GraphQLinq.Scaffolding
{
    class GraphQLClassesGenerator
    {
        List<string> usings = new() { "System", "System.Collections.Generic" };

        private Dictionary<string, string> renamedProperties = new();
        private readonly CodeGenerationOptions options;

        private static readonly Dictionary<string, (string name, Type type)> TypeMapping = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "Int", new("int", typeof(int)) },
            { "Float", new("float", typeof(float)) },
            { "String", new("string", typeof(string)) },
            { "ID", new("string", typeof(string)) },
            { "Date", new("DateTime", typeof(DateTime)) },
            { "Boolean", new("bool", typeof(bool)) },
            { "Long", new("long", typeof(long)) },
            { "uuid", new("Guid", typeof(Guid)) },
            { "timestamptz", new("DateTimeOffset", typeof(DateTimeOffset)) },
            { "Uri", new("Uri", typeof(Uri)) }
        };

        private static readonly List<string> BuiltInTypes = new()
        {
            "ID",
            "Int",
            "Float",
            "String",
            "Boolean"
        };

        private static readonly AdhocWorkspace Workspace = new();

        public GraphQLClassesGenerator(CodeGenerationOptions options)
        {
            var attributeNamespace = typeof(GraphQLAttribute).Namespace;
            var reflectionNamespace = typeof(PropertyInfo).Namespace;
            if (attributeNamespace != null) usings.Add(attributeNamespace);
            if (reflectionNamespace != null) usings.Add(reflectionNamespace);
            this.options = options;
        }

        public string GenerateClient(Schema schema, string endpointUrl)
        {
            var queryType = schema.QueryType.Name;
            var mutationType = schema.MutationType?.Name;
            var subscriptionType = schema.SubscriptionType?.Name;

            var types = schema.Types.Where(type => !type.Name.StartsWith("__")
                                                                && !BuiltInTypes.Contains(type.Name)
                                                                && queryType != type.Name && mutationType != type.Name && subscriptionType != type.Name).ToList();

            var enums = types.Where(type => type.Kind == TypeKind.Enum);
            var classes = types.Where(type => type.Kind == TypeKind.Object || type.Kind == TypeKind.InputObject || type.Kind == TypeKind.Union).OrderBy(type => type.Name);
            var interfaces = types.Where(type => type.Kind == TypeKind.Interface);

            AnsiConsole.WriteLine("Scaffolding enums ...");
            foreach (var enumInfo in enums)
            {
                var syntax = GenerateEnum(enumInfo);
                FormatAndWriteToFile(syntax, enumInfo.Name);
            }

            AnsiConsole.WriteLine("Scaffolding classes ...");
            foreach (var classInfo in classes)
            {
                var syntax = GenerateClass(classInfo);
                FormatAndWriteToFile(syntax, classInfo.Name);
            }

            AnsiConsole.WriteLine("Scaffolding interfaces ...");
            foreach (var interfaceInfo in interfaces)
            {
                var syntax = GenerateInterface(interfaceInfo);
                FormatAndWriteToFile(syntax, interfaceInfo.Name);
            }

            var classesWithArgFields = classes.Where(type => (type.Fields ?? new List<Field>()).Any(field => field.Args.Any())).ToList();

            AnsiConsole.WriteLine("Scaffolding Query Extensions ...");
            var queryExtensions = GenerateQueryExtensions(classesWithArgFields);
            FormatAndWriteToFile(queryExtensions, "QueryExtensions");

            var queryClass = schema.Types.Single(type => type.Name == queryType);

            AnsiConsole.WriteLine("Scaffolding GraphQLContext ...");
            var graphContext = GenerateGraphContext(queryClass, endpointUrl);
            FormatAndWriteToFile(graphContext, $"{options.ContextName}Context");

            AnsiConsole.WriteLine("Scaffolding GraphQLSubQueryContext ...");
            var graphSubQueryContext = GenerateSubQueryContext(classesWithArgFields);
            FormatAndWriteToFile(graphSubQueryContext, $"{options.ContextName}{CodeNameConstants.SubQueryContext}");

            return $"{options.ContextName}Context";
        }


        private SyntaxNode GenerateEnum(GraphqlType enumInfo)
        {
            var topLevelDeclaration = RoslynUtilities.GetTopLevelNode(options.Namespace);
            var name = enumInfo.Name.NormalizeIfNeeded(options);

            var declaration = EnumDeclaration(name).AddModifiers(Token(SyntaxKind.PublicKeyword));

            foreach (var enumValue in enumInfo.EnumValues)
            {
                declaration = declaration.AddMembers(EnumMemberDeclaration(Identifier(EscapeIdentifierName(enumValue.Name))));
            }

            return topLevelDeclaration.AddMembers(declaration);
        }

        private SyntaxNode GenerateClass(GraphqlType classInfo)
        {
            var topLevelDeclaration = RoslynUtilities.GetTopLevelNode(options.Namespace);

            var semicolonToken = Token(SyntaxKind.SemicolonToken);

            var className = classInfo.Name.NormalizeIfNeeded(options);

            var declaration = ClassDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword))
                .AddAttributeLists(CreateAttributeList(classInfo.Name));

            foreach (var @interface in classInfo.Interfaces ?? new List<GraphqlType>())
            {
                declaration = declaration.AddBaseListTypes(SimpleBaseType(ParseTypeName(@interface.Name)));
            }

            foreach (var field in classInfo.Fields ?? classInfo.InputFields ?? new List<Field>())
            {
                var fieldName = field.Name.NormalizeIfNeeded(options);

                if (fieldName == className)
                {
                    fieldName = $"{fieldName}{CodeNameConstants.PropertyPostfix}"; // TODO better naming
                    renamedProperties.Add(className, fieldName);
                }

                var (fieldTypeName, fieldType) = GetSharpTypeName(field.Type);

                var typeSyntax = ParseTypeName(fieldTypeName);

                if (NeedsNullable(fieldType, field.Type))
                {
                    typeSyntax = NullableType(typeSyntax);
                }

                var property = PropertyDeclaration(typeSyntax, fieldName)
                                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                                    .AddAttributeLists(CreateAttributeList(field.Name));

                property = property.AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                   .WithSemicolonToken(semicolonToken));

                property = property.AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                   .WithSemicolonToken(semicolonToken));

                declaration = declaration.AddMembers(property);
            }

            foreach (var @using in usings)
            {
                topLevelDeclaration = topLevelDeclaration.AddUsings(UsingDirective(IdentifierName(@using)));
            }

            topLevelDeclaration = topLevelDeclaration.AddMembers(declaration);

            return topLevelDeclaration;
        }

        private SyntaxNode GenerateInterface(GraphqlType interfaceInfo)
        {
            var topLevelDeclaration = RoslynUtilities.GetTopLevelNode(options.Namespace);

            var semicolonToken = Token(SyntaxKind.SemicolonToken);

            var name = interfaceInfo.Name.NormalizeIfNeeded(options);

            var declaration = InterfaceDeclaration(name).AddModifiers(Token(SyntaxKind.PublicKeyword));

            foreach (var field in interfaceInfo.Fields)
            {
                var (fieldTypeName, fieldType) = GetSharpTypeName(field.Type);

                if (NeedsNullable(fieldType, field.Type))
                {
                    fieldTypeName += "?";
                }

                var fieldName = field.Name.NormalizeIfNeeded(options);

                var property = PropertyDeclaration(ParseTypeName(fieldTypeName), fieldName);

                property = property.AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                   .WithSemicolonToken(semicolonToken));

                property = property.AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                   .WithSemicolonToken(semicolonToken));

                declaration = declaration.AddMembers(property);
            }

            foreach (var @using in usings)
            {
                topLevelDeclaration = topLevelDeclaration.AddUsings(UsingDirective(IdentifierName(@using)));
            }

            return topLevelDeclaration.AddMembers(declaration);
        }

        private SyntaxNode GenerateQueryExtensions(List<GraphqlType> classesWithArgFields)
        {
            var topLevelDeclaration = RoslynUtilities.GetTopLevelNode(options.Namespace);

            var declaration = ClassDeclaration("QueryExtensions")
                                            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword));

            foreach (var @class in classesWithArgFields)
            {
                foreach (var field in @class.Fields.Where(f => f.Args.Any()))
                {
                    var (fieldTypeName, _) = GetSharpTypeName(field.Type);

                    var fieldName = field.Name.NormalizeIfNeeded(options);

                    var methodDeclaration = MethodDeclaration(ParseTypeName(fieldTypeName), fieldName)
                                            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword));

                    var identifierName = EscapeIdentifierName(@class.Name.ToCamelCase());

                    var thisParameter = Parameter(Identifier(identifierName))
                                             .WithType(ParseTypeName(@class.Name.NormalizeIfNeeded(options)))
                                             .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)));

                    var methodParameters = new List<ParameterSyntax> { thisParameter };

                    foreach (var arg in field.Args)
                    {
                        (fieldTypeName, _) = GetSharpTypeName(arg.Type);

                        var typeName = TypeMapping.Values.Any(tuple => tuple.name == fieldTypeName) ? fieldTypeName : fieldTypeName.NormalizeIfNeeded(options);

                        var parameterSyntax = Parameter(Identifier(arg.Name)).WithType(ParseTypeName(typeName));
                        methodParameters.Add(parameterSyntax);
                    }

                    // Get the property name that might be renamed
                    var returnPropertyName = identifierName.Equals(fieldName, StringComparison.OrdinalIgnoreCase) && renamedProperties.ContainsKey(fieldName) ? renamedProperties[fieldName] : fieldName;

                    var returnStatement = ReturnStatement(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(identifierName),
                                IdentifierName(returnPropertyName))
                            .WithOperatorToken(Token(SyntaxKind.DotToken)));

                    methodDeclaration = methodDeclaration.AddParameterListParameters(methodParameters.ToArray())
                        .WithBody(Block(returnStatement))
                        .AddAttributeLists(CreateAttributeList(field.Name));

                    declaration = declaration.AddMembers(methodDeclaration);
                }
            }

            foreach (var @using in usings)
            {
                topLevelDeclaration = topLevelDeclaration.AddUsings(UsingDirective(IdentifierName(@using)));
            }

            topLevelDeclaration = topLevelDeclaration.AddMembers(declaration);

            return topLevelDeclaration;
        }

        private string GetTypeNameFromField(Field? field)
        {
            return field?.Type.Name ?? field?.Name ?? string.Empty;
        }
        private string? GetTypeNameFromFieldType(FieldType? fieldType)
        {
            return fieldType?.Name ?? GetTypeNameFromFieldType(fieldType?.OfType);
        }

        #region Synxtax helpers
        private LocalDeclarationStatementSyntax CreateVariableDeclaration(string variableName,
            ExpressionSyntax initializer)
        {
            return LocalDeclarationStatement(VariableDeclaration(Token(SyntaxKind.VarKeyword).Text.IdentifierName())
                .WithVariables(SeparatedList(new List<VariableDeclaratorSyntax>
                {
                    VariableDeclarator(variableName)
                        .WithInitializer(EqualsValueClause(initializer))
                })));
        }

        private InvocationExpressionSyntax CreateSimpleMemberInvocation(string target, string member)
        {
            return InvocationExpression(CreateSimpleMemberAccess(
                target,
                member
            ));
        }
        private AttributeListSyntax CreateAttributeList(string attributeName)
        {
            var methodAttributeArguments = ParseAttributeArgumentList($"({nameof(GraphQLAttribute.Name)} = \"{attributeName}\")");
            var methodAttribute = Attribute(ParseName(GetSimplifiedName(typeof(GraphQLAttribute))), methodAttributeArguments); ;
            return AttributeList(AttributeList().Attributes.Add(methodAttribute));
        }

        private MemberAccessExpressionSyntax CreateSimpleMemberAccess(string target, string member)
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                target.IdentifierName(),
                member.IdentifierName()
            );
        }
        #endregion

        private SyntaxNode GenerateGraphContext(GraphqlType queryInfo, string endpointUrl)
        {
            var topLevelDeclaration = RoslynUtilities.GetTopLevelNode(options.Namespace).AddUsings(UsingDirective(IdentifierName("GraphQLinq")));

            var className = $"{options.ContextName}Context";
            var declaration = ClassDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword))
                .AddBaseListTypes(SimpleBaseType(ParseTypeName("GraphContext")));

            var thisInitializer = ConstructorInitializer(SyntaxKind.ThisConstructorInitializer)
                                    .AddArgumentListArguments(Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(endpointUrl))));

            var defaultConstructorDeclaration = ConstructorDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithInitializer(thisInitializer)
                .WithBody(Block());

            var baseInitializer = ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                .AddArgumentListArguments(Argument(IdentifierName("baseUrl")),
                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(""))));

            var baseUrlConstructorDeclaration = ConstructorDeclaration(className)
                                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                                    .AddParameterListParameters(Parameter(Identifier("baseUrl")).WithType(ParseTypeName("string")))
                                    .WithInitializer(baseInitializer)
                                    .WithBody(Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, nameof(GraphContext.SubQueryContext).IdentifierName(), ObjectCreationExpression($"{options.ContextName}SubQueryContext".IdentifierName()).AddArgumentListArguments(Argument(ThisExpression()))))));

            var baseHttpClientInitializer = ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                .AddArgumentListArguments(Argument(IdentifierName("httpClient")));

            var httpClientConstructorDeclaration = ConstructorDeclaration(className)
                                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                                    .AddParameterListParameters(Parameter(Identifier("httpClient")).WithType(ParseTypeName("HttpClient")))
                                    .WithInitializer(baseHttpClientInitializer)
                                    .WithBody(Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, nameof(GraphContext.SubQueryContext).IdentifierName(), ObjectCreationExpression($"{options.ContextName}SubQueryContext".IdentifierName()).AddArgumentListArguments(Argument(ThisExpression()))))));

            declaration = declaration.AddMembers(defaultConstructorDeclaration, baseUrlConstructorDeclaration, httpClientConstructorDeclaration);

            foreach (var field in queryInfo.Fields)
            {
                if (field.Type.Name == queryInfo.Name || field.Type.OfType?.Name == queryInfo.Name)
                {
                    continue; //Workaround for Query.relay method in GitHub schema
                }

                var (fieldTypeName, fieldType) = GetSharpTypeName(field.Type.Kind == TypeKind.NonNull ? field.Type.OfType : field.Type, true);

                var baseMethodName = fieldTypeName.Replace("GraphItemQuery", "BuildItemQuery")
                                         .Replace("GraphCollectionQuery", "BuildCollectionQuery");

                var fieldName = field.Name.NormalizeIfNeeded(options);

                var methodDeclaration = MethodDeclaration(ParseTypeName(fieldTypeName), fieldName)
                                            .AddModifiers(Token(SyntaxKind.PublicKeyword));

                var methodParameters = new List<ParameterSyntax>();

                var initializer = InitializerExpression(SyntaxKind.ArrayInitializerExpression);

                foreach (var arg in field.Args)
                {
                    (fieldTypeName, fieldType) = GetSharpTypeName(arg.Type);

                    if (NeedsNullable(fieldType, arg.Type))
                    {
                        fieldTypeName += "?";
                    }

                    var parameterSyntax = Parameter(Identifier(arg.Name)).WithType(ParseTypeName(fieldTypeName));
                    methodParameters.Add(parameterSyntax);

                    initializer = initializer.AddExpressions(IdentifierName(arg.Name));
                }

                var paramsArray = ArrayCreationExpression(ArrayType(ParseTypeName("object[]")), initializer);

                var parametersDeclaration = LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                                            .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("parameterValues"))
                                            .WithInitializer(EqualsValueClause(paramsArray)))));

                var parametersArgument = Argument(IdentifierName("parameterValues"));

                var argumentSyntax = Argument(InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    TypeOfExpression(GetMappedType(GetTypeNameFromField(field)).name.IdentifierName()), // TODO Don't like that we need to get the mapped type, from the type name and then the name property. 
                    nameof(GraphQLAttributeExtensions.GetGraphQLNameFromType).IdentifierName())));

                var returnStatement = ReturnStatement(InvocationExpression(IdentifierName(baseMethodName))
                                            .WithArgumentList(ArgumentList(SeparatedList(new List<ArgumentSyntax> { parametersArgument, argumentSyntax }))));

                methodDeclaration = methodDeclaration.AddParameterListParameters(methodParameters.ToArray())
                                                        .WithBody(Block(parametersDeclaration, returnStatement));

                declaration = declaration.AddMembers(methodDeclaration);
            }

            foreach (var @using in usings)
            {
                topLevelDeclaration = topLevelDeclaration.AddUsings(UsingDirective(IdentifierName(@using)));
            }
            topLevelDeclaration = topLevelDeclaration.AddUsings(UsingDirective(IdentifierName("System.Net.Http")));

            topLevelDeclaration = topLevelDeclaration.AddMembers(declaration);

            return topLevelDeclaration;
        }

        private SyntaxNode GenerateSubQueryContext(List<GraphqlType> classesWithArgFields)
        {
            var topLevelDeclaration = RoslynUtilities.GetTopLevelNode(options.Namespace)
                .AddUsings(UsingDirective(IdentifierName(nameof(GraphQLinq))));

            var className = $"{options.ContextName}{CodeNameConstants.SubQueryContext}";
            var declaration = ClassDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SimpleBaseType(ParseTypeName(CodeNameConstants.SubQueryContext)));

            var baseInitializer = ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                .AddArgumentListArguments(Argument(IdentifierName(CodeNameConstants.ContextParameterName)));

            var baseUrlConstructorDeclaration = ConstructorDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(Parameter(Identifier(CodeNameConstants.ContextParameterName)).WithType(ParseTypeName(nameof(GraphContext))))
                .WithInitializer(baseInitializer)
                .WithBody(Block());

            declaration = declaration.AddMembers(baseUrlConstructorDeclaration);

            var fieldsWithArgs = classesWithArgFields.SelectMany(c => c.Fields).Where(f => f.Args.Any())
                .GroupBy(f => f.Name); // Group methods by name to find duplicates
            var distinctFieldsWithArgs = fieldsWithArgs
                .SelectMany(f => f.DistinctBy(g => (g.Args.SelectMany(s =>
                        $"{GetTypeNameFromFieldType(s.Type)}")
                    .ToString()))); // Find the distinct methods based arguments in group (name does not have to be unique as there can be overloads)

            foreach (var field in distinctFieldsWithArgs)
            {
                var (fieldTypeName, fieldType) =
                    GetSharpTypeName(field.Type.Kind == TypeKind.NonNull ? field.Type.OfType : field.Type, true);

                var baseMethodName = fieldTypeName.Replace( CodeNameConstants.GraphItemQueryTypeName, CodeNameConstants.BuildItemQueryMethodName)
                    .Replace(CodeNameConstants.GraphCollectionQueryTypeName, CodeNameConstants.BuildCollectionQueryMethodName);

                var fieldName = field.Name.NormalizeIfNeeded(options);

                var methodDeclaration = MethodDeclaration(ParseTypeName(fieldTypeName), fieldName)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword));

                var methodParameters = new List<ParameterSyntax>();

                var initializer = InitializerExpression(SyntaxKind.ArrayInitializerExpression);

                foreach (var arg in field.Args)
                {
                    (fieldTypeName, fieldType) = GetSharpTypeName(arg.Type);

                    var typeSyntax = ParseTypeName(fieldTypeName);
                    if (NeedsNullable(fieldType, field.Type))
                    {
                        typeSyntax = NullableType(typeSyntax);
                    }

                    var parameterSyntax = Parameter(arg.Name.Identifier()).WithType(typeSyntax);
                    methodParameters.Add(parameterSyntax);

                    initializer = initializer.AddExpressions(IdentifierName(arg.Name));
                }

                var paramsArray = ArrayCreationExpression(SyntaxKind.ObjectKeyword.ArrayTypeSyntax(), initializer);

                var parametersDeclaration = CreateVariableDeclaration(CodeNameConstants.ParameterValuesVariableName, paramsArray);

                var parametersArgument = Argument(CodeNameConstants.ParameterValuesVariableName.IdentifierName());

                var argumentSyntax = Argument(InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            InvocationExpression(nameof(Type.GetType).IdentifierName()),
                            nameof(GraphQLAttributeExtensions.GetGraphQLNameFromMethod).IdentifierName()))
                    .AddArgumentListArguments(
                        Argument(InvocationExpression(Token(SyntaxKind.NameOfKeyword).Text.IdentifierName())
                            .AddArgumentListArguments(Argument(GetMappedType(GetTypeNameFromField(field)).name
                                .IdentifierName())))));// TODO Don't like it, improve readability. Maybe split some thing or move to helper

                var returnStatement = ReturnStatement(InvocationExpression(baseMethodName.IdentifierName())
                    .WithArgumentList(ArgumentList(SeparatedList(new List<ArgumentSyntax>
                        { parametersArgument, argumentSyntax }))));

                methodDeclaration = methodDeclaration.AddParameterListParameters(methodParameters.ToArray())
                    .WithBody(Block(parametersDeclaration, returnStatement))
                    .AddAttributeLists(CreateAttributeList(field.Name));

                declaration = declaration.AddMembers(methodDeclaration);
            }

            topLevelDeclaration = topLevelDeclaration.AddMembers(declaration);
            foreach (var @using in usings)
            {
                topLevelDeclaration = topLevelDeclaration.AddUsings(UsingDirective(IdentifierName(@using)));
            }

            return topLevelDeclaration;
        }


        private static bool NeedsNullable(Type? systemType, FieldType type)
        {
            if (systemType == null)
            {
                return false;
            }

            return type.Kind == TypeKind.Scalar && systemType.IsValueType;
        }

        private void FormatAndWriteToFile(SyntaxNode syntax, string name)
        {
            if (!Directory.Exists(options.OutputDirectory))
            {
                Directory.CreateDirectory(options.OutputDirectory);
            }

            name = name.NormalizeIfNeeded(options);

            var fileName = Path.Combine(options.OutputDirectory, name + ".cs");
            using (var streamWriter = File.CreateText(fileName))
            {
                Formatter.Format(syntax, Workspace).WriteTo(streamWriter);
            }
        }

        private (string typeName, Type? typeType) GetSharpTypeName(FieldType? fieldType, bool wrapWithGraphTypes = false)
        {
            if (fieldType == null)
            {
                throw new NotImplementedException("ofType nested more than three levels not implemented");
            }

            var typeName = fieldType.Name;
            Type? resultType;

            if (typeName == null)
            {
                switch (fieldType.Kind)
                {
                    case TypeKind.List:
                        {
                            var type = GetSharpTypeName(fieldType.OfType).typeName;
                            typeName = wrapWithGraphTypes ? $"GraphCollectionQuery<{type}>" : $"List<{type}>";
                            return (typeName, null);
                        }
                    case TypeKind.NonNull when fieldType.OfType?.Name?.ToUpper() == "ID":
                        (typeName, resultType) = GetMappedType("string");
                        break;
                    default:
                        return GetSharpTypeName(fieldType.OfType);
                }
            }
            else
            {
                (typeName, resultType) = GetMappedType(fieldType.Name);

                if (resultType == null && fieldType.Kind == TypeKind.Scalar)
                {
                    (typeName, resultType) = GetMappedType("string");
                }
            }

            if (wrapWithGraphTypes)
            {
                typeName = $"GraphItemQuery<{typeName}>";
                resultType = null;
            }

            return (typeName, resultType);
        }


        private (string name, Type? type) GetMappedType(string name)
        {
            return TypeMapping.ContainsKey(name) ? TypeMapping[name] : (name.NormalizeIfNeeded(options), null);
        }

        private string EscapeIdentifierName(string name)
        {
            return SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? $"@{name}" : name;
        }

        private string GetSimplifiedName(Type type)
        {
            if (type.IsSubclassOf(typeof(Attribute)))
            {
                return type.Name.Replace(nameof(System.Attribute), string.Empty);
            }

            using var provider = new CSharpCodeProvider();

            var tRef = new CodeTypeReference(type);
            return provider.GetTypeOutput(tRef);
        }
    }
}