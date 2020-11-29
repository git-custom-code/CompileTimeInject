namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using Annotations;
    using CodeGeneration;
    using Metadata;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the "ServiceFactory" type.
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate either the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System;
    ///     using System.Collections.Generic;
    ///
    ///     public sealed partial class ServiceFactory
    ///         : IServiceFactory
    ///         , IServiceFactory<IFoo1>
    ///         ...
    ///         , IServiceFactory<IFooN>
    ///     {
    ///         public ServiceFactory(ServiceCache singletonInstances)
    ///         {
    ///             SingletonInstances = singletonInstances;
    ///         }
    ///
    ///         private ServiceCache SingletonInstances { get; }
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
    ///     using System.Collections.Generic;
    ///
    ///     public sealed partial class ServiceFactory
    ///         : IServiceFactory
    ///         , IServiceFactory<IFoo1>
    ///         ...
    ///         , IServiceFactory<IFooN>
    ///     {
    ///         public ServiceFactory(
    ///             ServiceCache scopedInstances,
    ///             ServiceCache singletonInstances)
    ///         {
    ///             ScopedInstances = scopedInstances;
    ///             SingletonInstances = singletonInstances;
    ///         }
    ///
    ///         private ServiceCache ScopedInstances { get; }
    ///
    ///         private ServiceCache SingletonInstances { get; }
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
                if (context.SyntaxReceiver is ServiceCandidateDetector currentCompilation)
                {
                    var services = context.Compilation.FindExportedServices(currentCompilation.ServiceCandidates);
                    if (services.Any())
                    {
                        detectedServices.AddRange(services);
                    }
                }

                // detect eported services in referenced assemblies
                foreach (var compilation in context.Compilation.GetReferencedIocVisibleAssemblies())
                {
                    var services = compilation.FindExportedServices();
                    if (services.Any())
                    {
                        detectedServices.AddRange(services);
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
                .Where(service => string.IsNullOrEmpty(service.ServiceId))
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
            var namedServiceContract = services
                .Where(s => !string.IsNullOrEmpty(s.ServiceId))
                .GroupBy(service => service.Contract)
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
                .Indent(code => code.ForEach(namedServiceContract, namedContract =>
                       $", INamedServiceFactory<{namedContract.Key.FullName}>"))
                    .BeginScope(
                        "#region Dependencies",
                        _,
                        "/// <summary>",
                        "/// Creates a new instance of the <see cref=\"ServiceFactory\"/> type.",
                        "/// </summary>")
                    .If(scopedServiceContract.None(), code => code.ContinueWith(
                        "/// <param name=\"singletonInstances\"> A cache for created singleton service instances. </param>",
                        "public ServiceFactory(ServiceCache singletonInstances)")
                        .BeginScope(
                            "SingletonInstances = singletonInstances;")
                        .EndScope())
                    .If(scopedServiceContract.Any(), code => code.ContinueWith(
                        "/// <param name=\"scopedInstances\"> A cache for created scroped service instances. </param>",
                        "/// <param name=\"singletonInstances\"> A cache for created singleton service instances. </param>",
                        "public ServiceFactory(")
                        .Indent(
                            "ServiceCache scopedInstances,",
                            "ServiceCache singletonInstances)")
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
                        "private ServiceCache SingletonInstances { get; }",
                        _)
                    .If(scopedServiceContract.Any(), code => code.ContinueWith(
                        "/// <summary>",
                        "/// Gets a cache for created singleton service instances.",
                        "/// </summary>",
                        "private ServiceCache ScopedInstances { get; }",
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
                        .ForEach(service.Dependencies, (dependency, index) =>
                           $"var dependency{index} = {dependency.CreateOrGetService()};")
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
                            .ForEach(service.Dependencies, (dependency, index) =>
                               $"var dependency{index} = {dependency.CreateOrGetService()};")
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
                            .ForEach(service.Dependencies, (dependency, index) =>
                               $"var dependency{index} = {dependency.CreateOrGetService()};")
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
                            .ForEach(service.Dependencies, (dependency, index) =>
                               $"var dependency{index} = {dependency.CreateOrGetService()};")
                            .ContinueWith(
                               $"var service = new {service.Implementation.FullName}({service.Dependencies.CommaSeparated()});",
                               $"services.Add(service);")
                        .EndScope())

                        // ... with Lifetime.Scoped

                        .ForEach(sharedContract.ScopedServices(), (service, code) => code
                        .BeginScope(
                           $"var service = ({service.Contract.FullName})ScopedInstances.GetOrAdd({service.CacheParameter()} =>")
                            .BeginInlineLambdaScope()
                            .ForEach(service.Dependencies, (dependency, index) =>
                               $"var dependency{index} = {dependency.CreateOrGetService()};")
                            .ContinueWith(
                               $"var service = new {service.Implementation.FullName}({service.Dependencies.CommaSeparated()});",
                                "return service;")
                            .EndInlineLambdaScope(");").ContinueWith(
                            "services.Add(service);")
                        .EndScope())

                        // ... with Lifetime.Singleton

                        .ForEach(sharedContract.SingletonServices(), (service, code) => code
                        .BeginScope(
                           $"var service = ({service.Contract.FullName})SingletonInstances.GetOrAdd({service.CacheParameter()} =>")
                            .BeginInlineLambdaScope()
                            .ForEach(service.Dependencies, (dependency, index) =>
                               $"var dependency{index} = {dependency.CreateOrGetService()};")
                            .ContinueWith(
                               $"var service = new {service.Implementation.FullName}({service.Dependencies.CommaSeparated()});",
                                "return service;")
                            .EndInlineLambdaScope(");").ContinueWith(
                            "services.Add(service);")
                        .EndScope())

                        .ContinueWith(
                            "return services;")
                        .EndScope())

                    // contracts with named service implementations ...

                    .ForEach(namedServiceContract, (namedContract, code) => code.ContinueWith(
                        _,
                       $"/// <inheritdoc cref=\"INamedServiceFactory{{{namedContract.Key.FullName}}}\" />",
                       $"{namedContract.Key.FullName}? INamedServiceFactory<{namedContract.Key.FullName}>.CreateOrGetNamedService(string serviceId)")
                        .BeginScope()

                        // ... with Lifetime.Transient

                        .ForEach(namedContract.TransientServices(), (service, code) => code.ContinueWith(
                           $"if (string.Equals(serviceId, \"{service.ServiceId}\", StringComparison.Ordinal))")
                            .BeginScope()
                            .ForEach(service.Dependencies, (dependency, index) =>
                               $"var dependency{index} = {dependency.CreateOrGetService()};")
                            .ContinueWith(
                               $"var service = new {service.Implementation.FullName}({service.Dependencies.CommaSeparated()});",
                               $"return service;")
                            .EndScope(_))

                        // ... with Lifetime.Scoped

                        .ForEach(namedContract.ScopedServices(), (service, code) => code.ContinueWith(
                           $"if (string.Equals(serviceId, \"{service.ServiceId}\", StringComparison.Ordinal))")
                            .BeginScope(
                               $"var service = ({service.Contract.FullName})ScopedInstances.GetOrAdd(typeof({service.Contract.FullName}), serviceId, _ =>")
                                .BeginInlineLambdaScope()
                                .ForEach(service.Dependencies, (dependency, index) =>
                                   $"var dependency{index} = {dependency.CreateOrGetService()};")
                                .ContinueWith(
                                   $"var service = new {service.Implementation.FullName}({service.Dependencies.CommaSeparated()});",
                                    "return service;")
                                .EndInlineLambdaScope(");").ContinueWith(
                                "return service;")
                            .EndScope(_))

                        // ... with Lifetime.Singleton

                        .ForEach(namedContract.SingletonServices(), (service, code) => code.ContinueWith(
                           $"if (string.Equals(serviceId, \"{service.ServiceId}\", StringComparison.Ordinal))")
                            .BeginScope(
                               $"var service = ({service.Contract.FullName})SingletonInstances.GetOrAdd(typeof({service.Contract.FullName}), serviceId, _ =>")
                                .BeginInlineLambdaScope()
                                .ForEach(service.Dependencies, (dependency, index) =>
                                   $"var dependency{index} = {dependency.CreateOrGetService()};")
                                .ContinueWith(
                                   $"var service = new {service.Implementation.FullName}({service.Dependencies.CommaSeparated()});",
                                    "return service;")
                                .EndInlineLambdaScope(");").ContinueWith(
                                "return service;")
                            .EndScope(_))

                        .ContinueWith(
                            "return default;")
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
