namespace Test.DirectReference.ByMultipleContracts
{
    using CustomCode.CompileTimeInject;

    [Export]
    public class Foo : IFoo, IBar
    { }
}
