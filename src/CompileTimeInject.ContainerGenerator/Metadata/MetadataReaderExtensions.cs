namespace CustomCode.CompileTimeInject.ContainerGenerator.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Metadata;

    /// <summary>
    /// Extension methods for the <see cref="MetadataReader"/> type.
    /// </summary>
    public static class MetadataReaderExtensions
    {
        #region Logic

        /// <summary>
        /// Gets a collection of <see cref="TypeDescriptor"/>s for each constructor dependency that needs
        /// to be injected for a given <paramref name="type"/> instance.
        /// </summary>
        /// <param name="reader"> The extended <see cref="MetadataReader"/>. </param>
        /// <param name="type"> The type whose constructor dependencies should be returned. </param>
        /// <returns> A collection of <see cref="TypeDescriptor"/>s for constructor dependency. </returns>
        public static IEnumerable<TypeDescriptor> GetConstructorDependencies(this MetadataReader reader, TypeDefinition type)
        {
            var dependencies = new List<TypeDescriptor>();
            foreach (var handle in type.GetMethods())
            {
                var method = reader.GetMethodDefinition(handle);
                if ((method.Attributes & MethodAttributes.SpecialName) != 0 &&
                    (method.Attributes & MethodAttributes.Public) != 0 &&
                    reader.GetString(method.Name) == ".ctor")
                {
                    var signature = method.DecodeSignature(new TypeDescriptorSignatureProvider(), null);
                    foreach (var dependency in signature.ParameterTypes)
                    {
                        dependencies.Add(dependency);
                    }

                    break; // ToDo: How to handle multiple ctor's?
                }
            }
            return dependencies;
        }

        /// <summary>
        /// Gets a collection of <see cref="ServiceMetadata"/>s for all types that are annotated
        /// with an <see cref="ExportAttribute"/>.
        /// </summary>
        /// <param name="reader"> The extended <see cref="MetadataReader"/>. </param>
        /// <returns> A collection of <see cref="ServiceMetadata"/>s for all annotated types. </returns>
        public static IEnumerable<ServiceMetadata> GetExportedServices(this MetadataReader reader)
        {
            var targetAttributeType = typeof(ExportAttribute);
            var targetName = targetAttributeType.Name;
            var targetNamespace = targetAttributeType.Namespace;
            var targetAssemblyName = targetAttributeType.Assembly.GetName().Name;

            var exportedTypes = new List<ServiceMetadata>();
            foreach (var attributeHandle in reader.CustomAttributes)
            {
                var attribute = reader.GetCustomAttribute(attributeHandle);

                // check only attributes that annotate a type at class level
                if (attribute.Parent.Kind == HandleKind.TypeDefinition &&
                    attribute.Constructor.Kind == HandleKind.MemberReference)
                {
                    var memberRef = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                    var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                    var assemblyRef = reader.GetAssemblyReference((AssemblyReferenceHandle)typeRef.ResolutionScope);

                    var assemblyName = reader.GetString(assemblyRef.Name);
                    var attributeName = reader.GetString(typeRef.Name);
                    var attributeNamespace = reader.GetString(typeRef.Namespace);

                    // check if the attribute is an ExportAttribute ...
                    if (string.Equals(targetName, attributeName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(targetNamespace, attributeNamespace, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(targetAssemblyName, assemblyName, StringComparison.OrdinalIgnoreCase))
                    {
                        var exportAttribute = attribute.DecodeValue(new ExportAttributeProvider());
                        var serviceDefinition = reader.GetTypeDefinition((TypeDefinitionHandle)attribute.Parent);
                        exportedTypes.Add(new ServiceMetadata(exportAttribute, serviceDefinition));
                    }
                }
            }

            return exportedTypes;
        }

        /// <summary>
        /// Gets a collection of <see cref="TypeDescriptor"/>s for each implemented interface for the
        /// given <paramref name="type"/>.
        /// </summary>
        /// <param name="reader"> The extended <see cref="MetadataReader"/>. </param>
        /// <param name="type"> The type whose implemented interfaces should be returned. </param>
        /// <returns> A collection of <see cref="TypeDescriptor"/>s for each implemented interface. </returns>
        public static IEnumerable<TypeDescriptor> GetImplementedInterfaces(this MetadataReader reader, TypeDefinition type)
        {
            var interfaceImplementations = type.GetInterfaceImplementations();
            if (interfaceImplementations.Count == 0)
            {
                return Enumerable.Empty<TypeDescriptor>();
            }

            var implementedInterfaces = new List<TypeDescriptor>();
            foreach (var implementationHandle in interfaceImplementations)
            {
                var interfaceImplementation = reader.GetInterfaceImplementation(implementationHandle);

                // interface is declared in the same assembly as the type
                if (interfaceImplementation.Interface.Kind == HandleKind.TypeDefinition)
                {
                    var definition = reader.GetTypeDefinition((TypeDefinitionHandle)interfaceImplementation.Interface);
                    var @interface = reader.ToTypeDescriptor(definition);
                    implementedInterfaces.Add(@interface);
                }
                // interface is declared in another assembly
                else if (interfaceImplementation.Interface.Kind == HandleKind.TypeReference)
                {
                    var reference = reader.GetTypeReference((TypeReferenceHandle)interfaceImplementation.Interface);
                    var @interface = reader.ToTypeDescriptor(reference);
                    implementedInterfaces.Add(@interface);
                }
                // interface is generic
                if (interfaceImplementation.Interface.Kind == HandleKind.TypeSpecification)
                {
                    var specification = reader.GetTypeSpecification((TypeSpecificationHandle)interfaceImplementation.Interface);
                    var @interface = specification.DecodeSignature(new TypeDescriptorSignatureProvider(), null);
                    implementedInterfaces.Add(@interface);
                }
            }
            return implementedInterfaces;
        }

        /// <summary>
        /// Convert the given <paramref name="typeDefinition"/> to a <see cref="TypeDescriptor"/>.
        /// </summary>
        /// <param name="reader"> The extended <see cref="MetadataReader"/>. </param>
        /// <param name="typeDefinition"> The <see cref="TypeDefinition"/> to be converted. </param>
        /// <returns> The converted <see cref="TypeDescriptor"/>. </returns>
        public static TypeDescriptor ToTypeDescriptor(this MetadataReader reader, TypeDefinition typeDefinition)
        {
            var name = reader.GetString(typeDefinition.Name);
            if (typeDefinition.Namespace.IsNil)
            {
                return new TypeDescriptor(name);
            }

            var @namespace = reader.GetString(typeDefinition.Namespace);
            return new TypeDescriptor(@namespace, name);
        }

        /// <summary>
        /// Convert the given <paramref name="typeReferefrence"/> to a <see cref="TypeDescriptor"/>.
        /// </summary>
        /// <param name="reader"> The extended <see cref="MetadataReader"/>. </param>
        /// <param name="typeReferefrence"> The <see cref="TypeReference"/> to be converted. </param>
        /// <returns> The converted <see cref="TypeDescriptor"/>. </returns>
        public static TypeDescriptor ToTypeDescriptor(this MetadataReader reader, TypeReference typeReferefrence)
        {
            var name = reader.GetString(typeReferefrence.Name);
            if (typeReferefrence.Namespace.IsNil)
            {
                return new TypeDescriptor(name);
            }

            var @namespace = reader.GetString(typeReferefrence.Namespace);
            return new TypeDescriptor(@namespace, name);
        }

        #endregion
    }
}
