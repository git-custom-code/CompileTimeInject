namespace CustomCode.CompileTimeInject.ContainerGenerator.Metadata
{
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection.Metadata;

    /// <summary>
    /// Implementation of the <see cref="ISignatureTypeProvider{TType, TGenericContext}"/> interface that
    /// allows decoding of generic types as <see cref="TypeDescriptor"/> instances.
    /// </summary>
    public sealed class TypeDescriptorSignatureProvider : ISignatureTypeProvider<TypeDescriptor, object?>
    {
        #region Logic

        /// <inheritdoc />
        public TypeDescriptor GetArrayType(TypeDescriptor elementType, ArrayShape shape)
        {
            return new TypeDescriptor($"{elementType}[{new string(',', shape.Rank - 1)}]");
        }

        /// <inheritdoc />
        public TypeDescriptor GetByReferenceType(TypeDescriptor elementType)
        {
            return new TypeDescriptor($"ref {elementType.FullName}*");
        }

        /// <inheritdoc />
        public TypeDescriptor GetFunctionPointerType(MethodSignature<TypeDescriptor> signature)
        {
            throw new System.NotImplementedException("GetFunctionPointerType");
        }

        /// <inheritdoc />
        public TypeDescriptor GetGenericInstantiation(TypeDescriptor genericType, ImmutableArray<TypeDescriptor> typeArguments)
        {
            var index = genericType.FullName.LastIndexOf('`');
            var fullName = genericType.FullName.Substring(0, index);
            var genericArgs = string.Join(",", typeArguments.Select(arg => arg.FullName).ToArray());
            return new TypeDescriptor($"{fullName}<{genericArgs}>");
        }

        /// <inheritdoc />
        public TypeDescriptor GetGenericMethodParameter(object? genericContext, int index)
        {
            throw new System.NotImplementedException("GetGenericTypeParameter");
        }

        /// <inheritdoc />
        public TypeDescriptor GetGenericTypeParameter(object? genericContext, int index)
        {
            throw new System.NotImplementedException("GetGenericTypeParameter");
        }

        /// <inheritdoc />
        public TypeDescriptor GetModifiedType(TypeDescriptor modifier, TypeDescriptor unmodifiedType, bool isRequired)
        {
            throw new System.NotImplementedException("GetModifiedType");
        }

        /// <inheritdoc />
        public TypeDescriptor GetPinnedType(TypeDescriptor elementType)
        {
            throw new System.NotImplementedException("GetPinnedType");
        }

        /// <inheritdoc />
        public TypeDescriptor GetPointerType(TypeDescriptor elementType)
        {
            return new TypeDescriptor($"{elementType.FullName}*");
        }

        /// <inheritdoc />
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
                    throw new System.NotImplementedException();
            }
        }

        /// <inheritdoc />
        public TypeDescriptor GetSZArrayType(TypeDescriptor elementType)
        {
            return new TypeDescriptor($"{elementType.FullName}[]");
        }

        /// <inheritdoc />
        public TypeDescriptor GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var definition = reader.GetTypeDefinition(handle);
            return reader.ToTypeDescriptor(definition);
        }

        /// <inheritdoc />
        public TypeDescriptor GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var reference = reader.GetTypeReference(handle);
            return reader.ToTypeDescriptor(reference);
        }

        /// <inheritdoc />
        public TypeDescriptor GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            throw new System.NotImplementedException("GetTypeFromSpecification");
        }

        #endregion
    }
}
