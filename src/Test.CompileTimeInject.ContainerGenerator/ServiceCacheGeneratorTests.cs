namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using Extensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Syntax;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="ServiceCacheGenerator"/> type.
    /// </summary>
    public sealed class ServiceCacheGeneratorTests
    {
        [Fact(DisplayName = "ServiceCacheGenerator: default")]
        public void GenerateDefaultServiceCache()
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
            var sourceGenerator = new ServiceCacheGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsClass("ServiceCache"));
            Assert.False(output.ContainsTypeWithMethodSignature(
                "ServiceCache",
                "public object GetOrAdd(Type key, string serviceId, Func<string, object> valueFactory)"));
        }

        [Fact(DisplayName = "ServiceCacheGenerator: named services")]
        public void GenerateServiceCacheForNamedServices()
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
            var sourceGenerator = new ServiceCacheGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsClass("ServiceCache"));
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "ServiceCache",
               @"public object GetOrAdd(Type key, string serviceId, Func<string, object> valueFactory)
                 {
                     var cache = NamedServiceCache.GetOrAdd(key, new ConcurrentDictionary<string, object>());
                     return cache.GetOrAdd(serviceId, valueFactory);
                 }"));
        }
    }
}
