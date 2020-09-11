namespace CustomCode.CompileTimeInject.ContainerGenerator.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Metadata;

    /// <summary>
    /// Extension methods for the <see cref="MetadataReader"/> type.
    /// </summary>
    public static class MetadataReaderExtensions
    {
        #region Logic

        /// <summary>
        /// Get a collection of <see cref="TypeDefinition"/>s for all types that are annotated
        /// with an <see cref="ExportAttribute"/>.
        /// </summary>
        /// <param name="reader"> The extended <see cref="MetadataReader"/>. </param>
        /// <returns> A collection of <see cref="TypeDefinition"/>s for all annotated types. </returns>
        public static IEnumerable<TypeDefinition> GetExportedTypeDefinitions(this MetadataReader reader)
        {
            var targetAttributeType = typeof(ExportAttribute);
            var targetName = targetAttributeType.Name;
            var targetNamespace = targetAttributeType.Namespace;
            var targetAssemblyName = targetAttributeType.Assembly.GetName().Name;

            var exportedTypes = new List<TypeDefinition>();
            foreach (var attributeHandle in reader.CustomAttributes)
            {
                var attribute = reader.GetCustomAttribute(attributeHandle);

                // check only attributes that annotate a type at class level
                if (attribute.Parent.Kind == HandleKind.TypeDefinition &&
                    attribute.Constructor.Kind == HandleKind.MemberReference)
                {
                    var memberRef = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                    var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                    var assemblyRef = reader.GetAssemblyReference((AssemblyReferenceHandle)typeRef.ResolutionScope);

                    var assemblyName = reader.GetString(assemblyRef.Name);
                    var attributeName = reader.GetString(typeRef.Name);
                    var attributeNamespace = reader.GetString(typeRef.Namespace);

                    // check if the attribute is an ExportAttribute ...
                    if (string.Equals(targetName, attributeName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(targetNamespace, attributeNamespace, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(targetAssemblyName, assemblyName, StringComparison.OrdinalIgnoreCase))
                    {
                        var typeDef = reader.GetTypeDefinition((TypeDefinitionHandle)attribute.Parent);
                        exportedTypes.Add(typeDef);
                    }
                }
            }

            return exportedTypes;
        }

        /// <summary>
        /// Convert the given <paramref name="typeDefinition"/> to a <see cref="TypeDescriptor"/>.
        /// </summary>
        /// <param name="reader"> The extended <see cref="MetadataReader"/>. </param>
        /// <param name="typeDefinition"> The <see cref="TypeDefinition"/> that be converted. </param>
        /// <returns> The converted <see cref="TypeDescriptor"/>. </returns>
        public static TypeDescriptor ToTypeDescriptor(this MetadataReader reader, TypeDefinition typeDefinition)
        {
            var name = reader.GetString(typeDefinition.Name);
            if (typeDefinition.Namespace.IsNil)
            {
                return new TypeDescriptor(name);
            }

            var @namespace = reader.GetString(typeDefinition.Namespace);
            return new TypeDescriptor(@namespace, name);
        }

        #endregion
    }
}
