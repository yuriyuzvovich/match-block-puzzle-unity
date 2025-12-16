namespace MatchPuzzle.Infrastructure.Bootstrap
{
    /// <summary>
    /// Optional cleanup contract executed in reverse order after the chain is disposed.
    /// </summary>
    public interface ICleanupStep
    {
        void Cleanup(ServiceContainer services);
    }
}