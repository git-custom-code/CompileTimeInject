namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using Extensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Syntax;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="IocContainerGenerator"/> type.
    /// </summary>
    public sealed class IocContainerGeneratorTests
    {
        [Fact]
        public void GenerateIocContainerClass()
        {
            // Given
            var input = CompilationBuilder.CreateEmptyAssembly();
            var sourceGenerator = new IocContainerGenerator();
            var runtime = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            runtime.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsClass("IocContainer"));
        }
    }
}
