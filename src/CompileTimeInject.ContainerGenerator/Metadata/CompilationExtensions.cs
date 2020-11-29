namespace CustomCode.CompileTimeInject.ContainerGenerator.Metadata
{
    using Annotations;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata;

    /// <summary>
    /// Extension methods for the <see cref="Compilation"/> type.
    /// </summary>
    public static class CompilationExtensions
    {
        #region Logic

        /// <summary>
        /// Co-routine that will return a <see cref="MetadataReader"/> for each referenced .Net assembly.
        /// </summary>
        /// <param name="compilation"> The extended <see cref="Compilation"/>. </param>
        /// <returns> A <see cref="MetadataReader"/> for each referenced .Net assembly. </returns>
        public static IEnumerable<MetadataReader> GetReferencedNetAssemblies(this Compilation compilation)
        {
            foreach (var reference in compilation.References.OfType<PortableExecutableReference>())
            {
                var reader = reference.GetMetadataReader();
                if (reader == null)
                {
                    continue;
                }

                yield return reader;
            }
        }

        /// <summary>
        /// Co-routine that will return a <see cref="MetadataReader"/> for each referenced assembly,
        /// that is annotated with an <see cref="IocVisibleAssemblyAttribute"/>.
        /// </summary>
        /// <param name="compilation"> The extended <see cref="Compilation"/>. </param>
        /// <returns> A <see cref="MetadataReader"/> for each referenced ioc visible assembly. </returns>
        public static IEnumerable<MetadataReader> GetReferencedIocVisibleAssemblies(this Compilation compilation)
        {
            var targetName = typeof(IocVisibleAssemblyAttribute).Name;
            var targetNamespace = typeof(IocVisibleAssemblyAttribute).Namespace;
            var targetAssemblyName = typeof(IocVisibleAssemblyAttribute).Assembly.GetName().Name;

            foreach (var metadata in compilation.GetReferencedNetAssemblies())
            {
                foreach (var attributeHandle in metadata.CustomAttributes)
                {
                    var attribute = metadata.GetCustomAttribute(attributeHandle);
                    // check only attributes that annotate an assembly at assembly level
                    if (attribute.Parent.Kind == HandleKind.AssemblyDefinition &&
                        attribute.Constructor.Kind == HandleKind.MemberReference)
                    {
                        var memberRef = metadata.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                        var typeRef = metadata.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                        var assemblyRef = metadata.GetAssemblyReference((AssemblyReferenceHandle)typeRef.ResolutionScope);

                        var assemblyName = metadata.GetString(assemblyRef.Name);
                        var attributeName = metadata.GetString(typeRef.Name);
                        var attributeNamespace = metadata.GetString(typeRef.Namespace);

                        // check if the attribute is an IocVisibleAssemblyAttribtue ...
                        if (string.Equals(targetName, attributeName, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(targetNamespace, attributeNamespace, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(targetAssemblyName, assemblyName, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return metadata;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compilation"></param>
        /// <returns></returns>
        public static bool IsIocVisibleAssembly(this Compilation compilation)
        {
            var iocVisibleAssembly = typeof(IocVisibleAssemblyAttribute).FullName;
            foreach (var attribute in compilation.Assembly.GetAttributes())
            {
                var name = attribute.AttributeClass?.ToString();
                if (iocVisibleAssembly.Equals(name, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Given a collection o potential <paramref name="serviceCandidates"/>, find the concrete exported
        /// services in the extended <paramref name="compilation"/>.
        /// </summary>
        /// <param name="compilation"> The extended <see cref="Compilation"/>. </param>
        /// <param name="serviceCandidates">
        /// A collection of potential service candidates that should be found within the given <paramref name="compilation"/>.
        /// </param>
        /// <returns> The found collection of exported services. </returns>
        public static IEnumerable<ServiceDescriptor> FindExportedServices(
        this Compilation compilation,
        IEnumerable<TypeDeclarationSyntax> serviceCandidates)
        {
            var foundServices = new List<ServiceDescriptor>();

            foreach (var serviceCandidate in serviceCandidates)
            {
                var semanticModel = compilation.GetSemanticModel(serviceCandidate.SyntaxTree);
                var classSymbol = semanticModel.GetDeclaredSymbol(serviceCandidate);
                if (classSymbol == null)
                {
                    continue;
                }

                foreach (var attribute in classSymbol.GetAttributes())
                {
                    if (typeof(ExportAttribute).FullName.Equals(attribute.AttributeClass?.ToString(), StringComparison.Ordinal))
                    {
                        var options = GetExportOptions(attribute);
                        var implementation = new TypeDescriptor(classSymbol.ToString());
                        var dependencies = GetConstructorDependencies(classSymbol);

                        if (options.contractFilter.HasValue)
                        {
                            foundServices.Add(new ServiceDescriptor(
                                options.contractFilter.Value,
                                implementation,
                                dependencies,
                                options.lifetime,
                                options.serviceId));
                        }
                        else
                        {
                            var interfaces = classSymbol.Interfaces.Select(i => new TypeDescriptor(i.ToString())).ToList();
                            if (interfaces.Any())
                            {
                                foreach (var contract in interfaces)
                                {
                                    foundServices.Add(new ServiceDescriptor(
                                        contract,
                                        implementation,
                                        dependencies,
                                        options.lifetime,
                                        options.serviceId));
                                }
                            }
                            else
                            {
                                foundServices.Add(new ServiceDescriptor(
                                   implementation,
                                   dependencies,
                                   options.lifetime,
                                   options.serviceId));
                            }
                        }
                    }
                }
            }

            return foundServices;
        }

        /// <summary>
        /// Extract the values of the <see cref="ExportAttribute.Lifetime"/>, <see cref="ExportAttribute.ServiceContract"/>
        /// and <see cref="ExportAttribute.ServiceId"/> properties.
        /// </summary>
        /// <param name="attribute"> The <see cref="ExportAttribute"/> whose values should be retrieved. </param>
        /// <returns> The <see cref="ExportAttribute"/>'s property values. </returns>
        private static (Lifetime lifetime, TypeDescriptor? contractFilter, string? serviceId) GetExportOptions(AttributeData attribute)
        {
            var constructorArguments = attribute.ConstructorArguments;
            var lifetime = Lifetime.Transient;
            var contractFilter = (TypeDescriptor?)null;
            foreach (var argument in constructorArguments)
            {
                if (argument.Kind == TypedConstantKind.Type && argument.Value != null)
                {
                    contractFilter = new TypeDescriptor(argument.Value.ToString());
                }
                else if (argument.Kind == TypedConstantKind.Enum && argument.Value is byte enumValue)
                {
                    lifetime = (Lifetime)enumValue;
                }
            }

            var namedArguments = attribute.NamedArguments;
            var serviceId = (string?)null;
            foreach (var argument in namedArguments)
            {
                if (argument.Key == nameof(ExportAttribute.ServiceId) &&
                    argument.Value.Value is string value)
                {
                    serviceId = value;
                }
            }

            return (lifetime, contractFilter, serviceId);
        }

        /// <summary>
        /// Gets a collection of <see cref="DependencyDescriptor"/>s for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type"> The <see cref="Type"/> whose constructor dependencies should be retrieved. </param>
        /// <returns> The constructor dependencies of the given <paramref name="type"/>. </returns>
        private static List<DependencyDescriptor> GetConstructorDependencies(INamedTypeSymbol type)
        {
            var ctor = type.InstanceConstructors.Single();
            var dependencies = ctor.Parameters
                .Select(p => new DependencyDescriptor(
                    contract: new TypeDescriptor(p.Type.ToString()),
                    serviceId: GetServiceId(p)))
                .ToList();
            return dependencies;
        }

        /// <summary>
        /// Extract the <see cref="ImportAttribute.ServiceId"/> for the given <paramref name="parameter"/>.
        /// </summary>
        /// <param name="parameter"> The constructor parameter whose optional ServiceId should be retrieved. </param>
        /// <returns> The parameter's optioal ServiceId. </returns>
        private static string? GetServiceId(IParameterSymbol parameter)
        {
            var serviceId = (string?)null;
            foreach (var attribute in parameter.GetAttributes())
            {
                if (nameof(ImportAttribute).Equals(attribute.AttributeClass?.Name, StringComparison.Ordinal))
                {
                    foreach (var argument in attribute.ConstructorArguments)
                    {
                        if (argument.Kind == TypedConstantKind.Primitive &&
                            argument.Value is string value)
                        {
                            serviceId = value;
                            break;
                        }
                    }
                }
            }

            return serviceId;
        }

        #endregion
    }
}
