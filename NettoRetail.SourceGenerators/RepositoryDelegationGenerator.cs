using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NettoRetail.SourceGenerators;

[Generator]
public class RepositoryDelegationGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Adding debug hooks
        context.RegisterForSyntaxNotifications(() => new ClassCollector());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not ClassCollector collector)
            return;

        Compilation compilation = context.Compilation;

        foreach (var candidate in collector.CandidateClasses)
        {
            var model = compilation.GetSemanticModel(candidate.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(candidate) as INamedTypeSymbol;

            if (classSymbol is null) continue;

            var interfaceMethods = classSymbol
                .AllInterfaces
                .SelectMany(i => i.GetMembers())
                .Distinct(SymbolEqualityComparer.Default)
                .OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Ordinary);

            var generatedMethods = new StringBuilder();

            foreach (var method in interfaceMethods)
            {
                string returnType = method.ReturnType.ToDisplayString();
                string methodName = method.Name;
                var parameters = method.Parameters
                    .Select(p => $"{p.Type.ToDisplayString()} {p.Name}")
                    .ToArray();
                var parameterNames = method.Parameters
                    .Select(p => p.Name)
                    .ToArray();

                string parametersString = string.Join(", ", parameters);
                string argsString = string.Join(", ", parameterNames);

                generatedMethods.AppendLine($@"
                    public {returnType} {methodName}({parametersString}) =>
                        _inner.{methodName}({argsString});
                ");
            }

            string ns = classSymbol.ContainingNamespace.ToDisplayString();
            string className = classSymbol.Name;

            string generatedCode = $@"
                using System;
                namespace {ns}
                {{
                    public partial class {className}
                    {{
                {generatedMethods}
                    }}
                }}";

            context.AddSource($"{className}.Delegated.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
        }
    }

    private class ClassCollector : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds &&
                cds.BaseList?.Types.Any() == true)
            {
                CandidateClasses.Add(cds);
            }
        }
    }
}
