using System.IO;
using Tetris.Model.Persistence;

namespace Tetris.Model;

public enum GameDifficulty
{
    Easy,
    Medium,
    Hard
}

/// <summary>
/// Handles the data of the game flow
/// </summary>
public class GameState
{
    //Current block in the game
    private Block? _currentBlock;

    public Block CurrentBlock
    {
        get => _currentBlock ?? throw new TetrisDataException("Currentblock is null");
        private set
        {
            _currentBlock = value;
            _currentBlock.Reset(GameGrid);

            _currentBlock.Move(1, 0);
            if (!BlockFits())
            {
                _currentBlock.Move(-1, 0);
            }
        }
    }

    public GameDifficulty GameDifficulty { get; private set; }

    private ITetrisDataAccess _dataAccess;

    public GameGrid GameGrid { get; }

    public BlockQueue BlockQueue { get; }

    public bool GameOver { get; private set; }

    public int Score { get; set; }

    public event EventHandler ScoreChanged = null!;

    //Initialize the game with the default grid size(largest)
    public GameState(ITetrisDataAccess dataAccess)
    {
        GameDifficulty = GameDifficulty.Hard;
        Score = 0;
        GameGrid = new GameGrid(18, 12);
        BlockQueue = new BlockQueue(GameGrid);
        CurrentBlock = BlockQueue.GetAndUpdate();
        _dataAccess = dataAccess;
    }

    //Initialize the game with given grid size
    public GameState(int row, int col, ITetrisDataAccess dataAccess)
    {
        Score = 0;
        GameGrid = new GameGrid(row, col);
        BlockQueue = new BlockQueue(GameGrid);
        CurrentBlock = BlockQueue.GetAndUpdate();
        if (GameGrid.IsSmall())
        {
            GameDifficulty = GameDifficulty.Easy;
        }
        else
        {
            GameDifficulty = GameDifficulty.Medium;
        }

        _dataAccess = dataAccess;
    }

    public GameState(ITetrisDataAccess dataAccess, Stream fileName)
    {
        if (dataAccess == null)
        {
            throw new InvalidOperationException("No data access is provided.");
        }

        MemoryStream memoryStream1 = new MemoryStream();
        fileName.CopyTo(memoryStream1);
        memoryStream1.Position = 0;
        
        MemoryStream memoryStream2 = new MemoryStream(memoryStream1.ToArray());
        memoryStream1.Position = 0;
        GameGrid = dataAccess.Load(memoryStream1);
        Score = dataAccess.LoadScore(memoryStream2);
        BlockQueue = new BlockQueue(GameGrid);
        CurrentBlock = BlockQueue.GetAndUpdate();
        if (GameGrid.IsSmall())
        {
            GameDifficulty = GameDifficulty.Easy;
        }
        else if (GameGrid.Columns < 12)
        {
            GameDifficulty = GameDifficulty.Medium;
        }
        else
        {
            GameDifficulty = GameDifficulty.Hard;
        }

        _dataAccess = dataAccess;
    }


    public void SaveGame(Stream path)
    {
        if (_dataAccess == null)
        {
            throw new InvalidOperationException("No data access is provided.");
        }

        try
        {
            _dataAccess.Save(path, GameGrid, Score);
        }
        catch (TetrisDataException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    //Check if a block is in a legal position by checking if its positions are overlapping another block or are outside
    private bool BlockFits()
    {
        foreach (var p in CurrentBlock.TilePositions())
        {
            if (!GameGrid.IsEmpty(p.Row, p.Column))
            {
                return false;
            }
        }

        return true;
    }

    //Checks if the game is over by checking the 2 extra rows
    private bool IsGameOver()
    {
        return !(GameGrid.IsRowEmpty(0) && GameGrid.IsRowEmpty(1));
    }

    //Place down a block when  it cant move down anymore
    private void PlaceBlock()
    {
        foreach (var p in CurrentBlock.TilePositions())
        {
            GameGrid[p.Row, p.Column] = CurrentBlock.Id;
        }

        Score += GameGrid.ClearFullRows() * 100;
        OnScoreChanged();

        if (IsGameOver())
        {
            GameOver = true;
        }
        else
        {
            CurrentBlock = BlockQueue.GetAndUpdate();
        }
    }

    //Rotate a block if it doesn't end up in an illegal state
    public void RotateBlockCW()
    {
        CurrentBlock.RotateCW();
        if (!BlockFits())
        {
            CurrentBlock.RotateCCW();
        }
    }

    public void RotateBlockCCW()
    {
        CurrentBlock.RotateCCW();
        if (!BlockFits())
        {
            CurrentBlock.RotateCW();
        }
    }

    //Move a block if it doesn't end up in an illegal state
    public void MoveBlockLeft()
    {
        CurrentBlock.Move(0, -1);
        if (!BlockFits())
        {
            CurrentBlock.Move(0, 1);
        }
    }

    public void MoveBlockRight()
    {
        CurrentBlock.Move(0, 1);
        if (!BlockFits())
        {
            CurrentBlock.Move(0, -1);
        }
    }

    //Move a block down one row
    public void MoveBlockDown()
    {
        CurrentBlock.Move(1, 0);
        if (!BlockFits())
        {
            CurrentBlock.Move(-1, 0);
            PlaceBlock();
        }
    }

    //Empty cells under the block cell
    private int TileDropDistance(Position p)
    {
        int drop = 0;
        while (GameGrid.IsEmpty(p.Row + drop + 1, p.Column))
        {
            drop++;
        }

        return drop;
    }

    //Number of empty cells under the lowest block cell. This is how much we need to drop
    private int BlockDropDistance()
    {
        int drop = GameGrid.Rows;

        foreach (Position p in CurrentBlock.TilePositions())
        {
            drop = Math.Min(drop, TileDropDistance(p));
        }

        return drop;
    }

    public void DropBlock()
    {
        CurrentBlock.Move(BlockDropDistance(), 0);
        PlaceBlock();
    }

    private void OnScoreChanged()
    {
        ScoreChanged?.Invoke(this, EventArgs.Empty);
    }
}