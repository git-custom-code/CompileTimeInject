namespace CustomCode.CompileTimeInject.Annotations
{
    using System;

    /// <summary>
    /// Use this attribute for constructor parameters to import a named service from the "IocContainer".
    /// </summary>
    /// <example>
    /// 
    /// - If the same contract is implemented by more than one named service, you can use the import attribute
    ///   to inject a specific service by unique ServiceId:
    /// <![CDATA[
    /// [Export(ServiceId = "FirstBarId")]
    /// public sealed class FirstBar : IBar
    /// { }
    /// 
    /// [Export(ServiceId = "SecondBarId")]
    /// public sealed class SecondBar : IBar
    /// { }
    /// 
    /// [Export]
    /// public sealed class Foo
    /// {
    ///   public Foo([Import("FirstBarId")] IBar bar)
    ///   { }
    /// }
    /// ]]>
    /// </example>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ImportAttribute : Attribute
    {
        #region Dependencies

        /// <summary>
        /// Standard ctor.
        /// </summary>
        /// <param name="serviceId"> The unique identifier for the named service to be injected. </param>
        public ImportAttribute(string serviceId)
        {
            ServiceId = serviceId;
        }

        #endregion

        #region Data

        /// <summary>
        ///  Gets the unique identifier for the named service to be injected.
        /// </summary>
        public string ServiceId { get; }

        #endregion
    }
}
