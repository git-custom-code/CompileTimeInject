namespace Test.DirectReference.AsNamedService
{
    using CustomCode.CompileTimeInject.Annotations;

    [Export]
    public sealed class Bar : IBar
    {
        public Bar([Import("SecondFooId")] IFoo foo)
        {
            Foo = foo;
        }

        public IFoo Foo { get; }
    }
}
