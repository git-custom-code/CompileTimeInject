namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using Extensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Syntax;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="ScopeGenerator"/> type.
    /// </summary>
    public sealed class ScopeGeneratorTests
    {
        [Fact(DisplayName = "Scope: generated")]
        public void GenerateScopeOnlyIfServicesWithLifetimeScopedAreDefined()
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

                      [Export(Lifetime.Scoped)]
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
            Assert.True(output.ContainsClass("Scope"));
        }

        [Fact(DisplayName = "Scope.Dispose: generated")]
        public void GenerateScopeDisposeMethod()
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

                      [Export(Lifetime.Scoped)]
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
                "Scope",
               @"public void Dispose()
                 {
                     DisposeAction();
                 }"));
        }

        [Fact(DisplayName = "Scope.GetService: generated")]
        public void GenerateScopeGetServiceMethod()
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

                      [Export(Lifetime.Scoped)]
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
                "Scope",
               @"public T? GetService<T>() where T : class
                 {
                     var factory = Factory as IServiceFactory<T>;
                     return factory?.CreateOrGetService();
                 }"));
        }

        [Fact(DisplayName = "Scope.GetServices: generated")]
        public void GenerateScopeGetServicesMethod()
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

                      [Export(Lifetime.Scoped)]
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
                "Scope",
               @"public IEnumerable<T> GetServices<T>() where T : class
                 {
                     if (Factory is IServiceFactory<IEnumerable<T>> collectionFactory)
                     {
                         return collectionFactory.CreateOrGetService();
                     }

                     if (Factory is IServiceFactory<T> factory)
                     {
                         return new List<T> { factory.CreateOrGetService() };
                     }

                     return Enumerable.Empty<T>();
                 }"));
        }

        [Fact(DisplayName = "Scope: not generated")]
        public void DoNotGenerateScopeIfNoServicesWithLifetimeScopedAreDefined()
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
            Assert.False(output.ContainsClass("Scope"));
        }
    }
}
