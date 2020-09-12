namespace Test.DirectReference.AsSingleton
{
    using CustomCode.CompileTimeInject;
    using System;

    [Export(Lifetime.Singleton)]
    public class Foo : IFoo
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
}
