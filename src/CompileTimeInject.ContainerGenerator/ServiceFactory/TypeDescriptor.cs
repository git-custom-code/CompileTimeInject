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
            FullName = $"{@namespace}.{name}";
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
                return FullName.AsSpan().Slice(0, namespaceLength - 1);
            }
        }

        #endregion
    }
}
