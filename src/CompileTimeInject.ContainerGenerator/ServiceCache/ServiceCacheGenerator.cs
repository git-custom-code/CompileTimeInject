namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using CodeGeneration;
    using Metadata;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using System.Linq;
    using System;
    using System.Text;

    /// <summary>
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the "ServiceCache" type.
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate either the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System;
    ///     using System.Collections.Concurrent;
    ///
    ///     public sealed class ServiceCache : ConcurrentDictionary<Type, object>
    ///     { }
    /// }
    /// ]]>
    ///
    /// or the following code:
    ///
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System;
    ///     using System.Collections.Concurrent;
    ///
    ///     public sealed class ServiceCache : ConcurrentDictionary<Type, object>
    ///     {
    ///         private ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> NamedServiceCache { get; }
    ///             = new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();
    ///
    ///         public object GetOrAdd(Type key, string serviceId, Func<string, object> valueFactory)
    ///         {
    ///             var cache = NamedServiceCache.GetOrAdd(key, new ConcurrentDictionary<string, object>());
    ///             return cache.GetOrAdd(serviceId, valueFactory);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </example>
    [Generator]
    public sealed class ServiceCacheGenerator : ISourceGenerator
    {
        #region Logic

        /// <inheritdoc cref="ISourceGenerator" />
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ServiceCacheSyntaxReceiver());
        }

        /// <inheritdoc cref="ISourceGenerator" />
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var useNamedServices = false;
                if (context.SyntaxReceiver is ServiceCacheSyntaxReceiver currrentCompilation)
                {
                    useNamedServices = currrentCompilation.UseNamedServices;
                    if (!useNamedServices)
                    {
                        useNamedServices = context.Compilation
                            .GetReferencedNetAssemblies()
                            .Any(compilation => compilation.DefinesAnyNamedService());
                    }
                }

                var code = useNamedServices ? CreateNamedServiceCacheType() : CreateServiceCacheType();
                context.AddSource("ServiceCache", SourceText.From(code, Encoding.UTF8));
            }
            catch (Exception e)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "CTI007",
                        title: "Can't generate the ServiceCache type",
                        messageFormat: $"{nameof(ServiceCacheGenerator)}: {{0}}",
                        category: "CompileTimeInject.ServiceCacheGenerator",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        description: "There was an unexpected exception generating the ServiceCache type"),
                    Location.None,
                    e);
                context.ReportDiagnostic(diagnostic);
            }
        }

        /// <summary>
        /// Create the in-memory source code for the default "ServiceCache" type.
        /// </summary>
        /// <returns> The created in-memory source code. </returns>
        private string CreateServiceCacheType()
        {
            var _ = string.Empty;

            var code = new CodeBuilder(
                "namespace CustomCode.CompileTimeInject.GeneratedCode")
                .BeginScope(
                    "using System;",
                    "using System.Collections.Concurrent;",
                    _,
                    "/// <summary>",
                    "/// Specialized <see cref=\"ConcurrentDictionary{TKey, TValue}\"/> that is used to store",
                    "/// created singleton or scoped service instances.",
                    "/// </summary>",
                    "public sealed class ServiceCache : ConcurrentDictionary<Type, object>")
                    .BeginScope()
                    .EndScope()
                .EndScope();
            return code.ToString();
        }

        /// <summary>
        /// Create the in-memory source code for the specialized "ServiceCache" type,
        /// that can create and store additional named service instances.
        /// </summary>
        /// <returns> The created in-memory source code. </returns>
        private string CreateNamedServiceCacheType()
        {
            var _ = string.Empty;

            var code = new CodeBuilder(
                "namespace CustomCode.CompileTimeInject.GeneratedCode")
                .BeginScope(
                    "using System;",
                    "using System.Collections.Concurrent;",
                    _,
                    "/// <summary>",
                    "/// Specialized <see cref=\"ConcurrentDictionary{TKey, TValue}\"/> that is used to store",
                    "/// created singleton or scoped service instances, as well as named service instances.",
                    "/// </summary>",
                    "public sealed class ServiceCache : ConcurrentDictionary<Type, object>")
                    .BeginScope(
                        "#region Data",
                        _,
                        "/// <summary>",
                        "/// Gets a secondary cache for created named service instances.",
                        "/// </summary>",
                        "private ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> NamedServiceCache { get; }")
                    .Indent("= new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();")
                    .ContinueWith(
                        _,
                        "#endregion",
                        _,
                        "#region Logic",
                        _,
                        "/// <summary>",
                        "/// Create or get a named service instance.",
                        "/// </summary>",
                        "/// <param name=\"key\"> The type of the named service's contract. </param>",
                        "/// <param name=\"serviceId\"> The named service's unique id. </param>",
                        "/// <param name=\"valueFactory\">",
                        "/// A factory that can create a named service instance for a given <paramref=\"serviceId\"/>.",
                        "/// </param>",
                        "/// <returns> The named service instance. </returns>",
                        "public object GetOrAdd(Type key, string serviceId, Func<string, object> valueFactory)")
                        .BeginScope(
                            "var cache = NamedServiceCache.GetOrAdd(key, new ConcurrentDictionary<string, object>());",
                            "return cache.GetOrAdd(serviceId, valueFactory);")
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
