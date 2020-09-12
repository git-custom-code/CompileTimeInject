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
        /// <param name="lifetime"> The service's lifetime policy. </param>
        public ServiceDescriptor(TypeDescriptor implementation, Lifetime lifetime)
            : this(implementation, implementation, lifetime)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceDescriptor"/> type.
        /// </summary>
        /// <param name="contract"> The described service's implemented contract type. </param>
        /// <param name="implementation"> The described service's implementation type. </param>
        /// <param name="lifetime"> The service's lifetime policy. </param>
        public ServiceDescriptor(TypeDescriptor contract, TypeDescriptor implementation, Lifetime lifetime)
        {
            Contract = contract;
            Implementation = implementation;
            Lifetime = lifetime;
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

        /// <summary>
        /// Gets the service's lifetime policy.
        /// </summary>
        public Lifetime Lifetime { get; }

        #endregion
    }
}
