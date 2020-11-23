namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using CodeGeneration;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Text;

    /// <summary>
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the "InvalidServiceException" type.
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     using System;
    ///
    ///     public sealed class InvalidServiceException : Exception
    ///     {
    ///         public InvalidServiceException(Type serviceContract)
    ///             : base($"A service with contract <{serviceContract.Name}> could not be created.")
    ///         {
    ///             ServiceContract = serviceContract;
    ///             ServiceId = null;
    ///         }
    ///         
    ///         public InvalidServiceException(Type serviceContract, string serviceId)
    ///             : base($"A service with contract <{serviceContract.Name}> and id <{serviceId}> could not be created.")
    ///         {
    ///             ServiceContract = serviceContract;
    ///             ServiceId = serviceId;
    ///         }
    ///
    ///         public Type ServiceContract { get; }
    ///
    ///         public string? ServiceId { get; }
    ///     }
    /// }
    /// ]]>
    /// </example>
    [Generator]
    public sealed class InvalidServiceExceptionGenerator : ISourceGenerator
    {
        #region Logic

        /// <inheritdoc cref="ISourceGenerator" />
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this generator
        }

        /// <inheritdoc cref="ISourceGenerator" />
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var code = CreateInvalidServiceExceptionType();
                context.AddSource("InvalidServiceException", SourceText.From(code, Encoding.UTF8));
            }
            catch (Exception e)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "CTI008",
                        title: "Can't generate the InvalidServiceException type",
                        messageFormat: $"{nameof(InvalidServiceExceptionGenerator)}: {{0}}",
                        category: "CompileTimeInject.InvalidServiceException",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        description: "There was an unexpected exception generating the InvalidServiceException type"),
                    Location.None,
                    e);
                context.ReportDiagnostic(diagnostic);
            }
        }

        /// <summary>
        /// Create the in-memory source code for the default "InvalidServiceException" type.
        /// </summary>
        /// <returns> The created in-memory source code. </returns>
        private string CreateInvalidServiceExceptionType()
        {
            var _ = string.Empty;

            var code = new CodeBuilder(
                "namespace CustomCode.CompileTimeInject.GeneratedCode")
                .BeginScope(
                    "using System;",
                    _,
                    "/// <summary>",
                    "/// A specialized <see cref=\"Exception\"/> that is thrown by the <see cref=\"IocContainer\"/>",
                    "/// when an invalid service was requested.",
                    "/// </summary>",
                    "public sealed class InvalidServiceException : Exception")
                    .BeginScope(
                        "#region Dependencies",
                        _,
                        "/// <summary>",
                        "/// Creates a new instance of the <see cref=\"InvalidServiceException\"/> type.",
                        "/// </summary>",
                        "/// <param name=\"serviceContract\"> The contract by which the service was requested. </param>",
                        "public InvalidServiceException(Type serviceContract)")
                        .Indent(": base($\"A service with contract <{serviceContract.Name}> could not be created.\")")
                        .BeginScope(
                            "ServiceContract = serviceContract;",
                            "ServiceId = null;")
                        .EndScope(
                        _,
                        "/// <summary>",
                        "/// Creates a new instance of the <see cref=\"InvalidServiceException\"/> type.",
                        "/// </summary>",
                        "/// <param name=\"serviceContract\"> The contract by which the service was requested. </param>",
                        "/// <param name=\"serviceId\"> The unique identifier by which the service was requested. </param>",
                        "public InvalidServiceException(Type serviceContract, string serviceId)")
                        .Indent(": base($\"A service with contract <{serviceContract.Name}> and id <{serviceId}> could not be created.\")")
                        .BeginScope(
                            "ServiceContract = serviceContract;",
                            "ServiceId = serviceId;")
                        .EndScope(
                        _,
                        "#endregion",
                        _,
                        "#region Data",
                        _,
                        "/// <summary>",
                        "/// Gets the contract by which the service was requested.",
                        "/// </summary>",
                        "public Type ServiceContract { get; }",
                        _,
                        "/// <summary>",
                        "/// Gets the unique identifier by which the service was onptionally requested.",
                        "/// </summary>",
                        "public string? ServiceId { get; }",
                        _,
                        "#endregion")
                    .EndScope()
                .EndScope();
            return code.ToString();
        }

        #endregion
    }
}
