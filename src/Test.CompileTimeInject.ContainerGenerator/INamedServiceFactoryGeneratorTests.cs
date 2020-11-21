namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using Extensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Syntax;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="INamedServiceFactoryGenerator"/> type.
    /// </summary>
    public sealed class INamedServiceFactoryGeneratorTests
    {
        [Fact(DisplayName = "INamedServiceFactoryGenerator: generated")]
        public void GenerateINamedServiceFactoryInterfaceWhenNeeded()
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
            var sourceGenerator = new INamedServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsInterface("INamedServiceFactory"));
        }

        [Fact(DisplayName = "INamedServiceFactoryGenerator: not generated")]
        public void DoNotGenerateINamedServiceFactoryInterfaceWhenNotNeeded()
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
            var sourceGenerator = new INamedServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);
            
            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.False(output.ContainsInterface("INamedServiceFactory"));
        }
    }
}
