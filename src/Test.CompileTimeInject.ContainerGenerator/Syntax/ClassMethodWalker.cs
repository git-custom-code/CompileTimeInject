namespace CustomCode.CompileTimeInject.ContainerGenerator.Syntax
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A <see cref="CSharpSyntaxWalker"/> that collects all method implementations of visited classes.
    /// </summary>
    public sealed class ClassMethodWalker : CSharpSyntaxWalker
    {
        #region Dependencies

        /// <summary>
        /// Creates a new instance of the <see cref="ClassMethodWalker"/> type.
        /// </summary>
        public ClassMethodWalker()
            : base(SyntaxWalkerDepth.Node)
        { }

        #endregion

        #region Data

        /// <summary>
        /// Gets the source code of all methods in all visited classes.
        /// </summary>
        public IDictionary<string, List<string>> FoundMethodsByClass { get; } =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Logic

        /// <inheritdoc />
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Parent is ClassDeclarationSyntax @class)
            {
                var className = @class.Identifier.ValueText;
                var sourceCode = string.Join(
                    Environment.NewLine,
                    node.ToString().Split(Environment.NewLine).Select(s => s.Trim()));

                if (FoundMethodsByClass.TryGetValue(className, out var methods))
                {
                    methods.Add(sourceCode);
                }
                else
                {
                    FoundMethodsByClass.Add(className, new List<string> { sourceCode });
                }
            }

            base.VisitMethodDeclaration(node);
        }

        #endregion
    }
}
