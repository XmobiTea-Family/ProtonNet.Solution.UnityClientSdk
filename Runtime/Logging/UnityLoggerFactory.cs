namespace XmobiTea.Logging.Unity
{
    /// <summary>
    /// Factory class to create instances of <see cref="UnityLogger"/>.
    /// This class is a singleton, with the single instance accessible via the <see cref="Instance"/> property.
    /// </summary>
    public sealed class UnityLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="UnityLoggerFactory"/>.
        /// </summary>
        public static readonly UnityLoggerFactory Instance = new UnityLoggerFactory();

        /// <summary>
        /// Creates a new instance of <see cref="UnityLogger"/> with the specified name.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <returns>A new instance of <see cref="UnityLogger"/>.</returns>
        public ILogger CreateLogger(string name) => new UnityLogger(name);

        /// <summary>
        /// Prevents a default instance of the <see cref="UnityLoggerFactory"/> class from being created.
        /// This constructor is private to enforce the singleton pattern.
        /// </summary>
        private UnityLoggerFactory() { }

    }

}
