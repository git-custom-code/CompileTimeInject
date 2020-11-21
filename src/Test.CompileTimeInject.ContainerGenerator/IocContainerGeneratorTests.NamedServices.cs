namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using Extensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Syntax;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="IocContainerGenerator"/> type.
    /// </summary>
    public sealed partial class IocContainerGeneratorTests
    {
        [Fact(DisplayName = "GetServices(serviceId): not generated")]
        public void DoNotGenerateGetServicesForNamedServicesWhenNotNeeded()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
             @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
             @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Foo : IFoo
                      { }
                  }");
            var sourceGenerator = new IocContainerGenerator();
            var runtime = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            runtime.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.False(output.ContainsTypeWithMethodSignature(
                "IocContainer",
                "public T? GetService<T>(string serviceId) where T : class"));
        }

        [Fact(DisplayName = "GetServices(serviceId): without scopes")]
        public void GenerateGetServicesForNamedServicesWithoutScopes()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
             @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
             @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(ServiceId = ""1"")]
                      public sealed class Foo : IFoo
                      { }
                  }");
            var sourceGenerator = new IocContainerGenerator();
            var runtime = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            runtime.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "IocContainer",
               @"public T? GetService<T>(string serviceId) where T : class
                 {
                     var factory = Factory as INamedServiceFactory<T>;
                     return factory?.CreateOrGetNamedService(serviceId);
                 }"));
        }

        [Fact(DisplayName = "GetServices(serviceId): with scopes")]
        public void GenerateGetServicesForNamedServicesWithScopes()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
             @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
             @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Scoped, ServiceId = ""1"")]
                      public sealed class Foo : IFoo
                      { }
                  }");
            var sourceGenerator = new IocContainerGenerator();
            var runtime = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            runtime.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "IocContainer",
               @"public T? GetService<T>(string serviceId) where T : class
                 {
                     var scope = GetActiveScope();
                     var service = scope.GetService<T>(serviceId);
                     return service;
                 }"));
        }
    }
}
