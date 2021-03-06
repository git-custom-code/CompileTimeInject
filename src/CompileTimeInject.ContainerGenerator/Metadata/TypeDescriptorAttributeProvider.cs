namespace CustomCode.CompileTimeInject.ContainerGenerator.Metadata
{
    using Annotations;
    using System;
    using System.Reflection.Metadata;

    /// <summary>
    /// Specialized implementation of the <see cref="ICustomAttributeTypeProvider{TType}"/> interface
    /// for <see cref="ExportAttribute"/> or <see cref="ImportAttribute"/> deserialization.
    /// </summary>
    public sealed class TypeDescriptorAttributeProvider : ICustomAttributeTypeProvider<TypeDescriptor>
    {
        #region Logic

        /// <inheritdoc cref="ISimpleTypeProvider{TType}" />
        public TypeDescriptor GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            switch (typeCode)
            {
                case PrimitiveTypeCode.Boolean:
                    return new TypeDescriptor("bool");
                case PrimitiveTypeCode.Byte:
                    return new TypeDescriptor("byte");
                case PrimitiveTypeCode.Char:
                    return new TypeDescriptor("char");
                case PrimitiveTypeCode.Double:
                    return new TypeDescriptor("double");
                case PrimitiveTypeCode.Int16:
                    return new TypeDescriptor("short");
                case PrimitiveTypeCode.Int32:
                    return new TypeDescriptor("int");
                case PrimitiveTypeCode.IntPtr:
                    return new TypeDescriptor("System", "IntPtr");
                case PrimitiveTypeCode.Int64:
                    return new TypeDescriptor("long");
                case PrimitiveTypeCode.Object:
                    return new TypeDescriptor("object");
                case PrimitiveTypeCode.SByte:
                    return new TypeDescriptor("sbyte");
                case PrimitiveTypeCode.Single:
                    return new TypeDescriptor("float");
                case PrimitiveTypeCode.String:
                    return new TypeDescriptor("string");
                case PrimitiveTypeCode.UInt16:
                    return new TypeDescriptor("ushort");
                case PrimitiveTypeCode.UInt32:
                    return new TypeDescriptor("uint");
                case PrimitiveTypeCode.UInt64:
                    return new TypeDescriptor("ulong");
                case PrimitiveTypeCode.UIntPtr:
                    return new TypeDescriptor("System", "UIntPtr");
                case PrimitiveTypeCode.Void:
                    return new TypeDescriptor("void");
                default:
                    throw new NotSupportedException($"Type code <{typeCode}> is not supported");
            }
        }

        /// <inheritdoc cref="ICustomAttributeTypeProvider{TType}" />
        public TypeDescriptor GetSystemType()
        {
            return new TypeDescriptor(typeof(Type).Namespace, typeof(Type).Name);
        }

        /// <inheritdoc cref="ISZArrayTypeProvider{TType}" />
        public TypeDescriptor GetSZArrayType(TypeDescriptor elementType)
        {
            return new TypeDescriptor($"{elementType.FullName}[]");
        }

        /// <inheritdoc cref="ISimpleTypeProvider{TType}" />
        public TypeDescriptor GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var definition = reader.GetTypeDefinition(handle);
            return reader.ToTypeDescriptor(definition);
        }

        /// <inheritdoc cref="ISimpleTypeProvider{TType}" />
        public TypeDescriptor GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var reference = reader.GetTypeReference(handle);
            return reader.ToTypeDescriptor(reference); 
        }

        /// <inheritdoc cref="ICustomAttributeTypeProvider{TType}" />
        public TypeDescriptor GetTypeFromSerializedName(string name)
        {
            return new TypeDescriptor(name);
        }

        /// <inheritdoc cref="ICustomAttributeTypeProvider{TType}" />
        public PrimitiveTypeCode GetUnderlyingEnumType(TypeDescriptor type)
        {
            if (type.FullName == $"{typeof(Lifetime).FullName}")
            {
                return PrimitiveTypeCode.Byte;
            }

            return PrimitiveTypeCode.Int32;
        }

        /// <inheritdoc cref="ICustomAttributeTypeProvider{TType}" />
        public bool IsSystemType(TypeDescriptor type)
        {
            return type.FullName == typeof(Type).FullName;
        }

        #endregion
    }
}
