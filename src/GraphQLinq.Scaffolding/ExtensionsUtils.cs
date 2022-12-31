using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace GraphQLinq.Scaffolding
{
    static class ExtensionsUtils
    {
        internal static string ToCamelCase(this string input)
        {
            if (char.IsLower(input[0]))
            {
                return input;
            }
            return input.Substring(0, 1).ToLower() + input.Substring(1);
        }

        internal static string ToPascalCase(this string input)
        {
            if (char.IsUpper(input[0]))
            {
                return input;
            }
            return input.Substring(0, 1).ToUpper() + input.Substring(1);
        }

        internal static string NormalizeIfNeeded(this string input, CodeGenerationOptions options)
        {
            return options.NormalizeCasing ? input.ToPascalCase() : input;
        }
    }

    static class RoslynUtilities
    {
        internal static SyntaxNode GetTopLevelNode(string? @namespace)
        {
            return string.IsNullOrEmpty(@namespace)
                ? CompilationUnit()
                : NamespaceDeclaration(SyntaxFactory.IdentifierName(@namespace));
        }

        internal static SyntaxNode AddUsings(this SyntaxNode node, UsingDirectiveSyntax usingDirectiveSyntax)
        {
            switch (node)
            {
                case CompilationUnitSyntax compilationUnit:
                    compilationUnit = compilationUnit.AddUsings(usingDirectiveSyntax);
                    return compilationUnit;
                case NamespaceDeclarationSyntax namespaceDeclaration:
                    namespaceDeclaration = namespaceDeclaration.AddUsings(usingDirectiveSyntax);
                    return namespaceDeclaration;
                default:
                    return node;
            }
        }

        internal static SyntaxNode AddMembers(this SyntaxNode node, MemberDeclarationSyntax memberDeclarationSyntax)
        {
            switch (node)
            {
                case CompilationUnitSyntax compilationUnit:
                    compilationUnit = compilationUnit.AddMembers(memberDeclarationSyntax);
                    return compilationUnit;
                case NamespaceDeclarationSyntax namespaceDeclaration:
                    namespaceDeclaration = namespaceDeclaration.AddMembers(memberDeclarationSyntax);
                    return namespaceDeclaration;
                default:
                    return node;
            }
        }

        internal static SyntaxToken Identifier(this string identifier) => SyntaxFactory.Identifier(identifier);
        internal static IdentifierNameSyntax IdentifierName(this string identifier) => SyntaxFactory.IdentifierName(identifier);
        internal static TypeSyntax TypeSyntax(this string type) => ParseTypeName(type);
        internal static TypeSyntax TypeSyntax(this SyntaxKind predefinedType) => PredefinedType(Token(predefinedType));

        internal static ArrayTypeSyntax ArrayTypeSyntax(this SyntaxKind type, int? size = null)
        {
            return ArrayType(type.TypeSyntax())
                .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>( size == null ? OmittedArraySizeExpression() : LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(size.Value)))));
        }
        internal static SeparatedSyntaxList<T> ToSyntaxList<T>(this T expressionSyntax) where T: SyntaxNode
        {
            return SingletonSeparatedList(expressionSyntax);
        }
    }
}