namespace CustomCode.CompileTimeInject.ContainerGenerator.Syntax
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A <see cref="CSharpSyntaxWalker"/> that collects all method signatures of visited types.
    /// </summary>
    public sealed class TypeMethodSignatureWalker : CSharpSyntaxWalker
    {
        #region Dependencies

        /// <summary>
        /// Creates a new instance of the <see cref="TypeMethodSignatureWalker"/> type.
        /// </summary>
        public TypeMethodSignatureWalker()
            : base(SyntaxWalkerDepth.Node)
        { }

        #endregion

        #region Data

        /// <summary>
        /// Gets the signatures of all methods in all visited types.
        /// </summary>
        public IDictionary<string, List<string>> FoundMethodSignaturesByType { get; } =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Logic

        /// <inheritdoc cref="CSharpSyntaxVisitor" />
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Parent is ClassDeclarationSyntax @class)
            {
                var typeName = @class.Identifier.ValueText;
                AddMethodImplementation(typeName, node);
            }
            else if (node.Parent is StructDeclarationSyntax @struct)
            {
                var typeName = @struct.Identifier.ValueText;
                AddMethodImplementation(typeName, node);
            }

            base.VisitMethodDeclaration(node);
        }

        /// <summary>
        /// Adds the (normalized) signature of the given <paramref name="method"/> to the
        /// <see cref="FoundMethodSignaturesByType"/> dictionary.
        /// </summary>
        /// <param name="typeName">
        /// The name of the <see cref="Type"/> that contains the <paramref name="method"/>.
        /// </param>
        /// <param name="method"> The method whose signature to collect. </param>
        private void AddMethodImplementation(string typeName, MethodDeclarationSyntax method)
        {
            var signature = method.ToString();
            var bodyIndex = signature.IndexOf("{");
            if (bodyIndex < 0)
            {
                bodyIndex = signature.IndexOf(";");
            }
            signature = signature.Substring(0, bodyIndex);
            signature = string.Join(" ", signature.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
            signature = string.Join(" ", signature.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            signature = signature.Trim();

            if (FoundMethodSignaturesByType.TryGetValue(typeName, out var methodSignatures))
            {
                methodSignatures.Add(signature);
            }
            else
            {
                FoundMethodSignaturesByType.Add(typeName, new List<string> { signature });
            }
        }

        #endregion
    }
}
