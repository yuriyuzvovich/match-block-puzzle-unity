namespace MatchPuzzle.Features.Background
{
    public sealed class NullBackgroundPresenter : IBackgroundPresenter
    {
        public static NullBackgroundPresenter Instance { get; } = new NullBackgroundPresenter();

        private NullBackgroundPresenter()
        {
        }

        public void Initialize()
        {
        }

        public void AdjustBackgroundSize()
        {
        }
    }
}
