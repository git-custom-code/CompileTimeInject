namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using DirectReference.Test;
    using GeneratedCode;
    using Xunit;

    public sealed class IocContainerTests
    {
        [Fact]
        public void GetServiceByImplementation()
        {
            // Given
            var container = new IocContainer();

            // When
            var foo = container.GetService<Foo>();

            // Then
            Assert.NotNull(foo);
        }
    }
}
