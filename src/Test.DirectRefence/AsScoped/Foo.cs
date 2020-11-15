namespace Test.DirectReference.AsScoped
{
    using CustomCode.CompileTimeInject.Annotations;
    using System;

    [Export(Lifetime.Scoped)]
    public class Foo : IFoo
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
}
