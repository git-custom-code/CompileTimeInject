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
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the
    /// generic "INamedServiceFactory{T}" interface (if and only if the developer has defined
    /// a named service, i.e. a service with a non-null serviceId in it's export attribute).
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     public interface INamedServiceFactory<T> where T : class
    ///     {
    ///         T? CreateOrGetNamedService(string serviceId);
    ///     }
    /// }
    /// ]]>
    /// </example>
    [Generator]
    public sealed class INamedServiceFactoryGenerator : ISourceGenerator
    {
        #region Logic

        /// <inheritdoc cref="ISourceGenerator" />
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new INamedServiceFactorySyntaxReceiver());
        }

        /// <inheritdoc cref="ISourceGenerator" />
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var useNamedServices = false;
                if (context.SyntaxReceiver is INamedServiceFactorySyntaxReceiver currrentCompilation)
                {
                    if (context.Compilation.IsIocVisibleAssembly())
                    {
                        useNamedServices = currrentCompilation.UseNamedServices;
                    }

                    if (!useNamedServices)
                    {
                        useNamedServices = context.Compilation
                            .GetReferencedIocVisibleAssemblies()
                            .Any(compilation => compilation.DefinesAnyNamedService());
                    }
                }

                if (useNamedServices)
                {
                    var code = CreateNamedServiceFactoryInterface();
                    context.AddSource("INamedServiceFactory", SourceText.From(code, Encoding.UTF8));
                }
            }
            catch (Exception e)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "CTI006",
                        title: "Can't generate the INamedServiceFactory interface",
                        messageFormat: $"{nameof(INamedServiceFactoryGenerator)}: {{0}}",
                        category: "CompileTimeInject.INamedServiceFactoryGenerator",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        description: "There was an unexpected exception generating the INamedServiceFactory interface"),
                    Location.None,
                    e);
                context.ReportDiagnostic(diagnostic);
            }
        }

        /// <summary>
        /// Create the in-memory source code for the "INamedServiceFactory{T}" interface.
        /// </summary>
        /// <returns> The created in-memory source code. </returns>
        private string CreateNamedServiceFactoryInterface()
        {
            var code = new CodeBuilder(
                "namespace CustomCode.CompileTimeInject.GeneratedCode")
                .BeginScope(
                    "/// <summary>",
                    "/// Interface for a factory that is able to create a new instance of a service that implements",
                    "/// a given contract and is identified by a unique service id.",
                    "/// </summary>",
                    "/// <typeparam name=\"T\"> The type of the contract that is implemented by the service. </typeparam>",
                    "public interface INamedServiceFactory<T> where T : class")
                    .BeginScope(
                        "/// <summary>",
                        "/// Creates a new service instance that implements a contract of type <typeparamref name=\"T\"/>.",
                        "/// </summary>",
                        "/// <param name=\"serviceId\"> The service's unique identifier. </param>",
                        "/// <returns> The newly created service instance. </returns>",
                        "T? CreateOrGetNamedService(string serviceId);")
                    .EndScope()
                .EndScope();
            return code.ToString();
        }

        #endregion
    }
}
