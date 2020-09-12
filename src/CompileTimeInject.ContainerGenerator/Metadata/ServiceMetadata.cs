namespace CustomCode.CompileTimeInject.ContainerGenerator.Metadata
{
    using System.Reflection.Metadata;

    /// <summary>
    /// Small data object that contains the metadata of an exported service.
    /// </summary>
    public readonly struct ServiceMetadata
    {
        #region Dependencies

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceMetadata"/> type.
        /// </summary>
        /// <param name="exportAttribute"> The service's <see cref="ExportAttribute"/> data. </param>
        /// <param name="typeDefinition"> The service's <see cref="TypeDefinition"/>. </param>
        public ServiceMetadata(CustomAttributeValue<TypeDescriptor> exportAttribute, TypeDefinition typeDefinition)
        {
            ExportAttribute = exportAttribute;
            TypeDefinition = typeDefinition;
        }

        #endregion

        #region Data

        /// <summary>
        /// The service's <see cref="ExportAttribute"/> data.
        /// </summary>
        public CustomAttributeValue<TypeDescriptor> ExportAttribute { get; }

        /// <summary>
        /// The service's <see cref="TypeDefinition"/>.
        /// </summary>
        public TypeDefinition TypeDefinition { get; }

        #endregion
    }
}
