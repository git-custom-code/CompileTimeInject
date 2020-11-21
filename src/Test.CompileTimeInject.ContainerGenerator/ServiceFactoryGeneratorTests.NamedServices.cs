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
        [Fact(DisplayName = "Class : IFoo (serviceId)")]
        public void GenerateServiceFactoryForNamedServices()
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

                      [Export(ServiceId = ""First"")]
                      public sealed class FirstFoo : IFoo
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(ServiceId = ""Second"")]
                      public sealed class SecondFoo : IFoo
                      { }
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
               @"Demo.Domain.IFoo IServiceFactory<Demo.Domain.IFoo>.CreateOrGetService()
                 {
                     var service = new Demo.Domain.Foo();
                     return service;
                 }"));
        }
    }
}
