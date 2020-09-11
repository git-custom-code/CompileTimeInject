namespace CustomCode.CompileTimeInject.ContainerGenerator
{
    using CustomCode.CompileTimeInject.ContainerGenerator.Metadata;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata;
    using System.Text;

    /// <summary>
    /// Implementation of an <see cref="ISourceGenerator"/> that is used to generate the ServiceFactory type.
    /// </summary>
    /// <example>
    /// This SourceGenerator will generate the following code:
    /// <![CDATA[
    /// namespace CustomCode.CompileTimeInject.GeneratedCode
    /// {
    ///     public sealed partial class ServiceFactory
    ///         : IServiceFactory<IFoo1>
    ///         , IServiceFactory<IFoo2>
    ///         ...
    ///         , IServiceFactory<IFooN>
    ///     {
    ///         IFoo1 IServiceFactory<IFoo1>.CreateOrGetService()
    ///         {
    ///             return new Foo1();
    ///         }
    ///         
    ///         IFoo2 IServiceFactory<IFoo2>.CreateOrGetService()
    ///         {
    ///             return new Foo2();
    ///         }
    ///
    ///         ...
    ///
    ///         IFooN IServiceFactory<IFooN>.CreateOrGetService()
    ///         {
    ///             return new FooN(dep0);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </example>
    [Generator]
    public sealed class ServiceFactoryGenerator : ISourceGenerator
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
                var detectedServices = new List<ServiceDescriptor>();
                foreach (var reference in context.Compilation.References.OfType<PortableExecutableReference>())
                {
                    var reader = reference.GetMetadataReader();
                    if (reader == null)
                    {
                        continue;
                    }

                    foreach (var typeDefinition in reader.GetExportedTypeDefinitions())
                    {
                        var implementation = reader.ToTypeDescriptor(typeDefinition);
                        detectedServices.Add(new ServiceDescriptor(implementation));
                    }
                }

                var code = CreateServiceFactory(detectedServices);
                context.AddSource("ServiceFactory", SourceText.From(code, Encoding.UTF8));
            }
            catch (Exception e)
            {
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "CTI002",
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
            const string t = "    ";

            var code = new StringBuilder();
            code.AppendLine("namespace CustomCode.CompileTimeInject.GeneratedCode");
            code.AppendLine("{");

            code.AppendLine($"{t}/// <summary>");
            code.AppendLine($"{t}/// Default implementation for each <see cref=\"IServiceFactory{{T}}\"/> interface.");
            code.AppendLine($"{t}/// </summary>");
            code.AppendLine($"{t}public sealed partial class ServiceFactory");
            if (services.Any())
            {
                var firstService = services.First();
                code.AppendLine($"{t}{t}: IServiceFactory<{firstService.Contract.FullName}>");
                foreach (var service in services.Skip(1))
                {
                    code.AppendLine($"{t}{t}, IServiceFactory<{service.Contract.FullName}>");
                }
            }
            code.AppendLine($"{t}{{");

            foreach (var service in services)
            {
                code.AppendLine($"{t}/// <inheritdoc />");
                code.AppendLine($"{t}{t}{service.Contract.FullName} IServiceFactory<{service.Contract.FullName}>.CreateOrGetService()");
                code.AppendLine($"{t}{t}{{");

                code.AppendLine($"{t}{t}{t}var service = new {service.Implementation.FullName}();");
                code.AppendLine($"{t}{t}{t}return service;");
                code.AppendLine($"{t}{t}}}");
            }

            code.AppendLine($"{t}}}");

            code.AppendLine("}");
            return code.ToString();
        }

        #endregion
    }
}
