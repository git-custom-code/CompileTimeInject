namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using CodeGeneration;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Text;

    /// <summary>
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the "IocContainer" type.
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System.Collections.Generic;
    ///     using System.Ling;
    ///
    ///     public sealed class IocContainer
    ///     {
    ///         private ServiceFactory Factory { get; } = new ServiceFactory();
    ///
    ///         public T? GetService<T>() where T : class
    ///         {
    ///             var factory = Factory as IServiceFactory<T>;
    ///             return factory?.CreateOrGetService();
    ///         }
    ///
    ///         public IEnumerable<T> GetServices<T>() where T : class
    ///         {
    ///             var collectionFactory = Factory as IServiceFactory<IEnumerable<T>>;
    ///             if (collectionFactory != null)
    ///             {
    ///                 return collectionFactory.CreateOrGetService();
    ///             }
    ///
    ///             var factory = Factory as IServiceFactory<T>;
    ///             if (factory != null)
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
    public sealed class IocContainerGenerator : ISourceGenerator
    {
        #region Logic

        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this generator
        }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var code = CreateIocContainerType();
                context.AddSource("IocContainer", SourceText.From(code, Encoding.UTF8));
            }
            catch (Exception e)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "CTI003",
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
        /// <returns> The created in-memory source code. </returns>
        private string CreateIocContainerType()
        {
            var _ = string.Empty;

            var code = new CodeBuilder(
                "namespace CustomCode.CompileTimeInject.GeneratedCode")
                .BeginScope(
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
                        "/// Gets the generated factory that is used to create service instances.",
                        "/// </summary>",
                        "private ServiceFactory Factory { get; } = new ServiceFactory();",
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
                        "/// Gets a collection of service implementations of the same contract.",
                        "/// </summary>",
                        "/// <typeparam name=\"T\"> The service contract whose implementations should be retrieved. </typeparam>",
                        "/// <returns> The service contract's implementations or <see cref=\"Enumerable.Empty{T}\"/> if no such implementations exists. </returns>",
                        "public IEnumerable<T> GetServices<T>() where T : class")
                        .BeginScope(
                            "var collectionFactory = Factory as IServiceFactory<IEnumerable<T>>;",
                            "if (collectionFactory != null)")
                            .BeginScope(
                                "return collectionFactory.CreateOrGetService();")
                            .EndScope(
                        _,
                        "var factory = Factory as IServiceFactory<T>;",
                        "if (factory != null)")
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
