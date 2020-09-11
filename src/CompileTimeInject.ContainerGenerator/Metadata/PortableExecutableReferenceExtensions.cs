namespace CustomCode.CompileTimeInject.ContainerGenerator.Metadata
{
    using Microsoft.CodeAnalysis;
    using System;
    using System.Reflection.Metadata;

    /// <summary>
    /// Extension methods for the <see cref="PortableExecutableReference"/> type.
    /// </summary>
    public static class PortableExecutableReferenceExtensions
    {
        #region Logic

        /// <summary>
        /// If the extended <paramref name="reference"/> is a .Net Assembly return a <see cref="MetadataReader"/>
        /// instance that can be used to read the assembly's embedded metadata, otherwise return null.
        /// </summary>
        /// <param name="reference"> The extended <see cref="PortableExecutableReference"/>. </param>
        /// <returns> A <see cref="MetadataReader"/> if the reference is a .Net assembly or null otherwise. </returns>
        public static MetadataReader? GetMetadataReader(this PortableExecutableReference reference)
        {
            try
            {
                if (reference.GetMetadata() is AssemblyMetadata assembly)
                {
                    foreach (var module in assembly.GetModules())
                    {
                        return module.GetMetadataReader();
                    }
                }
                else if (reference.GetMetadata() is ModuleMetadata module)
                {
                    return module.GetMetadataReader();
                }

                return null;
            }
            catch (BadImageFormatException) 
            {
                return null;
            }
        }

        #endregion
    }
}
