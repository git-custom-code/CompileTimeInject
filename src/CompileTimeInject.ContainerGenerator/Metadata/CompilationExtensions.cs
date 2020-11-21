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
                        var constructorArguments = attribute.ConstructorArguments;
                        var lifetime = Lifetime.Transient;
                        TypeDescriptor? contractFilter = null;
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
                            if (argument.Key == "ServiceId" && argument.Value.Value is string value)
                            {
                                serviceId = value;
                            }
                        }

                        var implementation = new TypeDescriptor(classSymbol.ToString());
                        var ctor = classSymbol.InstanceConstructors.Single();
                        var dependencies = ctor.Parameters.Select(p => new TypeDescriptor(p.Type.ToString())).ToList();
                        if (contractFilter.HasValue)
                        {
                            foundServices.Add(new ServiceDescriptor(
                                contractFilter.Value,
                                implementation,
                                dependencies,
                                lifetime,
                                serviceId));
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
                                        lifetime,
                                        serviceId));
                                }
                            }
                            else
                            {
                                foundServices.Add(new ServiceDescriptor(
                                   implementation,
                                   dependencies,
                                   lifetime,
                                   serviceId));
                            }
                        }
                    }
                }
            }

            return foundServices;
        }

        #endregion
    }
}
