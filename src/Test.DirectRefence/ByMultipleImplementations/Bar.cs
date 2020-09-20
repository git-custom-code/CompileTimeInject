namespace Test.DirectReference.ByMultipleImplementations
{
    using CustomCode.CompileTimeInject.Annotations;

    [Export]
    public class Bar : IFoo
    { }
}
