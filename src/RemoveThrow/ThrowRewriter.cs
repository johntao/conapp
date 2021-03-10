using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConApp
{
    public class ThrowRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _model;
        public ThrowRewriter(SemanticModel semanticModel) => _model = semanticModel;
        public override SyntaxNode VisitCatchClause(CatchClauseSyntax parentNode)
        {
            if (parentNode.Declaration == null) return parentNode;
            var id = parentNode.Declaration.Identifier;
            var oldNode = parentNode.DescendantNodes().OfType<ThrowStatementSyntax>().FirstOrDefault();
            if (oldNode == null) return parentNode; // return if throw not found
            if (oldNode.Expression == null) return parentNode; // return if "throw;"
            var arr = oldNode.Expression.ChildTokens();
            if (!arr.Any(q => q.IsEquivalentTo(id))) return parentNode;
            var newNode = oldNode.Update(
                oldNode.ThrowKeyword.WithTrailingTrivia(null),
                null!,
                oldNode.SemicolonToken);
            return parentNode.ReplaceNode(oldNode, newNode);
        }
    }
}