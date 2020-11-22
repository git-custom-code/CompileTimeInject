namespace CustomCode.CompileTimeInject.ContainerGenerator.Metadata
{
    using Annotations;
    using System;

    /// <summary>
    /// A data object that describes a detected constructor dependency that is optionally annotated
    /// with an <see cref="ImportAttribute"/>.
    /// </summary>
    public sealed class DependencyDescriptor
    {
        #region Dependencies

        /// <summary>
        /// Creates a new instance of the <see cref="DependencyDescriptor"/> type.
        /// </summary>
        /// <param name="contract"> The described dependency's implemented contract type. </param>
        /// <param name="serviceId"> An optional and unique identifier for the injected dependency. </param>
        public DependencyDescriptor(
            TypeDescriptor contract,
            string? serviceId = null)
        {
            Contract = contract;
            ServiceId = serviceId;
        }

        #endregion

        #region Data

        /// <summary>
        /// Gets the described dependency's implemented contract type.
        /// </summary>
        public TypeDescriptor Contract { get; }

        /// <summary>
        /// Gets an optional and unique identifier for the injected dependency.
        /// </summary>
        public string? ServiceId { get; }

        #endregion

        #region Logic

        /// <summary>
        /// Compares two <see cref="DependencyDescriptor"/> instances for equality.
        /// </summary>
        /// <param name="left"> The operator's left hand side argument. </param>
        /// <param name="right"> The operator's right hand side argument. </param>
        /// <returns> True if both <see cref="ServiceDescriptor"/> instances are equal, false otherwise. </returns>
        public static bool operator ==(DependencyDescriptor left, DependencyDescriptor right)
        {
            return string.Equals(left.Contract.FullName, right.Contract.FullName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(left.ServiceId, right.ServiceId, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares two <see cref="DependencyDescriptor"/> instances for inequality.
        /// </summary>
        /// <param name="left"> The operator's left hand side argument. </param>
        /// <param name="right"> The operator's right hand side argument. </param>
        /// <returns> False if both <see cref="DependencyDescriptor"/> instances are equal, true otherwise. </returns>
        public static bool operator !=(DependencyDescriptor left, DependencyDescriptor right)
        {
            return !string.Equals(left.Contract.FullName, right.Contract.FullName, StringComparison.OrdinalIgnoreCase) ||
                 !string.Equals(left.ServiceId, right.ServiceId, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc cref="object" />
        public override bool Equals(object obj)
        {
            if (obj is DependencyDescriptor dependency)
            {
                return string.Equals(Contract.FullName, dependency.Contract.FullName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(ServiceId, dependency.ServiceId, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <inheritdoc cref="object" />
        public override int GetHashCode()
        {
            var hashCode = Contract.FullName.GetHashCode();
            if (ServiceId != null)
            {
                hashCode = hashCode * 17 + ServiceId.GetHashCode();
            }
            return hashCode;
        }

        /// <inheritdoc cref="object" />
        public override string ToString()
        {
            if (ServiceId == null)
            {
                return Contract.FullName;
            }
            return $"{Contract.FullName} (Id: {ServiceId})";
        }

        #endregion
    }
}
