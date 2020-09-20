namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using CodeGeneration;
    using Metadata;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the ServiceFactory type.
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System;
    ///     using System.Collections.Concurrent;
    ///     using System.Collections.Generic;
    ///
    ///     public sealed partial class ServiceFactory
    ///         : IServiceFactory<IFoo1>
    ///         , IServiceFactory<IFoo2>
    ///         ...
    ///         , IServiceFactory<IFooN>
    ///     {
    ///         private ConcurrentDictionary<Type, object> SingletonInstances { get; } = new ConcurrentDictionary<Type, object>();
    /// 
    ///         IFoo1 IServiceFactory<IFoo1>.CreateOrGetService()
    ///         {
    ///             return new Foo1();
    ///         }
    ///         
    ///         IFoo2 IServiceFactory<IFoo2>.CreateOrGetService()
    ///         {
    ///             return new Foo2();
    ///         }
    ///
    ///         ...
    ///
    ///         IFooN IServiceFactory<IFooN>.CreateOrGetService()
    ///         {
    ///             return new FooN(dep0);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </example>
    [Generator]
    public sealed class ServiceFactoryGenerator : ISourceGenerator
    {
        #region Logic

        /// <inheritdoc />
        public void Initialize(InitializationContext context)
        {
            // No initialization required for this generator
        }

        /// <inheritdoc />
        public void Execute(SourceGeneratorContext context)
        {
            try
            {
                var detectedServices = new List<ServiceDescriptor>();
                foreach (var reference in context.Compilation.References.OfType<PortableExecutableReference>())
                {
                    var reader = reference.GetMetadataReader();
                    if (reader == null)
                    {
                        continue;
                    }

                    foreach (var service in reader.GetExportedServices())
                    {
                        var lifetime = Lifetime.Transient;
                        TypeDescriptor? contractFilter = null; 
                        foreach(var value in service.ExportAttribute.FixedArguments)
                        {
                            if (value.Type.FullName == typeof(Lifetime).FullName)
                            {
                                lifetime = (Lifetime)value.Value;
                            }
                            else if (value.Type.FullName == typeof(Type).FullName)
                            {
                                contractFilter = (TypeDescriptor)value.Value;
                            }
                        }

                        var implementation = reader.ToTypeDescriptor(service.TypeDefinition);
                        var dependencies = reader.GetConstructorDependencies(service.TypeDefinition);
                        if (contractFilter.HasValue)
                        {
                            detectedServices.Add(new ServiceDescriptor(contractFilter.Value, implementation, dependencies, lifetime));
                        }
                        else
                        {
                            var implementedInterfaces = reader.GetImplementedInterfaces(service.TypeDefinition);
                            if (implementedInterfaces.Any())
                            {
                                foreach (var @interface in implementedInterfaces)
                                {
                                    detectedServices.Add(new ServiceDescriptor(@interface, implementation, dependencies, lifetime));
                                }
                            }
                            else
                            {
                                detectedServices.Add(new ServiceDescriptor(implementation, dependencies, lifetime));
                            }
                        }
                    }
                }

                var code = CreateServiceFactory(detectedServices);
                context.AddSource("ServiceFactory", SourceText.From(code, Encoding.UTF8));
            }
            catch (Exception e)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "CTI002",
                        title: "Can't generate the ServiceFactory type",
                        messageFormat: $"{nameof(ServiceFactoryGenerator)}: {{0}}",
                        category: "CompileTimeInject.ContainerGenerator",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        description: "There was an unexpected exception generating the ServiceFactory type"),
                    Location.None,
                    e);
                context.ReportDiagnostic(diagnostic);
            }
        }

        /// <summary>
        /// Create the in-memory source code for the ServiceFactory type.
        /// </summary>
        /// <param name="services"> A collection of services the ServiceFactory should be able to create. </param>
        /// <returns> The created in-memory source code. </returns>
        private string CreateServiceFactory(IEnumerable<ServiceDescriptor> services)
        {
            var _ = string.Empty;
            var separator = new InterfaceSeparator();

            var serviceGroups = services
                .GroupBy(service => service.Contract)
                .ToList();

            var contractImplementation = serviceGroups
                .Where(group => group.Count() == 1)
                .Select(group => group.First())
                .ToList();
            var transientContractImplementation = contractImplementation
                .Where(service => service.Lifetime == Lifetime.Transient);
            var singletonContractImplementation = contractImplementation
                .Where(service => service.Lifetime == Lifetime.Singleton);

            var sharedContractImplementation = serviceGroups
                .Where(group => group.Count() > 1)
                .ToList();

            var code = new CodeBuilder(
                "namespace CustomCode.CompileTimeInject.GeneratedCode")
                .BeginScope(
                    "using System;",
                    "using System.Collections.Concurrent;",
                    "using System.Collections.Generic;",
                    _,
                    "/// <summary>",
                    "/// Default implementation for each <see cref=\"IServiceFactory{T}\"/> interface.",
                    "/// </summary>",
                    "public sealed partial class ServiceFactory")
                .ForEach(contractImplementation, service =>
                       $"{separator} IServiceFactory<{service.Contract.FullName}>")
                .ForEach(sharedContractImplementation, sharedContract =>
                       $"{separator} IServiceFactory<IEnumerable<{sharedContract.Key.FullName}>>")
                    .BeginScope(
                        "#region Data",
                        _,
                        "/// <summary>",
                        "/// Gets a cache for created singleton service instances.",
                        "/// </summary>",
                        "private ConcurrentDictionary<Type, object> SingletonInstances { get; } = new ConcurrentDictionary<Type, object>();",
                        _,
                        "#endregion",
                        _,
                        "#region Logic")

                    // contracts with a single implementation with Lifetime.Transient

                    .ForEach(transientContractImplementation, (service, code) => code.ContinueWith(
                        _,
                        "/// <inheritdoc />",
                       $"{service.Contract.FullName} IServiceFactory<{service.Contract.FullName}>.CreateOrGetService()")
                        .BeginScope()
                        .ForEach(service.Dependencies, (dependency, index) => dependency.IsFactory()
                         ? $"var dependency{index} = new Func<{dependency.Contract()}>(((IServiceFactory<{dependency.Contract()}>)this).CreateOrGetService);"
                         : $"var dependency{index} = ((IServiceFactory<{dependency.FullName}>)this).CreateOrGetService();")
                        .ContinueWith(
                           $"var service = new {service.Implementation.FullName}({service.Dependencies.CommaSeparated()});",
                            "return service;")
                        .EndScope())

                    // contracts with a single implementation with Lifetime.Singleton

                    .ForEach(singletonContractImplementation, (service, code) => code.ContinueWith(
                        _,
                        "/// <inheritdoc />",
                       $"{service.Contract.FullName} IServiceFactory<{service.Contract.FullName}>.CreateOrGetService()")
                        .BeginScope(
                           $"var service = ({service.Contract.FullName})SingletonInstances.GetOrAdd(typeof({service.Contract.FullName}), _ =>")
                            .BeginInlineLambdaScope()
                            .ForEach(service.Dependencies, (dependency, index) => dependency.IsFactory()
                             ? $"var dependency{index} = new Func<{dependency.Contract()}>(((IServiceFactory<{dependency.Contract()}>)this).CreateOrGetService);"
                             : $"var dependency{index} = ((IServiceFactory<{dependency.FullName}>)this).CreateOrGetService();")
                            .ContinueWith(
                               $"var service = new {service.Implementation.FullName}({service.Dependencies.CommaSeparated()});",
                                "return service;")
                            .EndInlineLambdaScope(");").ContinueWith(
                            "return service;")
                        .EndScope())

                    // contracts with multiple implementations ...

                    .ForEach(sharedContractImplementation, (sharedContract, code) => code.ContinueWith(
                        _,
                        "/// <inheritdoc />",
                       $"IEnumerable<{sharedContract.Key.FullName}> IServiceFactory<IEnumerable<{sharedContract.Key.FullName}>>.CreateOrGetService()")
                        .BeginScope(
                           $"var services = new List<{sharedContract.Key.FullName}>();")

                        // ... with Lifetime.Transient

                        .ForEach(sharedContract.TransientServices(), (service, code) => code
                        .BeginScope()
                            .ForEach(service.Dependencies, (dependency, index) => dependency.IsFactory()
                             ? $"var dependency{index} = new Func<{dependency.Contract()}>(((IServiceFactory<{dependency.Contract()}>)this).CreateOrGetService);"
                             : $"var dependency{index} = ((IServiceFactory<{dependency.FullName}>)this).CreateOrGetService();")
                            .ContinueWith(
                               $"var service = new {service.Implementation.FullName}({service.Dependencies.CommaSeparated()});",
                               $"services.Add(service);")
                        .EndScope())

                        // ... with Lifetime.Singleton

                        .ForEach(sharedContract.SingletonServices(), (service, code) => code
                        .BeginScope(
                           $"var service = ({service.Contract.FullName})SingletonInstances.GetOrAdd(typeof({service.Contract.FullName}), _ =>")
                            .BeginInlineLambdaScope()
                            .ForEach(service.Dependencies, (dependency, index) => dependency.IsFactory()
                             ? $"var dependency{index} = new Func<{dependency.Contract()}>(((IServiceFactory<{dependency.Contract()}>)this).CreateOrGetService);"
                             : $"var dependency{index} = ((IServiceFactory<{dependency.FullName}>)this).CreateOrGetService();")
                            .ContinueWith(
                               $"var service = new {service.Implementation.FullName}({service.Dependencies.CommaSeparated()});",
                                "return service;")
                            .EndInlineLambdaScope(");").ContinueWith(
                            "services.Add(service);")
                        .EndScope())

                        .ContinueWith(
                            "return services;")
                        .EndScope())

                    .ContinueWith(
                        _,
                        "#endregion")
                    .EndScope()
                .EndScope();

            return code.ToString();
        }

        #endregion
    }
}
