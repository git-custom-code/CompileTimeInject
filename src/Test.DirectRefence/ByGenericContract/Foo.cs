namespace Test.DirectReference.ByGenericContract
{
    using CustomCode.CompileTimeInject;

    [Export]
    public class Foo : IFoo<int[,]>
    { }
}
