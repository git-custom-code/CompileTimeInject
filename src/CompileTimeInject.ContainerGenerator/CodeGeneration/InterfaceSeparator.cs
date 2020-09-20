namespace CustomCode.CompileTimeInject.ContainerGenerator.CodeGeneration
{
    /// <summary>
    /// Small helper class that will return the correct separator for interface implementation
    /// on a class (':' for the first and ',' for all other interface implementations).
    /// </summary>
    public sealed class InterfaceSeparator
    {
        #region Data

        /// <summary>
        /// Tracks the number of times the <see cref="ToString"/> method was called.
        /// </summary>
        private uint CallCount { get; set; } = 0u;

        #endregion

        #region Logic

        /// <inheritdoc />
        public override string ToString()
        {
            ++CallCount;
            if (CallCount == 1)
            {
                return ":";
            }
            return ",";
        }

        #endregion
    }
}
