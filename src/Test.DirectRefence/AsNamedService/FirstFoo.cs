namespace Test.DirectReference.AsNamedService
{
    using CustomCode.CompileTimeInject.Annotations;

    [Export(typeof(IFoo), ServiceId = "FirstFooId")]
    public class FirstFoo : IFoo
    {
        public string Id { get; } = "1";
    }
}
