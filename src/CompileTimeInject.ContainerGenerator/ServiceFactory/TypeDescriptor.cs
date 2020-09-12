namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using System;

    /// <summary>
    /// A data object that contains the namespace and name of a type.
    /// </summary>
    public readonly struct TypeDescriptor
    {
        #region Dependencies

        /// <summary>
        /// Creates a new instance of the <see cref="TypeDescriptor"/> type.
        /// </summary>
        /// <param name="name"> The described type's name. </param>
        public TypeDescriptor(string name)
            : this(string.Empty, name)
        { }

        /// <summary>
        /// Creates a new instane of the <see cref="TypeDescriptor"/> type.
        /// </summary>
        /// <param name="namespace"> The described type's namespace. </param>
        /// <param name="name"> The described type's name. </param>
        public TypeDescriptor(string @namespace, string name)
        {
            if (string.IsNullOrEmpty(@namespace))
            {
                FullName = name;
            }
            else
            {
                FullName = $"{@namespace}.{name}";
            }
        }

        #endregion

        #region Data

        /// <summary>
        /// Gets the described type's full name.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Gets the described type's name.
        /// </summary>
        public ReadOnlySpan<char> Name
        {
            get
            {
                var namespaceLength = FullName.LastIndexOf('.');
                if (namespaceLength == -1)
                {
                    return FullName.AsSpan();
                }

                return FullName.AsSpan().Slice(namespaceLength);
            }
        }

        /// <summary>
        /// Gets the described type's namespace.
        /// </summary>
        public ReadOnlySpan<char> Namespace
        {
            get
            {
                var namespaceLength = FullName.LastIndexOf('.');
                if (namespaceLength == -1)
                {
                    return new ReadOnlySpan<char>();
                }

                return FullName.AsSpan().Slice(0, namespaceLength - 1);
            }
        }

        #endregion

        #region Logic

        /// <summary>
        /// Compares two <see cref="TypeDescriptor"/> instances for equality.
        /// </summary>
        /// <param name="left"> The operator's left hand side argument. </param>
        /// <param name="right"> The operator's right hand side argument. </param>
        /// <returns> True if both <see cref="TypeDescriptor"/> instances are equal, false otherwise. </returns>
        public static bool operator == (TypeDescriptor left, TypeDescriptor right)
        {
            return string.Equals(left.FullName, right.FullName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares two <see cref="TypeDescriptor"/> instances for inequality.
        /// </summary>
        /// <param name="left"> The operator's left hand side argument. </param>
        /// <param name="right"> The operator's right hand side argument. </param>
        /// <returns> False if both <see cref="TypeDescriptor"/> instances are equal, true otherwise. </returns>
        public static bool operator !=(TypeDescriptor left, TypeDescriptor right)
        {
            return !string.Equals(left.FullName, right.FullName, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is TypeDescriptor type)
            {
                return string.Equals(FullName, type.FullName, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        #endregion
    }
}
