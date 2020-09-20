namespace CustomCode.CompileTimeInject.ContainerGenerator.Syntax
{
    using CustomCode.CompileTimeInject.Annotations;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using System.Collections.Generic;
    using System.Reflection;

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

        /// <summary>
        /// Creates a new <see cref="Compilation"/> that represents a c# assembly with the
        /// given <paramref name="sourceCodeFiles"/>.
        /// </summary>
        /// <param name="sourceCodeFiles">
        /// A collection of c# source code files that should be included in the created assembly.
        /// </param>
        /// <returns> The created c# assembly. </returns>
        public static Compilation CreateAssemblyWithCode(params string[] sourceCodeFiles)
        {
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var netStandard = MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location);
            var systemRuntime = MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location);
            var annotations = MetadataReference.CreateFromFile(typeof(ExportAttribute).Assembly.Location);

            var syntaxTrees = new List<SyntaxTree>();
            foreach(var sourceCode in sourceCodeFiles)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, new CSharpParseOptions());
                syntaxTrees.Add(syntaxTree);
            }

            var options = new CSharpCompilationOptions(
               OutputKind.DynamicallyLinkedLibrary,
               optimizationLevel: OptimizationLevel.Debug);

            var assembly = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: syntaxTrees,
                references: new[] { mscorlib, netStandard, systemRuntime, annotations },
                options: options);

            return assembly;
        }

        #endregion
    }
}
