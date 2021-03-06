namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using CodeGeneration;
    using Metadata;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the "IocContainer" type.
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate either the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System;
    ///     using System.Collections.Generic;
    ///     using System.Linq;
    ///
    ///     public sealed class IocContainer
    ///     {
    ///         public IocContainer()
    ///         {
    ///             SingletonInstances = new ServiceCache();
    ///             Factory = new ServiceFactory(SingletonInstances);
    ///         }
    ///
    ///         private IServiceFactory Factory { get; }
    ///
    ///         private ServiceCache SingletonInstances { get; }
    ///
    ///         public T? GetService<T>() where T : class
    ///         {
    ///             var factory = Factory as IServiceFactory<T>;
    ///             return factory?.CreateOrGetService();
    ///         }
    ///
    ///         public IEnumerable<T> GetServices<T>() where T : class
    ///         {
    ///             if (Factory is IServiceFactory<IEnumerable<T>> collectionFactory)
    ///             {
    ///                 return collectionFactory.CreateOrGetService();
    ///             }
    ///
    ///             if (Factory is IServiceFactory<T> factory)
    ///             {
    ///                 return new List<T> { factory.CreateOrGetService() };
    ///             }
    ///
    ///             return Enumerable.Empty<T>();
    ///         }
    ///     }
    /// }
    /// ]]>
    ///
    /// or the following code:
    ///
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System;
    ///     using System.Collections.Generic;
    ///     using System.Threading;
    ///
    ///     public sealed class IocContainer
    ///     {
    ///         public IocContainer()
    ///         {
    ///             SingletonInstances = new ServiceCache();
    ///             DefaultScope = new Scope("DefaultScope", () => { }, SingletonInstances);
    ///             LifetimeScopes = new AsyncLocal<List<WeakReference<Scope>>>();
    ///             SyncLock = new object();
    ///         }
    ///
    ///         private Scope DefaultScope { get; }
    ///
    ///         private ServiceCache SingletonInstances { get; }
    ///
    ///         private AsyncLocal<List<WeakReference<Scope>>> LifetimeScopes { get; }
    ///
    ///         private object SyncLock { get; }
    ///
    ///         public IDisposable BeginScope()
    ///         {
    ///             lock (SyncLock)
    ///             {
    ///                 if (LifetimeScopes.Value == null)
    ///                 {
    ///                     LifetimeScopes.Value = new List<WeakReference<Scope>>();
    ///                 }
    ///
    ///                 var lifetimeScopes = LifetimeScopes.Value;
    ///                 var id = Guid.NewGuid().ToString();
    ///                 var index = lifetimeScopes.Count;
    ///                 var scope = new Scope(id, () => lifetimeScopes.RemoveAt(index), SingletonInstances);
    ///                 lifetimeScopes.Add(new WeakReference<Scope>(scope));
    ///                 return scope;
    ///             }
    ///         }
    ///
    ///         public T? GetService<T>() where T : class
    ///         {
    ///             var scope = GetActiveScope();
    ///             var service = scope.GetService<T>();
    ///             return service;
    ///         }
    ///
    ///         public IEnumerable<T> GetServices<T>() where T : class
    ///         {
    ///             var scope = GetActiveScope();
    ///             var services = scope.GetServices<T>();
    ///             return services;
    ///         }
    ///
    ///         private Scope GetActiveScope()
    ///         {
    ///             var lifetimeScopes = LifetimeScopes.Value;
    ///             if (lifetimeScopes != null)
    ///             {
    ///                 lock (SyncLock)
    ///                 {
    ///                     for (var i = lifetimeScopes.Count - 1; i >= 0; --i)
    ///                     {
    ///                         if (lifetimeScopes[i].TryGetTarget(out var scope))
    ///                         {
    ///                             return scope;
    ///                         }
    ///
    ///                         lifetimeScopes.RemoveAt(i);
    ///                     }
    ///                 }
    ///             }
    ///
    ///             return DefaultScope;
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </example>
    [Generator]
    public sealed class IocContainerGenerator : ISourceGenerator
    {
        #region Logic

        /// <inheritdoc cref="ISourceGenerator" />
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new IocContainerSyntaxReceiver());
        }

        /// <inheritdoc cref="ISourceGenerator" />
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var useScopedServices = false;
                var useNamedServices = false;
                if (context.SyntaxReceiver is IocContainerSyntaxReceiver currrentCompilation)
                {
                    if (context.Compilation.IsIocVisibleAssembly())
                    {
                        useScopedServices = currrentCompilation.UseLifetimeScoped;
                        useNamedServices = currrentCompilation.UseNamedServices;
                    }

                    if (!useScopedServices || !useNamedServices)
                    {
                        foreach(var compilation in context.Compilation.GetReferencedIocVisibleAssemblies())
                        {
                            if (compilation.DefinesServiceWithLifetimeScoped())
                            {
                                useScopedServices = true;
                            }
                            if (compilation.DefinesAnyNamedService())
                            {
                                useNamedServices = true;
                            }

                            if (useScopedServices && useNamedServices)
                            {
                                break;
                            }
                        }
                    }
                }

                var code = useScopedServices
                    ? CreateScopedIocContainerType(useNamedServices)
                    : CreateIocContainerType(useNamedServices);
                context.AddSource("IocContainer", SourceText.From(code, Encoding.UTF8));
            }
            catch (Exception e)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "CTI005",
                        title: "Can't generate the IocContainer type",
                        messageFormat: $"{nameof(IocContainerGenerator)}: {{0}}",
                        category: "CompileTimeInject.ContainerGenerator",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        description: "There was an unexpected exception generating the IocContainer type"),
                    Location.None,
                    e);
                context.ReportDiagnostic(diagnostic);
            }
        }

        /// <summary>
        /// Create the in-memory source code for the IocContainer type.
        /// </summary>
        /// <param name="useNamedServices">
        /// True if the container contains named services, false otherwise.
        /// </param>
        /// <returns> The created in-memory source code. </returns>
        private string CreateIocContainerType(bool useNamedServices)
        {
            var _ = string.Empty;

            var code = new CodeBuilder(
                "namespace CustomCode.CompileTimeInject.GeneratedCode")
                .BeginScope(
                    "using System;",
                    "using System.Collections.Generic;",
                    "using System.Linq;",
                    _,
                    "/// <summary>",
                    "/// A compile time generated inversion of control container.",
                    "/// </summary>",
                    "public sealed class IocContainer")
                    .BeginScope(
                        "#region Dependencies",
                        _,
                        "/// <summary>",
                        "/// Creates a new instance of the <see cref=\"IocContainer\"/> type.",
                        "/// </summary>",
                        "public IocContainer()")
                        .BeginScope(
                            "SingletonInstances = new ServiceCache();",
                            "Factory = new ServiceFactory(SingletonInstances);")
                        .EndScope(
                        _,
                        "/// <summary>",
                        "/// Gets the generated factory that is used to create service instances.",
                        "/// </summary>",
                        "private IServiceFactory Factory { get; }",
                        _,
                        "#endregion",
                        _,
                        "#region Data",
                        _,
                        "/// <summary>",
                        "/// Gets a cache for created singleton service instances.",
                        "/// </summary>",
                        "private ServiceCache SingletonInstances { get; }",
                        _,
                        "#endregion",
                        _,
                        "#region Logic",
                        _,
                        "/// <summary>",
                        "/// Gets a service implementation by contract.",
                        "/// </summary>",
                        "/// <typeparam name=\"T\"> The service contract whose implementation should be retrieved. </typeparam>",
                        "/// <returns> The contract's service implementation or null if no such implementation exists. </returns>",
                        "public T? GetService<T>() where T : class")
                        .BeginScope(
                            "var factory = Factory as IServiceFactory<T>;",
                            "return factory?.CreateOrGetService();")
                        .EndScope(
                        _,
                        "/// <summary>",
                        "/// Gets a required service implementation by contract.",
                        "/// </summary>",
                        "/// <typeparam name=\"T\"> The service contract whose implementation should be retrieved. </typeparam>",
                        "/// <returns> The contract's service implementation. </returns>",
                        "/// <exception cref=\"InvalidServiceException\"> Thrown if no service instance could be created. </exception>",
                        "public T GetRequiredService<T>() where T : class")
                        .BeginScope(
                            "if (Factory is IServiceFactory<T> factory)")
                            .BeginScope(
                                "return factory.CreateOrGetService();")
                            .EndScope(
                            _,
                            "throw new InvalidServiceException(typeof(T));")
                        .EndScope()
                        .If(useNamedServices, code => code.ContinueWith(
                        _,
                        "/// <summary>",
                        "/// Gets a named service implementation by contract and id.",
                        "/// </summary>",
                        "/// <typeparam name=\"T\"> The service contract whose implementation should be retrieved. </typeparam>",
                        "/// <param name=\"serviceId\"> The service's unique identifier. </param>",
                        "/// <returns> The contract's named service implementation or null if no such implementation exists. </returns>",
                        "public T? GetService<T>(string serviceId) where T : class")
                        .BeginScope(
                            "var factory = Factory as INamedServiceFactory<T>;",
                            "return factory?.CreateOrGetNamedService(serviceId);")
                        .EndScope(
                        _,
                        "/// <summary>",
                        "/// Gets a required named service implementation by contract and id.",
                        "/// </summary>",
                        "/// <typeparam name=\"T\"> The service contract whose implementation should be retrieved. </typeparam>",
                        "/// <param name=\"serviceId\"> The service's unique identifier. </param>",
                        "/// <returns> The contract's named service implementation or null if no such implementation exists. </returns>",
                        "/// <exception cref=\"InvalidServiceException\"> Thrown if no service instance could be created. </exception>",
                        "public T GetRequiredService<T>(string serviceId) where T : class")
                        .BeginScope(
                            "var factory = Factory as INamedServiceFactory<T>;",
                            "var service = factory?.CreateOrGetNamedService(serviceId);",
                            "if (service == null)")
                            .BeginScope(
                                "throw new InvalidServiceException(typeof(T), serviceId);")
                            .EndScope(
                            "return service;")
                        .EndScope()).ContinueWith(
                        _,
                        "/// <summary>",
                        "/// Gets a collection of service implementations of the same contract.",
                        "/// </summary>",
                        "/// <typeparam name=\"T\"> The service contract whose implementations should be retrieved. </typeparam>",
                        "/// <returns> The service contract's implementations or <see cref=\"Enumerable.Empty{T}\"/> if no such implementations exists. </returns>",
                        "public IEnumerable<T> GetServices<T>() where T : class")
                        .BeginScope(
                            "if (Factory is IServiceFactory<IEnumerable<T>> collectionFactory)")
                            .BeginScope(
                                "return collectionFactory.CreateOrGetService();")
                            .EndScope(
                            _,
                            "if (Factory is IServiceFactory<T> factory)")
                            .BeginScope(
                                "return new List<T> { factory.CreateOrGetService() };")
                            .EndScope(
                            _,
                            "return Enumerable.Empty<T>();")
                        .EndScope(
                        _,
                        "#endregion")
                    .EndScope()
                .EndScope();

            return code.ToString();
        }

        /// <summary>
        /// Create the in-memory source code for the IocContainer type that uses lifetime scopes.
        /// </summary>
        /// <param name="useNamedServices">
        /// True if the container contains named services, false otherwise.
        /// </param>
        /// <returns> The created in-memory source code. </returns>
        private string CreateScopedIocContainerType(bool useNamedServices)
        {
            var _ = string.Empty;

            var code = new CodeBuilder(
                "namespace CustomCode.CompileTimeInject.GeneratedCode")
                .BeginScope(
                    "using System;",
                    "using System.Collections.Generic;",
                    "using System.Threading;",
                    _,
                    "/// <summary>",
                    "/// A compile time generated inversion of control container.",
                    "/// </summary>",
                    "public sealed class IocContainer")
                    .BeginScope(
                        "#region Dependencies",
                        _,
                        "/// <summary>",
                        "/// Creates a new instance of the <see cref=\"IocContainer\"/> type.",
                        "/// </summary>",
                        "public IocContainer()")
                        .BeginScope(
                            "SingletonInstances = new ServiceCache();",
                            "DefaultScope = new Scope(\"DefaultScope\", () => { }, SingletonInstances);",
                            "LifetimeScopes = new AsyncLocal<List<WeakReference<Scope>>>();",
                            "SyncLock = new object();")
                        .EndScope(
                        _,
                        "/// <summary>",
                        "/// Gets the container's default <see cref=\"Scope\"/>.",
                        "/// </summary>",
                        "private Scope DefaultScope { get; }",
                        _,
                        "#endregion",
                        _,
                        "#region Data",
                        _,
                        "/// <summary>",
                        "/// Gets a cache for created singleton service instances.",
                        "/// </summary>",
                        "private ServiceCache SingletonInstances { get; }",
                        _,
                        "/// <summary>",
                        "/// Gets a collection of weak references to created lifetime <see cref=\"Scope\"/> instances.",
                        "/// </summary>",
                        "private AsyncLocal<List<WeakReference<Scope>>> LifetimeScopes { get; }",
                        _,
                        "/// <summary>",
                        "/// Gets a lightweight synchronisation object for thread safety.",
                        "/// </summary>",
                        "private object SyncLock { get; }",
                        _,
                        "#endregion",
                        _,
                        "#region Logic",
                        _,
                        "/// <summary>",
                        "/// Creates a new lifetime <see cref=\"Scope\"/> instance.",
                        "/// </summary>",
                        "/// <returns> The created lifetime <see cref=\"Scope\"/> instance. </returns>",
                        "public IDisposable BeginScope()")
                        .BeginScope(
                            "lock (SyncLock)")
                            .BeginScope(
                                "if (LifetimeScopes.Value == null)")
                                .BeginScope(
                                    "LifetimeScopes.Value = new List<WeakReference<Scope>>();")
                                .EndScope(
                                _,
                                "var lifetimeScopes = LifetimeScopes.Value;",
                                "var id = Guid.NewGuid().ToString();",
                                "var index = lifetimeScopes.Count;",
                                "var scope = new Scope(id, () => lifetimeScopes.RemoveAt(index), SingletonInstances);",
                                "lifetimeScopes.Add(new WeakReference<Scope>(scope));",
                                "return scope;")
                            .EndScope()
                        .EndScope(
                        _,
                        "/// <summary>",
                        "/// Gets a service implementation by contract.",
                        "/// </summary>",
                        "/// <typeparam name=\"T\"> The service contract whose implementation should be retrieved. </typeparam>",
                        "/// <returns> The contract's service implementation or null if no such implementation exists. </returns>",
                        "public T? GetService<T>() where T : class")
                        .BeginScope(
                            "var scope = GetActiveScope();",
                            "var service = scope.GetService<T>();",
                            "return service;")
                        .EndScope(
                        _,
                        "/// <summary>",
                        "/// Gets a required service implementation by contract.",
                        "/// </summary>",
                        "/// <typeparam name=\"T\"> The service contract whose implementation should be retrieved. </typeparam>",
                        "/// <returns> The contract's service implementation. </returns>",
                        "/// <exception cref=\"InvalidServiceException\"> Thrown if no service instance could be created. </exception>",
                        "public T GetRequiredService<T>() where T : class")
                        .BeginScope(
                            "var scope = GetActiveScope();",
                            "var service = scope.GetService<T>();",
                            "if (service == null)")
                            .BeginScope(
                                "throw new InvalidServiceException(typeof(T));")
                            .EndScope(
                            "return service;")
                        .EndScope()
                        .If(useNamedServices, code => code.ContinueWith(
                        _,
                        "/// <summary>",
                        "/// Gets a named service implementation by contract and id.",
                        "/// </summary>",
                        "/// <typeparam name=\"T\"> The service contract whose implementation should be retrieved. </typeparam>",
                        "/// <param name=\"serviceId\"> The service's unique identifier. </param>",
                        "/// <returns> The contract's named service implementation or null if no such implementation exists. </returns>",
                        "public T? GetService<T>(string serviceId) where T : class")
                        .BeginScope(
                            "var scope = GetActiveScope();",
                            "var service = scope.GetService<T>(serviceId);",
                            "return service;")
                        .EndScope(
                        _,
                        "/// <summary>",
                        "/// Gets a required named service implementation by contract and id.",
                        "/// </summary>",
                        "/// <typeparam name=\"T\"> The service contract whose implementation should be retrieved. </typeparam>",
                        "/// <param name=\"serviceId\"> The service's unique identifier. </param>",
                        "/// <returns> The contract's named service implementation or null if no such implementation exists. </returns>",
                        "/// <exception cref=\"InvalidServiceException\"> Thrown if no service instance could be created. </exception>",
                        "public T GetRequiredService<T>(string serviceId) where T : class")
                        .BeginScope(
                            "var scope = GetActiveScope();",
                            "var service = scope.GetService<T>(serviceId);",
                            "if (service == null)")
                            .BeginScope(
                                "throw new InvalidServiceException(typeof(T), serviceId);")
                            .EndScope(
                            "return service;")
                        .EndScope()).ContinueWith(
                        _,
                        "/// <summary>",
                        "/// Gets a collection of service implementations of the same contract.",
                        "/// </summary>",
                        "/// <typeparam name=\"T\"> The service contract whose implementations should be retrieved. </typeparam>",
                        "/// <returns> The service contract's implementations or <see cref=\"Enumerable.Empty{T}\"/> if no such implementations exists. </returns>",
                        "public IEnumerable<T> GetServices<T>() where T : class")
                        .BeginScope(
                            "var scope = GetActiveScope();",
                            "var services = scope.GetServices<T>();",
                            "return services;")
                        .EndScope(
                        _,
                        "/// <summary>",
                        "/// Gets the active <see cref=\"Scope\"/> instance.",
                        "/// </summary>",
                        "/// <returns> The active <see cref=\"Scope\"/> instance. </returns>",
                        "private Scope GetActiveScope()")
                        .BeginScope(
                            "var lifetimeScopes = LifetimeScopes.Value;",
                            "if (lifetimeScopes != null)")
                            .BeginScope(
                                "lock (SyncLock)")
                                .BeginScope(
                                    "for (var i = lifetimeScopes.Count - 1; i >= 0; --i)")
                                    .BeginScope(
                                        "if (lifetimeScopes[i].TryGetTarget(out var scope))")
                                        .BeginScope(
                                            "return scope;")
                                        .EndScope(
                                        _,
                                        "lifetimeScopes.RemoveAt(i);")
                                    .EndScope()
                                .EndScope()
                            .EndScope(
                            _,
                            "return DefaultScope;")
                        .EndScope(
                        _,
                        "#endregion")
                    .EndScope()
                .EndScope();

            return code.ToString();
        }

        #endregion
    }
}
