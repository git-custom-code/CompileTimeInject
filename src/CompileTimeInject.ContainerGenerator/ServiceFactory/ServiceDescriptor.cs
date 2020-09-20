namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using Annotations;
    using System.Collections.Generic;
    using System.Linq;

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
        /// <param name="dependencies"> The service's (constructor) dependencies, that need to be injected. </param>
        /// <param name="lifetime"> The service's lifetime policy. </param>
        public ServiceDescriptor(
            TypeDescriptor implementation,
            IEnumerable<TypeDescriptor>? dependencies = null,
            Lifetime lifetime = Lifetime.Transient)
            : this(implementation, implementation, dependencies, lifetime)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceDescriptor"/> type.
        /// </summary>
        /// <param name="contract"> The described service's implemented contract type. </param>
        /// <param name="implementation"> The described service's implementation type. </param>
        /// <param name="dependencies"> The service's (constructor) dependencies, that need to be injected. </param>
        /// <param name="lifetime"> The service's lifetime policy. </param>
        public ServiceDescriptor(
            TypeDescriptor contract,
            TypeDescriptor implementation,
            IEnumerable<TypeDescriptor>? dependencies = null,
            Lifetime lifetime = Lifetime.Transient)
        {
            Contract = contract;
            Dependencies = dependencies ?? Enumerable.Empty<TypeDescriptor>();
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
        /// Gets the service's (constructor) dependencies, that need to be injected.
        /// </summary>
        public IEnumerable<TypeDescriptor> Dependencies { get; }

        /// <summary>
        /// Gets the service's lifetime policy.
        /// </summary>
        public Lifetime Lifetime { get; }

        #endregion
    }
}
