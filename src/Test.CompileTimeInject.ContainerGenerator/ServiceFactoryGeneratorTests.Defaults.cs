namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using Extensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Syntax;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="ScopeGenerator"/> type.
    /// </summary>
    public sealed partial class ServiceFactoryGeneratorTests
    {
        [Fact(DisplayName = "Class")]
        public void GenerateServiceFactoryForClass()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Foo
                      { }
                  }");
            var sourceGenerator = new ScopeGenerator();
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
               @"Demo.Domain.Foo IServiceFactory<Demo.Domain.Foo>.CreateOrGetService()
                 {
                     var service = new Demo.Domain.Foo();
                     return service;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo")]
        public void GenerateServiceFactoryForClassWithSingleInterface()
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
            var sourceGenerator = new ScopeGenerator();
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

        [Fact(DisplayName = "Class : IFoo, IBar")]
        public void GenerateServiceFactoryForClassWithMultipleInterfaces()
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
                      public interface IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Foo : IFoo, IBar
                      { }
                  }");
            var sourceGenerator = new ScopeGenerator();
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
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "ServiceFactory",
               @"Demo.Domain.IBar IServiceFactory<Demo.Domain.IBar>.CreateOrGetService()
                 {
                     var service = new Demo.Domain.Foo();
                     return service;
                 }"));
        }

        [Fact]
        public void GenerateServiceFactoryForClassWithoutInheritedInterface()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      public interface IFoo : IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Foo : IFoo
                      { }
                  }");
            var sourceGenerator = new ScopeGenerator();
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
