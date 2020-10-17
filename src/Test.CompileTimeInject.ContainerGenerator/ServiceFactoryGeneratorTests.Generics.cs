namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using Extensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Syntax;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="ServiceFactoryGenerator"/> type for generice services.
    /// </summary>
    public sealed partial class ServiceFactoryGeneratorTests
    {
        [Fact(DisplayName = "Class : IFoo<T>")]
        public void GenerateServiceFactoryForClassWithSingleGenericInterfaceWithOneGenericParameter()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo<T>
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Foo<T> : IFoo<T>
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
               @"Demo.Domain.IFoo<T> IServiceFactory<Demo.Domain.IFoo<T>>.CreateOrGetService()
                 {
                     var service = new Demo.Domain.Foo<T>();
                     return service;
                 }"));
        }

        [Fact(DisplayName ="Class : IFoo<T1, T2>")]
        public void GenerateServiceFactoryForClassWithSingleGenericInterfaceWithTwoGenericParameters()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo<T1, T2>
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Foo<T1, T2> : IFoo<T1, T2>
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
               @"Demo.Domain.IFoo<T1, T2> IServiceFactory<Demo.Domain.IFoo<T1, T2>>.CreateOrGetService()
                 {
                     var service = new Demo.Domain.Foo<T1, T2>();
                     return service;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo<int>")]
        public void GenerateServiceFactoryForClassWithSingleGenericInterfaceWithValueTypeParameter()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo<T>
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Foo : IFoo<int>
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
               @"Demo.Domain.IFoo<int> IServiceFactory<Demo.Domain.IFoo<int>>.CreateOrGetService()
                 {
                     var service = new Demo.Domain.Foo();
                     return service;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo<object>")]
        public void GenerateServiceFactoryForClassWithSingleGenericInterfaceWithReferenceTypeParameter()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo<T>
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Foo : IFoo<object>
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
               @"Demo.Domain.IFoo<object> IServiceFactory<Demo.Domain.IFoo<object>>.CreateOrGetService()
                 {
                     var service = new Demo.Domain.Foo();
                     return service;
                 }"));
        }

        [Fact(DisplayName = "Struct : IFoo<T>")]
        public void GenerateServiceFactoryForStructWithSingleGenericInterfaceWithOneGenericParameter()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo<T>
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public struct Foo<T> : IFoo<T>
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
               @"Demo.Domain.IFoo<T> IServiceFactory<Demo.Domain.IFoo<T>>.CreateOrGetService()
                 {
                     var service = new Demo.Domain.Foo<T>();
                     return service;
                 }"));
        }
    }
}
