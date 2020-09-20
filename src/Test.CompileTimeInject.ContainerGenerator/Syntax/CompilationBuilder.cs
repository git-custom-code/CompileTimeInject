namespace CustomCode.CompileTimeInject.ContainerGenerator.Syntax
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// A set of helper methods for easier creation of in-memory <see cref="Compilation"/>s.
    /// </summary>
    public static class CompilationBuilder
    {
        #region Logic

        /// <summary>
        /// Creates a new <see cref="Compilation"/> that represents an empty c# assembly.
        /// </summary>
        /// <returns> The created empty assembly. </returns>
        public static Compilation CreateEmptyAssembly()
        {
            var emptyAssembly = CSharpCompilation.Create("TestAssembly");
            return emptyAssembly;
        }

        #endregion
    }
}
