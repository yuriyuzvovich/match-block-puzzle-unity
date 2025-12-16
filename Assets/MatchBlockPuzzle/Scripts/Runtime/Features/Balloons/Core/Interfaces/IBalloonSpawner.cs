namespace MatchPuzzle.Features.Balloon
{
    /// <summary>
    /// Service for spawning and managing background balloons.
    /// </summary>
    public interface IBalloonSpawner
    {
        void Initialize(BalloonSettings balloonSettings);
        void DoFrameTick();
        void Cleanup();

        /// <summary>
        /// Re-runs spawn setup when level/camera/grid changes so balloons match new bounds.
        /// </summary>
        void RefreshSpawnArea();
    }
}
