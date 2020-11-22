namespace CustomCode.CompileTimeInject.ContainerGenerator.Metadata
{
    using Annotations;
    using System;
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
        /// <param name="serviceId"> An optional and unique identifier for the service. </param>
        public ServiceDescriptor(
            TypeDescriptor implementation,
            IEnumerable<DependencyDescriptor>? dependencies = null,
            Lifetime lifetime = Lifetime.Transient,
            string? serviceId = null)
            : this(implementation, implementation, dependencies, lifetime, serviceId)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceDescriptor"/> type.
        /// </summary>
        /// <param name="contract"> The described service's implemented contract type. </param>
        /// <param name="implementation"> The described service's implementation type. </param>
        /// <param name="dependencies"> The service's (constructor) dependencies, that need to be injected. </param>
        /// <param name="lifetime"> The service's lifetime policy. </param>
        /// <param name="serviceId"> An optional and unique identifier for the service. </param>
        public ServiceDescriptor(
            TypeDescriptor contract,
            TypeDescriptor implementation,
            IEnumerable<DependencyDescriptor>? dependencies = null,
            Lifetime lifetime = Lifetime.Transient,
            string? serviceId = null)
        {
            Contract = contract;
            Dependencies = dependencies ?? Enumerable.Empty<DependencyDescriptor>();
            Implementation = implementation;
            Lifetime = lifetime;
            ServiceId = serviceId;
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
        public IEnumerable<DependencyDescriptor> Dependencies { get; }

        /// <summary>
        /// Gets the service's lifetime policy.
        /// </summary>
        public Lifetime Lifetime { get; }

        /// <summary>
        /// Gets an optional and unique identifier for the service.
        /// </summary>
        public string? ServiceId { get; }

        #endregion

        #region Logic

        /// <summary>
        /// Compares two <see cref="ServiceDescriptor"/> instances for equality.
        /// </summary>
        /// <param name="left"> The operator's left hand side argument. </param>
        /// <param name="right"> The operator's right hand side argument. </param>
        /// <returns> True if both <see cref="ServiceDescriptor"/> instances are equal, false otherwise. </returns>
        public static bool operator ==(ServiceDescriptor left, ServiceDescriptor right)
        {
            return string.Equals(left.Implementation.FullName, right.Implementation.FullName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(left.Contract.FullName, right.Contract.FullName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(left.ServiceId, right.ServiceId, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares two <see cref="ServiceDescriptor"/> instances for inequality.
        /// </summary>
        /// <param name="left"> The operator's left hand side argument. </param>
        /// <param name="right"> The operator's right hand side argument. </param>
        /// <returns> False if both <see cref="ServiceDescriptor"/> instances are equal, true otherwise. </returns>
        public static bool operator !=(ServiceDescriptor left, ServiceDescriptor right)
        {
            return !string.Equals(left.Implementation.FullName, right.Implementation.FullName, StringComparison.OrdinalIgnoreCase) ||
                 !string.Equals(left.Contract.FullName, right.Contract.FullName, StringComparison.OrdinalIgnoreCase) ||
                 !string.Equals(left.ServiceId, right.ServiceId, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is ServiceDescriptor service)
            {
                return string.Equals(Implementation.FullName, service.Implementation.FullName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(Contract.FullName, service.Contract.FullName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(ServiceId, service.ServiceId, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = Contract.FullName.GetHashCode();
            if (Contract != Implementation)
            {
                hashCode = hashCode * 17 + Implementation.FullName.GetHashCode();
            }
            if (ServiceId != null)
            {
                hashCode = hashCode * 17 + ServiceId.GetHashCode();
            }
            return hashCode;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Contract == Implementation)
            {
                if (ServiceId == null)
                {
                    return Implementation.FullName;
                }
                return $"{Implementation.FullName} (Id: {ServiceId})";
            }

            if (ServiceId == null)
            {
                return $"{Implementation.FullName} : {Contract.FullName}";
            }
            return $"{Implementation.FullName} : {Contract.FullName} (Id: {ServiceId})";
        }

        #endregion
    }
}
