namespace CustomCode.CompileTimeInject.ContainerGenerator.Metadata
{
    using Microsoft.CodeAnalysis;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata;

    /// <summary>
    /// Extension methods for the <see cref="Compilation"/> type.
    /// </summary>
    public static class CompilationExtensions
    {
        #region Logic

        /// <summary>
        /// Co-routine that will return a <see cref="MetadataReader"/> for each referenced .Net assembly.
        /// </summary>
        /// <param name="compilation"> The extended <see cref="Compilation"/>. </param>
        /// <returns> A <see cref="MetadataReader"/> for each referenced .Net assembly. </returns>
        public static IEnumerable<MetadataReader> GetReferencedNetAssemblies(this Compilation compilation)
        {
            foreach (var reference in compilation.References.OfType<PortableExecutableReference>())
            {
                var reader = reference.GetMetadataReader();
                if (reader == null)
                {
                    continue;
                }

                yield return reader;
            }
        }

        #endregion
    }
}
