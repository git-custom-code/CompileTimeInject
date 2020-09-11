namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Text;

    /// <summary>
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the "IServiceFactory{T}" interface.
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     public interface IServiceFactory<T> where T : class
    ///     {
    ///         T CreateOrGetService();
    ///     }
    /// }
    /// ]]>
    /// </example>
    [Generator]
    public sealed class IServiceFactoryGenerator : ISourceGenerator
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
                var code = CreateServiceFactoryInterface();
                context.AddSource("IServiceFactory", SourceText.From(code, Encoding.UTF8));
            }
            catch (Exception e)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "CTI001",
                        title: "Can't generate the IServiceFactory<T> interface",
                        messageFormat: $"{nameof(IServiceFactoryGenerator)}: {{0}}",
                        category: "CompileTimeInject.ContainerGenerator",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        description: "There was an unexpected exception generating the IServiceFactory<T> interface"),
                    Location.None,
                    e);
                context.ReportDiagnostic(diagnostic);
            }
        }

        /// <summary>
        /// Create the in-memory source code for the "IServiceFactory{T}" interface.
        /// </summary>
        /// <returns> The created in-memory source code. </returns>
        private string CreateServiceFactoryInterface()
        {
            const string t = "    ";

            var code = new StringBuilder();
            code.AppendLine("namespace CustomCode.CompileTimeInject.GeneratedCode");
            code.AppendLine("{");

            code.AppendLine($"{t}/// <summary>");
            code.AppendLine($"{t}/// Interface for a factory that is able to create a new instance of a service that implements a given contract.");
            code.AppendLine($"{t}/// </summary>");
            code.AppendLine($"{t}/// <typeparam name=\"T\"> The type of the contract that is implemented by the service. </typeparam>");
            code.AppendLine($"{t}public interface IServiceFactory<T> where T : class");
            code.AppendLine($"{t}{{");

            code.AppendLine($"{t}/// <summary>");
            code.AppendLine($"{t}/// Creates a new service instance that implements a contract of type <typeparamref name=\"T\"/>.");
            code.AppendLine($"{t}/// </summary>");
            code.AppendLine($"{t}/// <returns> The newly created service instance. </returns>");
            code.AppendLine($"{t}{t}T CreateOrGetService();");

            code.AppendLine($"{t}}}");

            code.AppendLine("}");
            return code.ToString();
        }

        #endregion
    }
}
