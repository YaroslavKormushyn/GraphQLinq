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
using GraphQLinq.Client.Attributes;
using Microsoft.CSharp;
using Spectre.Console;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace GraphQLinq.Scaffolding
{
    class GraphQLClassesGenerator
    {
        List<string> usings = new() { "System", "System.Collections.Generic" };

        private Dictionary<string, string> renamedProperties = new();
        private readonly CodeGenerationOptions options;

        private static readonly Dictionary<string, (string Name, Type type)> TypeMapping = new(StringComparer.InvariantCultureIgnoreCase)
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
            var attributeNamespace = typeof(GraphQLMemberAttribute).Namespace;
            if (attributeNamespace != null) usings.Add(attributeNamespace);
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

            // TODO Refactor
            var classAttributeArguments = ParseAttributeArgumentList($"({nameof(GraphQLTypeAttribute.Name)} = \"{classInfo.Name}\")");
            var classAttribute = Attribute(ParseName(GetSimplifiedName(typeof(GraphQLTypeAttribute))), classAttributeArguments);
            var classAttributes = AttributeList(AttributeList().Attributes.Add(classAttribute));

            var declaration = ClassDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword))
                .AddAttributeLists(classAttributes);

            foreach (var @interface in classInfo.Interfaces ?? new List<GraphqlType>())
            {
                declaration = declaration.AddBaseListTypes(SimpleBaseType(ParseTypeName(@interface.Name)));
            }

            foreach (var field in classInfo.Fields ?? classInfo.InputFields ?? new List<Field>())
            {
                var fieldName = field.Name.NormalizeIfNeeded(options);

                if (fieldName == className)
                {
                    fieldName = $"{fieldName}Prop"; // TODO better naming
                    renamedProperties.Add(className, fieldName);
                }

                // TODO refactor
                var memberAttributeArguments = ParseAttributeArgumentList($"({nameof(GraphQLMemberAttribute.Name)} = \"{field.Name}\")");
                var memberAttribute = Attribute(ParseName(GetSimplifiedName(typeof(GraphQLMemberAttribute))), memberAttributeArguments); ;
                var memberAttributes = AttributeList(AttributeList().Attributes.Add(memberAttribute));

                if (fieldName == className)
                {
                    declaration = declaration.ReplaceToken(declaration.Identifier, Identifier($"{className}Type"));
                }

                var (fieldTypeName, fieldType) = GetSharpTypeName(field.Type);

                if (NeedsNullable(fieldType, field.Type))
                {
                    fieldTypeName += "?";
                }

                var property = PropertyDeclaration(ParseTypeName(fieldTypeName), fieldName)
                                            .AddModifiers(Token(SyntaxKind.PublicKeyword))
                                            .AddAttributeLists(memberAttributes);

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
            var exceptionMessage = Literal("This method is not implemented. It exists solely for query purposes.");
            var argumentListSyntax = ArgumentList(SingletonSeparatedList(Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, exceptionMessage))));

            var notImplemented = ThrowStatement(ObjectCreationExpression(IdentifierName("NotImplementedException"), argumentListSyntax, null));

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

                        var typeName = TypeMapping.Values.Any(tuple => tuple.Name == fieldTypeName) ? fieldTypeName : fieldTypeName.NormalizeIfNeeded(options);

                        var parameterSyntax = Parameter(Identifier(arg.Name)).WithType(ParseTypeName(typeName));
                        methodParameters.Add(parameterSyntax);
                    }

                    // TODO refactor
                    var methodAttributeArguments = ParseAttributeArgumentList($"({nameof(GraphQLMethodAttribute.Name)} = \"{field.Name}\")");
                    var methodAttribute = Attribute(ParseName(GetSimplifiedName(typeof(GraphQLMethodAttribute))), methodAttributeArguments); ;
                    var methodAttributes = AttributeList(AttributeList().Attributes.Add(methodAttribute));

                    // Get the property name that might be renamed
                    var returnPropertyName = identifierName.Equals(fieldName, StringComparison.OrdinalIgnoreCase) && renamedProperties.ContainsKey(fieldName) ? renamedProperties[fieldName] : fieldName;

                    var returnStatement = ReturnStatement(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(identifierName),
                                IdentifierName(returnPropertyName))
                            .WithOperatorToken(Token(SyntaxKind.DotToken)));

                    methodDeclaration = methodDeclaration.AddParameterListParameters(methodParameters.ToArray())
                        .WithBody(Block(returnStatement))
                        .AddAttributeLists(methodAttributes);


                    declaration = declaration.AddMembers(methodDeclaration);
                }
            }

            declaration = GenerateTypeAttributeHelpers(declaration);
            declaration = GenerateMemberAttributeHelpers(declaration);
            declaration = GenerateMethodAttributeHelpers(declaration);

            foreach (var @using in usings)
            {
                topLevelDeclaration = topLevelDeclaration.AddUsings(UsingDirective(IdentifierName(@using)));
            }

            topLevelDeclaration = topLevelDeclaration.AddMembers(declaration);

            return topLevelDeclaration;
        }

        private ClassDeclarationSyntax GenerateTypeAttributeHelpers(ClassDeclarationSyntax declaration)
        {
            // TODO magic strings
            var getGraphQLNameFromTypeMethodName = "GetGraphQLNameFromType";
            var typeVariableName = "t";

            var graphQLTypeAttributeName = nameof(GraphQLTypeAttribute);

            // Method parameters
            var typeParameterName = nameof(Type).ToLower();
            var typeParameterType = nameof(Type);
            var namePropertyName = nameof(Type.Name);
            var objectParameterTypeName = GetSimplifiedName(typeof(object));
            var typeGetTypeMethodName = nameof(Type.GetType);

            // Method with return type
            var fromTypeMethodDeclaration = MethodDeclaration(SyntaxKind.StringKeyword.TypeSyntax() ,getGraphQLNameFromTypeMethodName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword));
            var fromTypeThisMethodDeclaration = fromTypeMethodDeclaration;

            var typeParameterSyntax = Parameter(typeParameterName.Identifier()).WithType(typeParameterType.TypeSyntax());
            var thisObjectParameterSyntax = Parameter(typeParameterName.Identifier())
                                                .WithType(objectParameterTypeName.TypeSyntax())
                                                .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword))); ;

            var fromTypeMethodParameters = new List<ParameterSyntax> { typeParameterSyntax };
            var fromTypeThisMethodParameters = new List<ParameterSyntax> { thisObjectParameterSyntax };

            var attributeTypeName = nameof(System.Attribute);
            var attributeMethodName = nameof(System.Attribute.GetCustomAttribute);
            var attributeVariableName = nameof(System.Attribute).ToLower();

            var invokeGetPropertyMethod = InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    typeParameterName.IdentifierName(),
                    typeGetTypeMethodName.IdentifierName()
                ));

            var variableDeclaration = LocalDeclarationStatement(VariableDeclaration(Token(SyntaxKind.VarKeyword).Text.IdentifierName())
                .WithVariables(SeparatedList(new List<VariableDeclaratorSyntax>
                {
                    VariableDeclarator(typeVariableName)
                        .WithInitializer(EqualsValueClause(invokeGetPropertyMethod))
                })));

            var thisInvokeAttributeMethod = InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, IdentifierName(attributeTypeName),
                    IdentifierName(attributeMethodName))).
                WithArgumentList(ArgumentList(SeparatedList(new List<ArgumentSyntax>
                {
                    Argument(typeVariableName.IdentifierName()),
                    Argument(TypeOfExpression(graphQLTypeAttributeName.IdentifierName())),
                })));

            var invokeAttributeMethod = InvocationExpression(MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, IdentifierName(attributeTypeName),
                IdentifierName(attributeMethodName))).
                    WithArgumentList(ArgumentList(SeparatedList(new List<ArgumentSyntax>
                    {
                        Argument(typeParameterName.IdentifierName()),
                        Argument(TypeOfExpression(graphQLTypeAttributeName.IdentifierName())),
                    })));

            var declarationPattern = DeclarationPattern(graphQLTypeAttributeName.IdentifierName(),
                SingleVariableDesignation(attributeVariableName.Identifier()));

            var ifStatement = IfStatement(
                IsPatternExpression(invokeAttributeMethod, declarationPattern)
                    .WithIsKeyword(Token(SyntaxKind.IsKeyword)),
                ReturnStatement(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    attributeVariableName.IdentifierName(), namePropertyName.IdentifierName())));

            var thisIfStatement = IfStatement(
                IsPatternExpression(thisInvokeAttributeMethod, declarationPattern)
                    .WithIsKeyword(Token(SyntaxKind.IsKeyword)),
                ReturnStatement(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    attributeVariableName.IdentifierName(), namePropertyName.IdentifierName())));

            var thisReturnStatement = ReturnStatement(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        typeVariableName.IdentifierName(),
                        namePropertyName.IdentifierName())
                    .WithOperatorToken(Token(SyntaxKind.DotToken)));

            var returnStatement = ReturnStatement(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        typeParameterName.IdentifierName(),
                        namePropertyName.IdentifierName())
                    .WithOperatorToken(Token(SyntaxKind.DotToken)));

            fromTypeMethodDeclaration = fromTypeMethodDeclaration.AddParameterListParameters(fromTypeMethodParameters.ToArray())
                .WithBody(Block(ifStatement, returnStatement));
            fromTypeThisMethodDeclaration = fromTypeThisMethodDeclaration.AddParameterListParameters(fromTypeThisMethodParameters.ToArray())
                .WithBody(Block(variableDeclaration, thisIfStatement, thisReturnStatement));

            return declaration.AddMembers(fromTypeMethodDeclaration, fromTypeThisMethodDeclaration);
        }

        private ClassDeclarationSyntax GenerateMemberAttributeHelpers(ClassDeclarationSyntax declaration)
        {
            // TODO magic strings
            var getGraphQLNameFromMemberMethodName = "GetGraphQLNameFromMember";
            var namePropertyName = nameof(Type.Name);
            var typeGetPropertyMethodName = nameof(Type.GetProperty);
            var propertyInfoVariableName = "propertyInfo";

            var graphQLMemberAttributeName = nameof(GraphQLMemberAttribute);

            // Method parameters
            var memberParameterName = "memberName";
            var typeParameterName = nameof(Type).ToLower();
            var typeParameterType = nameof(Type);

            // Method with return type
            var fromMemberMethodDeclaration = MethodDeclaration(SyntaxKind.StringKeyword.TypeSyntax(), getGraphQLNameFromMemberMethodName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword));

            var typeParameterSyntax = Parameter(typeParameterName.Identifier()).WithType(typeParameterType.TypeSyntax());
            var memberParameterSyntax = Parameter(memberParameterName.Identifier()).WithType(SyntaxKind.StringKeyword.TypeSyntax());
            var fromMemberMethodParameters = new List<ParameterSyntax> { typeParameterSyntax, memberParameterSyntax };

            var attributeTypeName = nameof(System.Attribute);
            var attributeMethodName = nameof(System.Attribute.GetCustomAttribute);
            var attributeVariableName = nameof(System.Attribute).ToLower();


            var invokeGetPropertyMethod = InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    typeParameterName.IdentifierName(),
                    typeGetPropertyMethodName.IdentifierName()
                )).
                WithArgumentList(ArgumentList(SeparatedList(new List<ArgumentSyntax>
                {
                    Argument(memberParameterName.IdentifierName()),
                })));
            var variableDeclaration = LocalDeclarationStatement(VariableDeclaration(Token(SyntaxKind.VarKeyword).Text.IdentifierName())
                .WithVariables(SeparatedList(new List<VariableDeclaratorSyntax>
                {
                    VariableDeclarator(propertyInfoVariableName)
                        .WithInitializer(EqualsValueClause(invokeGetPropertyMethod))
                })));

            var invokeAttributeMethod = InvocationExpression(MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, IdentifierName(attributeTypeName),
                IdentifierName(attributeMethodName))).
                    WithArgumentList(ArgumentList(SeparatedList(new List<ArgumentSyntax>
                    {
                        Argument(typeParameterName.IdentifierName()),
                        Argument(TypeOfExpression(graphQLMemberAttributeName.IdentifierName())),
                    })));

            var declarationPattern = DeclarationPattern(graphQLMemberAttributeName.IdentifierName(),
                SingleVariableDesignation(attributeVariableName.Identifier()));


            var propInfoBinaryExpression = BinaryExpression(SyntaxKind.NotEqualsExpression,
                propertyInfoVariableName.IdentifierName(),
                LiteralExpression(SyntaxKind.NullLiteralExpression));

            var isPatternExpression = IsPatternExpression(invokeAttributeMethod, declarationPattern)
                .WithIsKeyword(Token(SyntaxKind.IsKeyword));

            var returnIfMemberExpression = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                attributeVariableName.IdentifierName(), namePropertyName.IdentifierName());

            var returnIfStatementExpression = ReturnStatement(returnIfMemberExpression);

            var ifBinaryExpression = BinaryExpression(SyntaxKind.LogicalAndExpression,
                propInfoBinaryExpression,
                isPatternExpression);

            var ifStatement = IfStatement(
                ifBinaryExpression,
                returnIfStatementExpression
                );

            var returnStatement = ReturnStatement(memberParameterName.IdentifierName());

            fromMemberMethodDeclaration = fromMemberMethodDeclaration.AddParameterListParameters(fromMemberMethodParameters.ToArray())
                .WithBody(Block(variableDeclaration, ifStatement, returnStatement));

            return declaration.AddMembers(fromMemberMethodDeclaration);
        }

        private ClassDeclarationSyntax GenerateMethodAttributeHelpers(ClassDeclarationSyntax declaration)
        {
            // TODO magic strings
            var getGraphQLNameFromMethodMethodName = "GetGraphQLNameFromMethod";
            var memberInfoVariableName = "memberInfo";
            var graphQLMethodAttributeName = nameof(GraphQLMethodAttribute);

            // Method parameters
            var methodParameterName = "methodName";
            var typeGetMethodMethodName = nameof(Type.GetMethod);
            var typeParameterName = nameof(Type).ToLower();
            var typeParameterType = nameof(Type);
            var namePropertyName = nameof(Type.Name);

            // Method with return type
            var fromMethodMethodDeclaration = MethodDeclaration(SyntaxKind.StringKeyword.TypeSyntax(), getGraphQLNameFromMethodMethodName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword));

            var typeParameterSyntax = Parameter(typeParameterName.Identifier()).WithType(typeParameterType.TypeSyntax());
            var methodParameterSyntax = Parameter(methodParameterName.Identifier()).WithType(SyntaxKind.StringKeyword.TypeSyntax());

            var fromMethodMethodParameters = new List<ParameterSyntax> { typeParameterSyntax, methodParameterSyntax };

            var attributeTypeName = nameof(System.Attribute);
            var attributeMethodName = nameof(System.Attribute.GetCustomAttribute);
            var attributeVariableName = nameof(System.Attribute).ToLower();


            var invokeGetPropertyMethod = InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    typeParameterName.IdentifierName(),
                    typeGetMethodMethodName.IdentifierName()
                )).
                WithArgumentList(ArgumentList(SeparatedList(new List<ArgumentSyntax>
                {
                    Argument(methodParameterName.IdentifierName()),
                })));

            var variableDeclaration = LocalDeclarationStatement(
                VariableDeclaration(Token(SyntaxKind.VarKeyword).Text.IdentifierName())
                .WithVariables(SeparatedList(new List<VariableDeclaratorSyntax>
                {
                    VariableDeclarator(memberInfoVariableName)
                        .WithInitializer(EqualsValueClause(invokeGetPropertyMethod))
                })));

            var invokeAttributeMethod = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression, 
                        IdentifierName(attributeTypeName),
                IdentifierName(attributeMethodName))).
                    WithArgumentList(ArgumentList(SeparatedList(new List<ArgumentSyntax>
                    {
                        Argument(typeParameterName.IdentifierName()),
                        Argument(TypeOfExpression(graphQLMethodAttributeName.IdentifierName())),
                    })));

            var declarationPattern = DeclarationPattern(graphQLMethodAttributeName.IdentifierName(),
                SingleVariableDesignation(attributeVariableName.Identifier()));

            var ifStatement = IfStatement(
                BinaryExpression(SyntaxKind.LogicalAndExpression,
                    BinaryExpression(SyntaxKind.NotEqualsExpression, memberInfoVariableName.IdentifierName(),
                        LiteralExpression(SyntaxKind.NullLiteralExpression)),
                    IsPatternExpression(invokeAttributeMethod, declarationPattern)
                        .WithIsKeyword(Token(SyntaxKind.IsKeyword))),
                ReturnStatement(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    attributeVariableName.IdentifierName(), namePropertyName.IdentifierName())));

            var returnStatement = ReturnStatement(methodParameterName.IdentifierName());

            fromMethodMethodDeclaration = fromMethodMethodDeclaration.AddParameterListParameters(fromMethodMethodParameters.ToArray())
                .WithBody(Block(variableDeclaration ,ifStatement, returnStatement));

            return declaration.AddMembers(fromMethodMethodDeclaration);
        }

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
                                    .WithBody(Block());

            var baseHttpClientInitializer = ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                                    .AddArgumentListArguments(Argument(IdentifierName("httpClient")));

            var httpClientConstructorDeclaration = ConstructorDeclaration(className)
                                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                                    .AddParameterListParameters(Parameter(Identifier("httpClient")).WithType(ParseTypeName("HttpClient")))
                                    .WithInitializer(baseHttpClientInitializer)
                                    .WithBody(Block());

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
                var argumentSyntax = Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal($"{field.Name}")));

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


        private (string, Type?) GetMappedType(string name)
        {
            return TypeMapping.ContainsKey(name) ? TypeMapping[name] : new(name.NormalizeIfNeeded(options), null);
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