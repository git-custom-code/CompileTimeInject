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
    ///     using System;
    ///     using System.Collections.Concurrent;
    ///     using System.Collections.Generic;
    ///
    ///     public sealed partial class ServiceFactory
    ///         : IServiceFactory<IFoo1>
    ///         , IServiceFactory<IFoo2>
    ///         ...
    ///         , IServiceFactory<IFooN>
    ///     {
    ///         private ConcurrentDictionary<Type, object> SingletonInstances { get; } = new ConcurrentDictionary<Type, object>();
    /// 
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

                    foreach (var service in reader.GetExportedServices())
                    {
                        var lifetime = Lifetime.Transient;
                        TypeDescriptor? contractFilter = null; 
                        foreach(var value in service.ExportAttribute.FixedArguments)
                        {
                            if (value.Type.FullName == typeof(Lifetime).FullName)
                            {
                                lifetime = (Lifetime)value.Value;
                            }
                            else if (value.Type.FullName == typeof(Type).FullName)
                            {
                                contractFilter = (TypeDescriptor)value.Value;
                            }
                        }

                        var implementation = reader.ToTypeDescriptor(service.TypeDefinition);
                        if (contractFilter.HasValue)
                        {
                            detectedServices.Add(new ServiceDescriptor(contractFilter.Value, implementation, lifetime));
                        }
                        else
                        {
                            var interfaceImplementations = service.TypeDefinition.GetInterfaceImplementations();
                            if (interfaceImplementations.Count == 0)
                            {
                                detectedServices.Add(new ServiceDescriptor(implementation, lifetime));
                            }
                            else
                            {
                                foreach (var implementationHandle in interfaceImplementations)
                                {
                                    var interfaceImplementation = reader.GetInterfaceImplementation(implementationHandle);
                                    var contractType = reader.GetTypeDefinition((TypeDefinitionHandle)interfaceImplementation.Interface);
                                    var contract = reader.ToTypeDescriptor(contractType);
                                    detectedServices.Add(new ServiceDescriptor(contract, implementation, lifetime));
                                }
                            }
                        }
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
            code.AppendLine($"{t}using System;");
            code.AppendLine($"{t}using System.Collections.Concurrent;");
            code.AppendLine($"{t}using System.Collections.Generic;");
            code.AppendLine();

            code.AppendLine($"{t}/// <summary>");
            code.AppendLine($"{t}/// Default implementation for each <see cref=\"IServiceFactory{{T}}\"/> interface.");
            code.AppendLine($"{t}/// </summary>");
            code.AppendLine($"{t}public sealed partial class ServiceFactory");

            var serviceGroups = services.GroupBy(service => service.Contract).ToList();
            if (serviceGroups.Any())
            {
                var separator = ":";
                foreach (var serviceGroup in serviceGroups)
                {
                    if (serviceGroup.Count() > 1)
                    {
                        code.AppendLine($"{t}{t}{separator} IServiceFactory<IEnumerable<{serviceGroup.Key.FullName}>>");
                    }
                    else
                    {
                        code.AppendLine($"{t}{t}{separator} IServiceFactory<{serviceGroup.Key.FullName}>");
                    }
                    separator = ",";
                }
            }
            code.AppendLine($"{t}{{");

            // Data
            code.AppendLine($"{t}{t}#region Data");
            code.AppendLine();
            code.AppendLine($"{t}{t}/// <summary>");
            code.AppendLine($"{t}{t}/// Gets a cache for created singleton service instances.");
            code.AppendLine($"{t}{t}/// </summary>");
            code.AppendLine($"{t}{t}private ConcurrentDictionary<Type, object> SingletonInstances {{ get; }} = new ConcurrentDictionary<Type, object>();");
            code.AppendLine();
            code.AppendLine($"{t}{t}#endregion");
            code.AppendLine();

            // Logic
            code.AppendLine($"{t}{t}#region Logic");
            code.AppendLine();

            foreach (var serviceGroup in serviceGroups)
            {
                if (serviceGroup.Count() > 1)
                {
                    code.AppendLine($"{t}{t}/// <inheritdoc />");
                    code.AppendLine($"{t}{t}IEnumerable<{serviceGroup.Key.FullName}> IServiceFactory<IEnumerable<{serviceGroup.Key.FullName}>>.CreateOrGetService()");
                    code.AppendLine($"{t}{t}{{");
                    code.AppendLine($"{t}{t}{t}var services = new List<{serviceGroup.Key.FullName}>();");
                    code.AppendLine();

                    var implementationCount = 0;
                    foreach (var service in serviceGroup)
                    {
                        if (service.Lifetime == Lifetime.Singleton)
                        {
                            code.AppendLine($"{t}{t}{t}var service{implementationCount} = ({service.Contract.FullName})SingletonInstances.GetOrAdd(typeof({service.Contract.FullName}), _ => new {service.Implementation.FullName}());");
                        }
                        else
                        {
                            code.AppendLine($"{t}{t}{t}var service{implementationCount} = new {service.Implementation.FullName}();");
                        }

                        code.AppendLine($"{t}{t}{t}services.Add(service{implementationCount});");
                        code.AppendLine();
                        ++implementationCount;
                    }

                    code.AppendLine($"{t}{t}{t}return services;");
                    code.AppendLine($"{t}{t}}}");
                }
                else
                {

                    var service = serviceGroup.First();
                    code.AppendLine($"{t}{t}/// <inheritdoc />");
                    code.AppendLine($"{t}{t}{service.Contract.FullName} IServiceFactory<{service.Contract.FullName}>.CreateOrGetService()");
                    code.AppendLine($"{t}{t}{{");

                    if (service.Lifetime == Lifetime.Singleton)
                    {
                        code.AppendLine($"{t}{t}{t}var service = ({service.Contract.FullName})SingletonInstances.GetOrAdd(typeof({service.Contract.FullName}), _ => new {service.Implementation.FullName}());");
                    }
                    else
                    {
                        code.AppendLine($"{t}{t}{t}var service = new {service.Implementation.FullName}();");
                    }

                    code.AppendLine($"{t}{t}{t}return service;");
                    code.AppendLine($"{t}{t}}}");
                }
                code.AppendLine();
            }

            code.AppendLine($"{t}{t}#endregion");

            code.AppendLine($"{t}}}");

            code.AppendLine("}");
            return code.ToString();
        }

        #endregion
    }
}
