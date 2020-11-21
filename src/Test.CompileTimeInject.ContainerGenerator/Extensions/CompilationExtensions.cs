namespace CustomCode.CompileTimeInject.ContainerGenerator.Extensions
{
    using Microsoft.CodeAnalysis;
    using Syntax;
    using System;
    using System.Linq;

    /// <summary>
    /// Extension methods for the <see cref="Compilation"/> type.
    /// </summary>
    public static class CompilationExtensions
    {
        #region Logic

        /// <summary>
        /// Query if the extended <paramref name="compilation"/> contains a class with the
        /// given <paramref name="className"/>.
        /// </summary>
        /// <param name="compilation"> The extended <see cref="Compilation"/>. </param>
        /// <param name="className"> The name of the class to be found. </param>
        /// <returns>
        /// True if a class with the given <paramref name="className"/> was found, false otherwise.
        /// </returns>
        public static bool ContainsClass(this Compilation compilation, string className)
        {
            var classWalker = new ClassWalker();
            foreach (var tree in compilation.SyntaxTrees)
            {
                classWalker.Visit(tree.GetRoot());
                if (classWalker.FoundClasses.Any(@class => string.Equals(
                    @class, className, System.StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Query if the extended <paramref name="compilation"/> contains an interface with the
        /// given <paramref name="interfaceName"/>.
        /// </summary>
        /// <param name="compilation"> The extended <see cref="Compilation"/>. </param>
        /// <param name="interfaceName"> The name of the interface to be found. </param>
        /// <returns>
        /// True if an interface with the given <paramref name="interfaceName"/> was found, false otherwise.
        /// </returns>
        public static bool ContainsInterface(this Compilation compilation, string interfaceName)
        {
            var interfaceWalker = new InterfaceWalker();
            foreach (var tree in compilation.SyntaxTrees)
            {
                interfaceWalker.Visit(tree.GetRoot());
                if (interfaceWalker.FoundInterfaces.Any(name => string.Equals(
                    name, interfaceName, System.StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Query if the extended <paramref name="compilation"/> contains a type (with the given
        /// <paramref name="typeName"/>) that contains the given <paramref name="methodImplementation"/>.
        /// </summary>
        /// <param name="compilation"> The extended <see cref="Compilation"/>. </param>
        /// <param name="typeName"> The name of the type to be found. </param>
        /// <param name="methodImplementation"> The method implementation to be found. </param>
        /// <returns>
        /// True if a type with the given <paramref name="typeName"/> and <paramref name="methodImplementation"/>
        /// was found, false otherwise.
        /// </returns>
        public static bool ContainsTypeWithMethodImplementation(
            this Compilation compilation, string typeName, string methodImplementation)
        {
            var code = string.Join(
                Environment.NewLine,
                methodImplementation.Split(Environment.NewLine).Select(s => s.Trim()));

            var typeMethodWalker = new TypeMethodWalker();
            foreach (var tree in compilation.SyntaxTrees)
            {
                typeMethodWalker.Visit(tree.GetRoot());
                if (typeMethodWalker.FoundMethodsByType.TryGetValue(typeName, out var methodImplementations))
                {
                    if (methodImplementations.Any(m => m.Equals(code, StringComparison.OrdinalIgnoreCase)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Query if the extended <paramref name="compilation"/> contains a type (with the given
        /// <paramref name="typeName"/>) that contains a method with the given <paramref name="methodSignature"/>.
        /// </summary>
        /// <param name="compilation"> The extended <see cref="Compilation"/>. </param>
        /// <param name="typeName"> The name of the type to be found. </param>
        /// <param name="methodSignature"> The method signature to be found. </param>
        /// <returns>
        /// True if a type with the given <paramref name="typeName"/> and <paramref name="methodSignature"/>
        /// was found, false otherwise.
        /// </returns>
        public static bool ContainsTypeWithMethodSignature(
            this Compilation compilation, string typeName, string methodSignature)
        {
            var code = string.Join(" ", methodSignature.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            var typeMethodSignatureWalker = new TypeMethodSignatureWalker();
            foreach (var tree in compilation.SyntaxTrees)
            {
                typeMethodSignatureWalker.Visit(tree.GetRoot());
                if (typeMethodSignatureWalker.FoundMethodSignaturesByType.TryGetValue(typeName, out var methodSignatures))
                {
                    if (methodSignatures.Any(m => m.Equals(code, StringComparison.OrdinalIgnoreCase)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
