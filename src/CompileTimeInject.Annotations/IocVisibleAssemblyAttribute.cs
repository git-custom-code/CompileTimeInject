namespace CustomCode.CompileTimeInject.Annotations
{
    using System;

    /// <summary>
    /// Use this attribute at the assembly level in order to tell the source generator
    /// to  generate code for all types that are annotated with an <see cref="ExportAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class IocVisibleAssemblyAttribute : Attribute
    { }
}