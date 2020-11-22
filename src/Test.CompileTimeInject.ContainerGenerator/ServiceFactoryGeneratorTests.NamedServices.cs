namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using Extensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Syntax;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="ServiceFactoryGenerator"/> type.
    /// </summary>
    public sealed partial class ServiceFactoryGeneratorTests
    {
        [Fact(DisplayName = "Class : IFoo (named: transient/transient)")]
        public void GenerateTwoTransientNamedServices()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      {
                          string Id { get; }
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(ServiceId = ""1"")]
                      public sealed class FirstFoo : IFoo
                      {
                          public string Id { get; } = ""1"";
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(ServiceId = ""2"")]
                      public sealed class SecondFoo : IFoo
                      {
                          public string Id { get; } = ""2"";
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "ServiceFactory",
               @"Demo.Domain.IFoo? INamedServiceFactory<Demo.Domain.IFoo>.CreateOrGetNamedService(string serviceId)
                 {
                     if (string.Equals(serviceId, ""1"", StringComparison.Ordinal))
                     {
                         var service = new Demo.Domain.FirstFoo();
                         return service;
                     }

                     if (string.Equals(serviceId, ""2"", StringComparison.Ordinal))
                     {
                         var service = new Demo.Domain.SecondFoo();
                         return service;
                     }

                     return default;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (named: scoped/scoped)")]
        public void GenerateTwoScopedNamedServices()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      {
                          string Id { get; }
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Scoped, ServiceId = ""1"")]
                      public sealed class FirstFoo : IFoo
                      {
                          public string Id { get; } = ""1"";
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Scoped, ServiceId = ""2"")]
                      public sealed class SecondFoo : IFoo
                      {
                          public string Id { get; } = ""2"";
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "ServiceFactory",
               @"Demo.Domain.IFoo? INamedServiceFactory<Demo.Domain.IFoo>.CreateOrGetNamedService(string serviceId)
                 {
                     if (string.Equals(serviceId, ""1"", StringComparison.Ordinal))
                     {
                         var service = (Demo.Domain.IFoo)ScopedInstances.GetOrAdd(typeof(Demo.Domain.IFoo), serviceId, _ =>
                             {
                                 var service = new Demo.Domain.FirstFoo();
                                 return service;
                             });
                         return service;
                     }

                     if (string.Equals(serviceId, ""2"", StringComparison.Ordinal))
                     {
                         var service = (Demo.Domain.IFoo)ScopedInstances.GetOrAdd(typeof(Demo.Domain.IFoo), serviceId, _ =>
                             {
                                 var service = new Demo.Domain.SecondFoo();
                                 return service;
                             });
                         return service;
                     }

                     return default;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (named: singleton/singleton)")]
        public void GenerateTwoSingletonNamedServices()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      {
                          string Id { get; }
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Singleton, ServiceId = ""1"")]
                      public sealed class FirstFoo : IFoo
                      {
                          public string Id { get; } = ""1"";
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Singleton, ServiceId = ""2"")]
                      public sealed class SecondFoo : IFoo
                      {
                          public string Id { get; } = ""2"";
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "ServiceFactory",
               @"Demo.Domain.IFoo? INamedServiceFactory<Demo.Domain.IFoo>.CreateOrGetNamedService(string serviceId)
                 {
                     if (string.Equals(serviceId, ""1"", StringComparison.Ordinal))
                     {
                         var service = (Demo.Domain.IFoo)SingletonInstances.GetOrAdd(typeof(Demo.Domain.IFoo), serviceId, _ =>
                             {
                                 var service = new Demo.Domain.FirstFoo();
                                 return service;
                             });
                         return service;
                     }

                     if (string.Equals(serviceId, ""2"", StringComparison.Ordinal))
                     {
                         var service = (Demo.Domain.IFoo)SingletonInstances.GetOrAdd(typeof(Demo.Domain.IFoo), serviceId, _ =>
                             {
                                 var service = new Demo.Domain.SecondFoo();
                                 return service;
                             });
                         return service;
                     }

                     return default;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (named: transient/scoped)")]
        public void GenerateNamedTransientAndScopedServices()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      {
                          string Id { get; }
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(ServiceId = ""1"")]
                      public sealed class FirstFoo : IFoo
                      {
                          public string Id { get; } = ""1"";
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Scoped, ServiceId = ""2"")]
                      public sealed class SecondFoo : IFoo
                      {
                          public string Id { get; } = ""2"";
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "ServiceFactory",
               @"Demo.Domain.IFoo? INamedServiceFactory<Demo.Domain.IFoo>.CreateOrGetNamedService(string serviceId)
                 {
                     if (string.Equals(serviceId, ""1"", StringComparison.Ordinal))
                     {
                         var service = new Demo.Domain.FirstFoo();
                         return service;
                     }

                     if (string.Equals(serviceId, ""2"", StringComparison.Ordinal))
                     {
                         var service = (Demo.Domain.IFoo)ScopedInstances.GetOrAdd(typeof(Demo.Domain.IFoo), serviceId, _ =>
                             {
                                 var service = new Demo.Domain.SecondFoo();
                                 return service;
                             });
                         return service;
                     }

                     return default;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (named: transient/singleton)")]
        public void GenerateNamedTransientAndSingletonServices()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      {
                          string Id { get; }
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(ServiceId = ""1"")]
                      public sealed class FirstFoo : IFoo
                      {
                          public string Id { get; } = ""1"";
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Singleton, ServiceId = ""2"")]
                      public sealed class SecondFoo : IFoo
                      {
                          public string Id { get; } = ""2"";
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "ServiceFactory",
               @"Demo.Domain.IFoo? INamedServiceFactory<Demo.Domain.IFoo>.CreateOrGetNamedService(string serviceId)
                 {
                     if (string.Equals(serviceId, ""1"", StringComparison.Ordinal))
                     {
                         var service = new Demo.Domain.FirstFoo();
                         return service;
                     }

                     if (string.Equals(serviceId, ""2"", StringComparison.Ordinal))
                     {
                         var service = (Demo.Domain.IFoo)SingletonInstances.GetOrAdd(typeof(Demo.Domain.IFoo), serviceId, _ =>
                             {
                                 var service = new Demo.Domain.SecondFoo();
                                 return service;
                             });
                         return service;
                     }

                     return default;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (named: scoped/singleton)")]
        public void GenerateNamedScopedAndSingletonServices()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      {
                          string Id { get; }
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Scoped, ServiceId = ""1"")]
                      public sealed class FirstFoo : IFoo
                      {
                          public string Id { get; } = ""1"";
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Singleton, ServiceId = ""2"")]
                      public sealed class SecondFoo : IFoo
                      {
                          public string Id { get; } = ""2"";
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "ServiceFactory",
               @"Demo.Domain.IFoo? INamedServiceFactory<Demo.Domain.IFoo>.CreateOrGetNamedService(string serviceId)
                 {
                     if (string.Equals(serviceId, ""1"", StringComparison.Ordinal))
                     {
                         var service = (Demo.Domain.IFoo)ScopedInstances.GetOrAdd(typeof(Demo.Domain.IFoo), serviceId, _ =>
                             {
                                 var service = new Demo.Domain.FirstFoo();
                                 return service;
                             });
                         return service;
                     }

                     if (string.Equals(serviceId, ""2"", StringComparison.Ordinal))
                     {
                         var service = (Demo.Domain.IFoo)SingletonInstances.GetOrAdd(typeof(Demo.Domain.IFoo), serviceId, _ =>
                             {
                                 var service = new Demo.Domain.SecondFoo();
                                 return service;
                             });
                         return service;
                     }

                     return default;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (named: transient/scoped/singleton)")]
        public void GenerateNamedTransientAndScopedAndSingletonServices()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      {
                          string Id { get; }
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(ServiceId = ""1"")]
                      public sealed class FirstFoo : IFoo
                      {
                          public string Id { get; } = ""1"";
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Scoped, ServiceId = ""2"")]
                      public sealed class SecondFoo : IFoo
                      {
                          public string Id { get; } = ""2"";
                      }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Singleton, ServiceId = ""3"")]
                      public sealed class ThirdFoo : IFoo
                      {
                          public string Id { get; } = ""3"";
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "ServiceFactory",
               @"Demo.Domain.IFoo? INamedServiceFactory<Demo.Domain.IFoo>.CreateOrGetNamedService(string serviceId)
                 {
                     if (string.Equals(serviceId, ""1"", StringComparison.Ordinal))
                     {
                         var service = new Demo.Domain.FirstFoo();
                         return service;
                     }

                     if (string.Equals(serviceId, ""2"", StringComparison.Ordinal))
                     {
                         var service = (Demo.Domain.IFoo)ScopedInstances.GetOrAdd(typeof(Demo.Domain.IFoo), serviceId, _ =>
                             {
                                 var service = new Demo.Domain.SecondFoo();
                                 return service;
                             });
                         return service;
                     }

                     if (string.Equals(serviceId, ""3"", StringComparison.Ordinal))
                     {
                         var service = (Demo.Domain.IFoo)SingletonInstances.GetOrAdd(typeof(Demo.Domain.IFoo), serviceId, _ =>
                             {
                                 var service = new Demo.Domain.ThirdFoo();
                                 return service;
                             });
                         return service;
                     }

                     return default;
                 }"));
        }
    }
}
