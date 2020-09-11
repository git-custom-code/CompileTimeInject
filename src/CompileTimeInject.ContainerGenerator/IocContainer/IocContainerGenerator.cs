namespace CustomCode.CompileTimeInject.ContainerGenerator
{
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
    ///     public sealed class IocContainer
    ///     {
    ///         private ServiceFactory Factory { get; } = new ServiceFactory();
    ///
    ///         public T? GetService<T>() where T : class
    ///         {
    ///             var factory = Factory as IServiceFactory<T>;
    ///             return factory?.CreateOrGetService();
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </example>
    [Generator]
    public sealed class IocConainerGenerator : ISourceGenerator
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
                var code = CreateIocContainerType();
                context.AddSource("IocContainer", SourceText.From(code, Encoding.UTF8));
            }
            catch (Exception e)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "CTI003",
                        title: "Can't generate the IocContainer type",
                        messageFormat: $"{nameof(IocConainerGenerator)}: {{0}}",
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
            const string t = "    ";

            var code = new StringBuilder();
            code.AppendLine("namespace CustomCode.CompileTimeInject.GeneratedCode");
            code.AppendLine("{");

            code.AppendLine($"{t}/// <summary>");
            code.AppendLine($"{t}/// A compile time generated inversion of control container.");
            code.AppendLine($"{t}/// </summary>");
            code.AppendLine($"{t}public sealed class IocContainer");
            code.AppendLine($"{t}{{");

            // Dependencies
            code.AppendLine($"{t}{t}#region Dependencies");
            code.AppendLine();
            code.AppendLine($"{t}{t}/// <summary>");
            code.AppendLine($"{t}{t}/// Gets the generated factory that is used to create service instances.");
            code.AppendLine($"{t}{t}/// </summary>");
            code.AppendLine($"{t}{t}private ServiceFactory Factory {{ get; }} = new ServiceFactory();");
            code.AppendLine();
            code.AppendLine($"{t}{t}#endregion");
            code.AppendLine();

            // Logic
            code.AppendLine($"{t}{t}#region Logic");
            code.AppendLine();
            code.AppendLine($"{t}{t}/// <summary>");
            code.AppendLine($"{t}{t}/// Gets a service implementation by contract.");
            code.AppendLine($"{t}{t}/// </summary>");
            code.AppendLine($"{t}{t}/// <typeparam name=\"T\"> The service contract whose implementation should be retrieved. </typeparam>");
            code.AppendLine($"{t}{t}/// <returns> The contract's service implementation or null if no such implementation exists. </returns>");
            code.AppendLine($"{t}{t}public T? GetService<T>() where T : class");
            code.AppendLine($"{t}{t}{{");
            code.AppendLine($"{t}{t}{t}var factory = Factory as IServiceFactory<T>;");
            code.AppendLine($"{t}{t}{t}return factory?.CreateOrGetService();");
            code.AppendLine($"{t}{t}}}");
            code.AppendLine();
            code.AppendLine($"{t}{t}#endregion");

            code.AppendLine($"{t}}}");
            code.AppendLine("}");
            return code.ToString();
        }

        #endregion
    }
}
