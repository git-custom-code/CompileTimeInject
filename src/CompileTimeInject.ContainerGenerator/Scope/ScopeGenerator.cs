namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using CodeGeneration;
    using Metadata;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Text;

    /// <summary>
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the "Scope" type.
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System;
    ///     using System.Collections.Generic;
    ///     using System.Linq;
    ///
    ///     public sealed class Scope : IDisposable
    ///     {
    ///         internal Scope(string id, Action disposeAction, ServiceCache singletonInstances)
    ///         {
    ///             Id = id;
    ///             DisposeAction = disposeAction;
    ///             ScopedInstances = new ServiceCache();
    ///             Factory = new ServiceFactory(singletonInstances, ScopedInstances);
    ///         }
    ///
    ///         private IServiceFactory Factory { get; }
    ///
    ///         public string Id { get; }
    ///
    ///         private Action DisposeAction { get; }
    ///
    ///         private ServiceCache ScopedInstances { get; }
    ///
    ///         public void Dispose()
    ///         {
    ///             DisposeAction();
    ///         }
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
    /// </example>
    [Generator]
    public sealed class ScopeGenerator : ISourceGenerator
    {
        #region Logic

        /// <inheritdoc cref="ISourceGenerator" />
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ScopeSyntaxReceiver());
        }

        /// <inheritdoc cref="ISourceGenerator" />
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var useScopedServices = false;
                var useNamedServices = false;
                if (context.SyntaxReceiver is ScopeSyntaxReceiver currrentCompilation)
                {
                    useScopedServices = currrentCompilation.UseLifetimeScoped;
                    useNamedServices = currrentCompilation.UseNamedServices;
                    if (!useScopedServices || !useNamedServices)
                    {
                        foreach (var compilation in context.Compilation.GetReferencedIocVisibleAssemblies())
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

                if (useScopedServices)
                {
                    var code = CreateScopeType(useNamedServices);
                    context.AddSource("Scope", SourceText.From(code, Encoding.UTF8));
                }
            }
            catch (Exception e)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "CTI004",
                        title: "Can't generate the Scope type",
                        messageFormat: $"{nameof(ScopeGenerator)}: {{0}}",
                        category: "CompileTimeInject.ScopeGenerator",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        description: "There was an unexpected exception generating the Scope type"),
                    Location.None,
                    e);
                context.ReportDiagnostic(diagnostic);
            }
        }

        /// <summary>
        /// Create the in-memory source code for the "Scope" type.
        /// </summary>
        /// <param name="useNamedServices">
        /// True if the scope contains named services, false otherwise.
        /// </param>
        /// <returns> The created in-memory source code. </returns>
        private string CreateScopeType(bool useNamedServices)
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
                    "/// A compile time generated service scope.",
                    "/// </summary>",
                    "public sealed class Scope : IDisposable")
                    .BeginScope(
                        "#region Dependencies",
                        _,
                        "/// <summary>",
                        "/// Creates a new instance of the <see cref=\"Scope\"/> type.",
                        "/// </summary>",
                        "/// <param name=\"id\"> The scope's unique identifier. </param>",
                        "/// <param name=\"disposeAction\">",
                        "/// A delegate that is executed when this instance is disposed.",
                        "/// </param>",
                        "/// <param name=\"singletonInstances\">",
                        "/// A cache for created singleton service instances.",
                        "/// </param>",
                        "internal Scope(string id, Action disposeAction, ServiceCache singletonInstances)")
                        .BeginScope(
                            "Id = id;",
                            "DisposeAction = disposeAction;",
                            "ScopedInstances = new ServiceCache();",
                            "Factory = new ServiceFactory(ScopedInstances, singletonInstances);")
                        .EndScope(
                        _,
                        "/// <summary>",
                        "/// Gets a generated factory that is used to create service instances.",
                        "/// </summary>",
                        "private IServiceFactory Factory { get; }",
                        _,
                        "#endregion",
                        _,
                        "#region Data",
                        _,
                        "/// <summary>",
                        "/// Gets the scope's unique identifier.",
                        "/// </summary>",
                        "public string Id { get; }",
                        _,
                        "/// <summary>",
                        "/// Gets a delegate that is executed when this instance is disposed.",
                        "/// </summary>",
                        "private Action DisposeAction { get; }",
                        _,
                        "/// <summary>",
                        "/// A cache for created scoped service instances.",
                        "/// </summary>",
                        "private ServiceCache ScopedInstances { get; }",
                        _,
                         "#endregion",
                        _,
                        "#region Logic",
                        _,
                        "/// <inheritdoc cref=\"IDisposable\" />",
                        "public void Dispose()")
                        .BeginScope(
                            "DisposeAction();")
                        .EndScope(
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

        #endregion
    }
}
