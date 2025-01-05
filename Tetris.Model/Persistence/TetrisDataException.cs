namespace Tetris.Model.Persistence;

public class TetrisDataException : Exception
{
    public TetrisDataException() { }
    public TetrisDataException(string message) : base(message) { }
}