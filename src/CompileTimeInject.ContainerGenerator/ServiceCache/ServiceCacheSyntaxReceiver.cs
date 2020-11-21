namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Linq;

    /// <summary>
    /// <see cref="ISyntaxReceiver"/> implementation that will search the current <see cref="Compilation"/>
    /// for all types (i.e. <see cref="TypeDeclarationSyntax"/>) that are annotated with an attribute
    /// whose name starts with "Export" and contains the optional ServiceId property.
    /// </summary>
    public sealed class ServiceCacheSyntaxReceiver : ISyntaxReceiver
    {
        #region Data

        /// <summary>
        /// The name of the CustomCode.CompileTimeInject.Annotations.ExportAttribute.
        /// </summary>
        private const string ExportAttributeName = "Export";

        /// <summary>
        /// The name of the ServiceId property.
        /// </summary>
        private const string ServiceIdPropertyName = "ServiceId";

        /// <summary>
        /// True if the current <see cref="Compilation"/> defines an exported named service (with a unique service id),
        /// false otherwise.
        /// </summary>
        public bool UseNamedServices { get; private set; }

        #endregion

        #region Logic

        /// <inheritdoc cref="ISyntaxReceiver" />
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (UseNamedServices)
            {
                return;
            }

            if (syntaxNode is TypeDeclarationSyntax typeSyntax)
            {
                foreach(var attribute in typeSyntax.AttributeLists.SelectMany(list => list.Attributes))
                {
                    var attributeDeclaration = attribute.ToString();
                    if (attributeDeclaration.StartsWith(ExportAttributeName, StringComparison.OrdinalIgnoreCase) &&
                        attributeDeclaration.IndexOf(ServiceIdPropertyName, StringComparison.Ordinal) >= 0)
                    {
                        UseNamedServices = true;
                        return;
                    }
                }
            }
        }

        #endregion
    }
}
