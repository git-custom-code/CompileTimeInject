namespace Test.DirectReference.ByFilteredContract
{
    using CustomCode.CompileTimeInject;

    [Export(typeof(IBar))]
    public class Foo : IFoo, IBar
    { }
}
