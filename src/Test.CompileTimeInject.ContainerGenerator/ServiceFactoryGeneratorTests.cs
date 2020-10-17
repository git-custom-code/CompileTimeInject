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
        [Fact(DisplayName = "Generate ServiceFactory")]
        public void GenerateServiceFactoryClass()
        {
            // Given
            var input = CompilationBuilder.CreateEmptyAssembly();
            var sourceGenerator = new ServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);
            
            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsClass("ServiceFactory"));
        }
    }
}
