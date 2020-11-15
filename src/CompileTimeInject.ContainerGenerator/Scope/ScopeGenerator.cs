namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using CodeGeneration;
    using Metadata;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Linq;
    using System.Reflection.Metadata;
    using System.Text;

    /// <summary>
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the Scope type.
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System;
    ///     using System.Collections.Concurrent;
    ///     using System.Collections.Generic;
    ///     using System.Linq;
    ///
    ///     public sealed class Scope : IDisposable
    ///     {
    ///         internal Scope(string id, Action disposeAction, ConcurrentDictionary<Type, object> singletonInstances)
    ///         {
    ///             Id = id;
    ///             DisposeAction = disposeAction;
    ///             ScopedInstances = new ConcurrentDictionary<Type, object>();
    ///             Factory = new ServiceFactory(singletonInstances, ScopedInstances);
    ///         }
    ///
    ///         private IServiceFactory Factory { get; }
    ///
    ///         public string Id { get; }
    ///
    ///         private Action DisposeAction { get; }
    ///
    ///         private ConcurrentDictionary<Type, object> ScopedInstances { get; }
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
                var useLifetimeScoped = false;

                // detect scoped services in the current compilation
                if (context.SyntaxReceiver is ScopeSyntaxReceiver detector)
                {
                    useLifetimeScoped = detector.UseLifetimeScoped;
                    if (!useLifetimeScoped)
                    {
                        // detect scoped services in referenced assemblies
                        foreach (var reference in context.Compilation.References.OfType<PortableExecutableReference>())
                        {
                            var reader = reference.GetMetadataReader();
                            if (reader == null)
                            {
                                continue;
                            }

                            useLifetimeScoped = reader.DefinesServiceWithLifetimeScoped();
                            if (useLifetimeScoped)
                            {
                                break;
                            }
                        }
                    }
                }

                if (useLifetimeScoped)
                {
                    var code = CreateScopeType();
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
        /// Create the in-memory source code for the Scope type.
        /// </summary>
        /// <returns> The created in-memory source code. </returns>
        private string CreateScopeType()
        {
            var _ = string.Empty;

            var code = new CodeBuilder(
                "namespace CustomCode.CompileTimeInject.GeneratedCode")
                .BeginScope(
                    "using System;",
                    "using System.Collections.Concurrent;",
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
                        "internal Scope(string id, Action disposeAction, ConcurrentDictionary<Type, object> singletonInstances)")
                        .BeginScope(
                            "Id = id;",
                            "DisposeAction = disposeAction;",
                            "ScopedInstances = new ConcurrentDictionary<Type, object>();",
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
                        "private ConcurrentDictionary<Type, object> ScopedInstances { get; }",
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
                        .EndScope(
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
