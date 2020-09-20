namespace Test.DirectReference.ByMultipleContracts
{
    using CustomCode.CompileTimeInject.Annotations;

    [Export]
    public class Foo : IFoo, IBar
    { }
}
