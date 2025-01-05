using Tetris.Model.Persistence;

namespace Tetris.Model;

/// <summary>
/// Abstract class of the blocks containing their basic data and methods
/// </summary>
public abstract class Block
{
    protected abstract Position[][] Tiles { get; }
    protected abstract Position StartOffset { get; }
    protected abstract Position SmallStartOffset { get; }
    

    //To identify each type of block 1=
    public abstract int Id { get; }

    private int _rotationState;
    private Position _offset;

    public Block(GameGrid grid)
    {
        if (grid.IsSmall())
        {
            _offset = new Position(SmallStartOffset.Row, SmallStartOffset.Column);
        }
        else
        {
            _offset = new Position(StartOffset.Row, StartOffset.Column);
        }
    }

    //Grid positions occupied by the block
    //loops over the tile positions in the specific state
    //and returns the position one by one adding the offsets too
    public IEnumerable<Position> TilePositions()
    {
        foreach (Position position in Tiles[_rotationState])
        {
            yield return new Position(position.Row + _offset.Row, position.Column + _offset.Column);
        }
    }

    //Rotate cw by incrementing rotation state
    public void RotateCW()
    {
        _rotationState = (_rotationState + 1) % Tiles.Length;
    }

    public void RotateCCW()
    {
        if (_rotationState == 0)
        {
            _rotationState = Tiles.Length - 1;
        }
        else
        {
            _rotationState--;
        }
    }

    //Move the block by the given number of rows and columns
    public void Move(int rows, int columns)
    {
        _offset.Row += rows;
        _offset.Column += columns;
    }

    //Resets the position and rotation of the block
    public void Reset(GameGrid grid)
    {
        _rotationState = 0;
        if (grid.IsSmall())
        {
            _offset.Row = SmallStartOffset.Row;
            _offset.Column = SmallStartOffset.Column;
        }
        else
        {
            _offset.Row = StartOffset.Row;
            _offset.Column = StartOffset.Column;
        }
    }
}