namespace CustomCode.CompileTimeInject.ContainerGenerator.Syntax
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// A <see cref="CSharpSyntaxWalker"/> that collects all class names.
    /// </summary>
    public sealed class ClassWalker : CSharpSyntaxWalker
    {
        #region Dependencies

        /// <summary>
        /// Creates a new instance of the <see cref="ClassWalker"/> type.
        /// </summary>
        public ClassWalker()
            : base(SyntaxWalkerDepth.Node)
        { }

        #endregion

        #region Data

        /// <summary>
        /// Gets the names of all visited class.
        /// </summary>
        public IEnumerable<string> FoundClasses
        {
            get { return _foundClasses; }
        }

        /// <summary> Backing field for the <see cref="FoundClasses"/> property. </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<string> _foundClasses = new List<string>();

        #endregion

        #region Logic

        /// <inheritdoc />
        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _foundClasses.Add(node.Identifier.ValueText);
            base.VisitClassDeclaration(node);
        }

        #endregion
    }
}
