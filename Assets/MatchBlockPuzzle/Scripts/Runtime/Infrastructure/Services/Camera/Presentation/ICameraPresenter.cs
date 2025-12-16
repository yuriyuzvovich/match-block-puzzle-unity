namespace MatchPuzzle.Infrastructure.Services
{
    public interface ICameraPresenter
    {
        void Initialize();
        void AdjustCameraForGrid(int rows, int columns, float cellSize, float gridOffsetY);
    }
}