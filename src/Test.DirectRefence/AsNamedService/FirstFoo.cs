namespace Test.DirectReference.AsNamedService
{
    using CustomCode.CompileTimeInject.Annotations;

    [Export(typeof(IFoo), ServiceId = "FirstFoo")]
    public class FirstFoo : IFoo
    {
        public string Id { get; } = "First";
    }
}
