namespace Test.DirectReference.ByGenericContract
{
    using CustomCode.CompileTimeInject.Annotations;

    [Export]
    public class Foo : IFoo<int[,]>
    { }
}
