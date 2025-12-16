using System;

namespace MatchPuzzle.Core.Interfaces
{
    /// <summary>
    /// Abstraction over Unity engine events to broadcast frame updates.
    /// </summary>
    public interface IUnityEventsService
    {
        event Action<float> OnUpdate;

        void PublishUpdate(float deltaTime);
    }
}
