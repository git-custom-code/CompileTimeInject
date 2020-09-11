namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    /// <summary>
    /// A data object that describes a detected service type that is annotated
    /// with an <see cref="ExportAttribute"/>.
    /// </summary>
    public sealed class ServiceDescriptor
    {
        #region Dependencies

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceDescriptor"/> type.
        /// </summary>
        /// <param name="implementation"> The described service's implementation type. </param>
        public ServiceDescriptor(TypeDescriptor implementation)
            : this(implementation, implementation)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceDescriptor"/> type.
        /// </summary>
        /// <param name="contract"> The described service's implemented contract type. </param>
        /// <param name="implementation"> The described service's implementation type. </param>
        public ServiceDescriptor(TypeDescriptor contract, TypeDescriptor implementation)
        {
            Contract = contract;
            Implementation = implementation;
        }

        #endregion

        #region Data

        /// <summary>
        /// Gets the described service's implemented contract type.
        /// </summary>
        public TypeDescriptor Contract { get; }

        /// <summary>
        /// Gets the described service's implementation type.
        /// </summary>
        public TypeDescriptor Implementation { get; }

        #endregion
    }
}
