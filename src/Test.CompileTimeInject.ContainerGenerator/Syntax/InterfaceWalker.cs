namespace CustomCode.CompileTimeInject.ContainerGenerator.Syntax
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// A <see cref="CSharpSyntaxWalker"/> that collects all interface names.
    /// </summary>
    public sealed class InterfaceWalker : CSharpSyntaxWalker
    {
        #region Dependencies

        /// <summary>
        /// Creates a new instance of the <see cref="InterfaceWalker"/> type.
        /// </summary>
        public InterfaceWalker()
            : base(SyntaxWalkerDepth.Node)
        { }

        #endregion

        #region Data

        /// <summary>
        /// Gets the names of all visited interfaces.
        /// </summary>
        public IEnumerable<string> FoundInterfaces
        {
            get { return _foundInterfaces; }
        }

        /// <summary> Backing field for the <see cref="FoundInterfaces"/> property. </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<string> _foundInterfaces = new List<string>();

        #endregion

        #region Logic

        /// <inheritdoc />
        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            _foundInterfaces.Add(node.Identifier.ValueText);
            base.VisitInterfaceDeclaration(node);
        }

        #endregion
    }
}
