namespace Tetris.Model.Persistence;

public interface ITetrisDataAccess
{
    GameGrid Load(Stream path);
    void Save(Stream path, GameGrid grid, int score);
    int LoadScore(Stream fileName);
}