namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using Extensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Syntax;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="IServiceFactoryGenerator"/> type.
    /// </summary>
    public sealed class IServiceFactoryGeneratorTests
    {
        [Fact]
        public void GenerateIServiceFactoryInterface()
        {
            // Given
            var input = CompilationBuilder.CreateEmptyAssembly();
            var sourceGenerator = new IServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);
            
            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsInterface("IServiceFactory"));
        }
    }
}
