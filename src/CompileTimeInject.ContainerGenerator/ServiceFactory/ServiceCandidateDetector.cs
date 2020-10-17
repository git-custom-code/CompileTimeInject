namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// <see cref="ISyntaxReceiver"/> implementation that will search the current <see cref="Compilation"/>
    /// for all types (i.e. <see cref="ClassDeclarationSyntax"/>) that are annotated with an attribute whose
    /// name starts with "Export".
    /// </summary>
    public sealed class ServiceCandidateDetector : ISyntaxReceiver
    {
        #region Data

        /// <summary>
        /// The name of the CustomCode.CompileTimeInject.Annotations.ExportAttribute.
        /// </summary>
        private const string ExportAttributeName = "Export";

        #region ServiceCandidates

        /// <summary>
        /// Gets a collection of potential candidates for exported service classes.
        /// </summary>
        public IEnumerable<TypeDeclarationSyntax> ServiceCandidates
        {
            get { return _serviceCandidates; }
        }

        /// <summary> Backing field for the <see cref="ServiceCandidates"/> property. </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<TypeDeclarationSyntax> _serviceCandidates = new List<TypeDeclarationSyntax>();

        #endregion

        #endregion

        #region Logic

        /// <inheritdoc />
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classSyntax)
            {
                foreach(var attribute in classSyntax.AttributeLists.SelectMany(list => list.Attributes))
                {
                    if (attribute.Name.ToString().StartsWith(ExportAttributeName, StringComparison.OrdinalIgnoreCase))
                    {
                        _serviceCandidates.Add(classSyntax);
                        return;
                    }
                }
            }
            else if (syntaxNode is StructDeclarationSyntax typeSyntax)
            {
                foreach (var attribute in typeSyntax.AttributeLists.SelectMany(list => list.Attributes))
                {
                    if (attribute.Name.ToString().StartsWith(ExportAttributeName, StringComparison.OrdinalIgnoreCase))
                    {
                        _serviceCandidates.Add(typeSyntax);
                        return;
                    }
                }
            }
        }

        #endregion
    }
}
