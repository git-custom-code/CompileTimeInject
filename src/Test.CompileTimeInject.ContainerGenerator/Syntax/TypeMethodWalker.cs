namespace CustomCode.CompileTimeInject.ContainerGenerator.Syntax
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A <see cref="CSharpSyntaxWalker"/> that collects all method implementations of visited types.
    /// </summary>
    public sealed class TypeMethodWalker : CSharpSyntaxWalker
    {
        #region Dependencies

        /// <summary>
        /// Creates a new instance of the <see cref="TypeMethodWalker"/> type.
        /// </summary>
        public TypeMethodWalker()
            : base(SyntaxWalkerDepth.Node)
        { }

        #endregion

        #region Data

        /// <summary>
        /// Gets the source code of all methods in all visited types.
        /// </summary>
        public IDictionary<string, List<string>> FoundMethodsByType { get; } =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Logic

        /// <inheritdoc />
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
        /// Adds the (normalized) source code of the given <paramref name="method"/> to the
        /// <see cref="FoundMethodsByType"/> dictionary.
        /// </summary>
        /// <param name="typeName"> </param>
        /// <param name="method"></param>
        private void AddMethodImplementation(string typeName, MethodDeclarationSyntax method)
        {
            var sourceCode = string.Join(
                    Environment.NewLine,
                    method.ToString().Split(Environment.NewLine).Select(s => s.Trim()));

            if (FoundMethodsByType.TryGetValue(typeName, out var methods))
            {
                methods.Add(sourceCode);
            }
            else
            {
                FoundMethodsByType.Add(typeName, new List<string> { sourceCode });
            }
        }

        #endregion
    }
}
