namespace Test.DirectReference.ByFilteredContract
{
    using CustomCode.CompileTimeInject.Annotations;

    [Export(typeof(IBar))]
    public class Foo : IFoo, IBar
    { }
}
