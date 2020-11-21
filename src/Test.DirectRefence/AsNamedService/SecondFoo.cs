namespace Test.DirectReference.AsNamedService
{
    using CustomCode.CompileTimeInject.Annotations;

    [Export(typeof(IFoo), ServiceId = "SecondFoo")]
    public class SecondFoo : IFoo
    {
        public string Id { get; } = "Second";
    }
}
