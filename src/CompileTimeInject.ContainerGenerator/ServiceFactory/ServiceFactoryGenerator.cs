namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using Annotations;
    using CodeGeneration;
    using Metadata;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata;
    using System.Text;

    /// <summary>
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the ServiceFactory type.
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate either the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System;
    ///     using System.Collections.Concurrent;
    ///     using System.Collections.Generic;
    ///
    ///     public sealed partial class ServiceFactory
    ///         : IServiceFactory
    ///         , IServiceFactory<IFoo1>
    ///         ...
    ///         , IServiceFactory<IFooN>
    ///     {
    ///         public ServiceFactory(ConcurrentDictionary<Type, object> singletonInstances)
    ///         {
    ///             SingletonInstances = singletonInstances;
    ///         }
    ///
    ///         private ConcurrentDictionary<Type, object> SingletonInstances { get; }
    ///
    ///         IFoo1 IServiceFactory<IFoo1>.CreateOrGetService()
    ///         {
    ///             return new Foo1();
    ///         }
    ///
    ///         ...
    ///
    ///         IFooN IServiceFactory<IFooN>.CreateOrGetService()
    ///         {
    ///             var dep0 = ((IServiceFactory<IFoo1>)this).CreateOrGetService();
    ///             ...
    ///             var depX = ((IServiceFactory<IFooX>)this).CreateOrGetService();
    ///             return new FooN(dep0, ..., depX);
    ///         }
    ///     }
    /// }
    /// ]]>
    ///
    /// or the following code:
    ///
    /// /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System;
    ///     using System.Collections.Concurrent;
    ///     using System.Collections.Generic;
    ///
    ///     public sealed partial class ServiceFactory
    ///         : IServiceFactory
    ///         , IServiceFactory<IFoo1>
    ///         ...
    ///         , IServiceFactory<IFooN>
    ///     {
    ///         public ServiceFactory(
    ///             ConcurrentDictionary<Type, object> scopedInstances,
    ///             ConcurrentDictionary<Type, object> singletonInstances)
    ///         {
    ///             ScopedInstances = scopedInstances;
    ///             SingletonInstances = singletonInstances;
    ///         }
    ///
    ///         private ConcurrentDictionary<Type, object> ScopedInstances { get; }
    ///
    ///         private ConcurrentDictionary<Type, object> SingletonInstances { get; }
    ///
    ///         IFoo1 IServiceFactory<IFoo1>.CreateOrGetService()
    ///         {
    ///             return new Foo1();
    ///         }
    ///
    ///         ...
    ///
    ///         IFooN IServiceFactory<IFooN>.CreateOrGetService()
    ///         {
    ///             var dep0 = ((IServiceFactory<IFoo1>)this).CreateOrGetService();
    ///             ...
    ///             var depX = ((IServiceFactory<IFooX>)this).CreateOrGetService();
    ///             return new FooN(dep0, ..., depX);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// 
    /// </example>
    [Generator]
    public sealed class ServiceFactoryGenerator : ISourceGenerator
    {
        #region Logic

        /// <inheritdoc cref="ISourceGenerator" />
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ServiceCandidateDetector());
        }

        /// <inheritdoc cref="ISourceGenerator" />
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var detectedServices = new List<ServiceDescriptor>();

                // detect exported services in the current compilation
                if (context.SyntaxReceiver is ServiceCandidateDetector detector)
                {
                    foreach (var serviceClass in detector.ServiceCandidates)
                    {
                        var semanticModel = context.Compilation.GetSemanticModel(serviceClass.SyntaxTree);
                        var classSymbol = semanticModel.GetDeclaredSymbol(serviceClass);
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
                                var implementation = new TypeDescriptor(classSymbol.ToString());
                                var ctor = classSymbol.InstanceConstructors.Single();
                                var dependencies = ctor.Parameters.Select(p => new TypeDescriptor(p.Type.ToString())).ToList();
                                if (contractFilter.HasValue)
                                {
                                    detectedServices.Add(new ServiceDescriptor(
                                        contractFilter.Value,
                                        implementation,
                                        dependencies,
                                        lifetime));
                                }
                                else
                                {
                                    var interfaces = classSymbol.Interfaces.Select(i => new TypeDescriptor(i.ToString())).ToList();
                                    if (interfaces.Any())
                                    {
                                        foreach (var contract in interfaces)
                                        {
                                            detectedServices.Add(new ServiceDescriptor(
                                                contract,
                                                implementation,
                                                dependencies,
                                                lifetime));
                                        }
                                    }
                                    else
                                    {
                                        detectedServices.Add(new ServiceDescriptor(
                                           implementation,
                                           dependencies,
                                           lifetime));
                                    }
                                }
                            }
                        }
                    }
                }

                // detect eported services in referenced assemblies
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
                        foreach (var value in service.ExportAttribute.FixedArguments)
                        {
                            if (value.Type.FullName == typeof(Lifetime).FullName)
                            {
                                lifetime = (Lifetime)(value.Value ?? Lifetime.Transient);
                            }
                            else if (value.Type.FullName == typeof(Type).FullName)
                            {
                                contractFilter = (TypeDescriptor?)value.Value;
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
                        id: "CTI003",
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

            var serviceGroups = services
                .GroupBy(service => service.Contract)
                .ToList();

            var serviceContract = serviceGroups
                .Where(group => group.Count() == 1)
                .Select(group => group.First())
                .ToList();
            var transientServiceContract = serviceContract
                .Where(service => service.Lifetime == Lifetime.Transient);
            var singletonServiceContract = serviceContract
                .Where(service => service.Lifetime == Lifetime.Singleton);
            var scopedServiceContract = serviceContract
                .Where(service => service.Lifetime == Lifetime.Scoped);

            var sharedServiceContract = serviceGroups
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
                    "/// Default implementation for the <see cref=\"IServiceFactory\"/> marker interface",
                    "/// and each <see cref=\"IServiceFactory{T}\"/> interface.",
                    "/// </summary>",
                    "public sealed partial class ServiceFactory")
                .Indent(": IServiceFactory")
                .Indent(code => code.ForEach(serviceContract, service =>
                       $", IServiceFactory<{service.Contract.FullName}>"))
                .Indent(code => code.ForEach(sharedServiceContract, sharedContract =>
                       $", IServiceFactory<IEnumerable<{sharedContract.Key.FullName}>>"))
                    .BeginScope(
                        "#region Dependencies",
                        _,
                        "/// <summary>",
                        "/// Creates a new instance of the <see cref=\"ServiceFactory\"/> type.",
                        "/// </summary>")
                    .If(scopedServiceContract.None(), code => code.ContinueWith(
                        "/// <param name=\"singletonInstances\"> A cache for created singleton service instances. </param>",
                        "public ServiceFactory(ConcurrentDictionary<Type, object> singletonInstances)")
                        .BeginScope(
                            "SingletonInstances = singletonInstances;")
                        .EndScope())
                    .If(scopedServiceContract.Any(), code => code.ContinueWith(
                        "/// <param name=\"scopedInstances\"> A cache for created scroped service instances. </param>",
                        "/// <param name=\"singletonInstances\"> A cache for created singleton service instances. </param>",
                        "public ServiceFactory(")
                        .Indent(
                            "ConcurrentDictionary<Type, object> scopedInstances,",
                            "ConcurrentDictionary<Type, object> singletonInstances)")
                        .BeginScope(
                            "ScopedInstances = scopedInstances;",
                            "SingletonInstances = singletonInstances;")
                        .EndScope())
                    .ContinueWith(
                        _,
                        "#endregion",
                        _,
                        "#region Data",
                        _,
                        "/// <summary>",
                        "/// Gets a cache for created singleton service instances.",
                        "/// </summary>",
                        "private ConcurrentDictionary<Type, object> SingletonInstances { get; }",
                        _)
                    .If(scopedServiceContract.Any(), code => code.ContinueWith(
                        "/// <summary>",
                        "/// Gets a cache for created singleton service instances.",
                        "/// </summary>",
                        "private ConcurrentDictionary<Type, object> ScopedInstances { get; }",
                        _))
                    .ContinueWith(
                        "#endregion",
                        _,
                        "#region Logic")

                    // contracts with a single implementation with Lifetime.Transient

                    .ForEach(transientServiceContract, (service, code) => code.ContinueWith(
                        _,
                       $"/// <inheritdoc cref=\"IServiceFactory{{{service.Contract.FullName}}}\" />",
                       $"{service.Contract.FullName} IServiceFactory<{service.Contract.FullName}>.CreateOrGetService()")
                        .BeginScope()
                        .ForEach(service.Dependencies, (dependency, index) => dependency.IsFactory()
                         ? $"var dependency{index} = new Func<{dependency.Contract()}>(((IServiceFactory<{dependency.Contract()}>)this).CreateOrGetService);"
                         : $"var dependency{index} = ((IServiceFactory<{dependency.FullName}>)this).CreateOrGetService();")
                        .ContinueWith(
                           $"var service = new {service.Implementation.FullName}({service.Dependencies.CommaSeparated()});",
                            "return service;")
                        .EndScope())

                    // contracts with a single implementation with Lifetime.Scoped

                    .ForEach(scopedServiceContract, (service, code) => code.ContinueWith(
                        _,
                       $"/// <inheritdoc cref=\"IServiceFactory{{{service.Contract.FullName}}}\" />",
                       $"{service.Contract.FullName} IServiceFactory<{service.Contract.FullName}>.CreateOrGetService()")
                        .BeginScope(
                           $"var service = ({service.Contract.FullName})ScopedInstances.GetOrAdd(typeof({service.Contract.FullName}), _ =>")
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

                    // contracts with a single implementation with Lifetime.Singleton

                    .ForEach(singletonServiceContract, (service, code) => code.ContinueWith(
                        _,
                       $"/// <inheritdoc cref=\"IServiceFactory{{{service.Contract.FullName}}}\" />",
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

                    .ForEach(sharedServiceContract, (sharedContract, code) => code.ContinueWith(
                        _,
                       $"/// <inheritdoc cref=\"IServiceFactory{{{sharedContract.Key.FullName}}}\" />",
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

                        // ... with Lifetime.Scoped

                        .ForEach(sharedContract.SingletonServices(), (service, code) => code
                        .BeginScope(
                           $"var service = ({service.Contract.FullName})ScopedInstances.GetOrAdd(typeof({service.Contract.FullName}), _ =>")
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
