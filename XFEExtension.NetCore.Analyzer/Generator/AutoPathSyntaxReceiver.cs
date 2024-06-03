using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace XFEExtension.NetCore.Analyzer.Generator
{
    public class AutoPathSyntaxReceiver : ISyntaxReceiver
    {
        public List<FieldDeclarationSyntax> CandidateFields { get; } = new List<FieldDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is FieldDeclarationSyntax fieldDeclarationSyntax)
            {
                if (fieldDeclarationSyntax.AttributeLists.Any(PathPropertyAutoGenerator.IsAutoPathAttribute))
                {
                    CandidateFields.Add(fieldDeclarationSyntax);
                }
            }
        }
    }
}
