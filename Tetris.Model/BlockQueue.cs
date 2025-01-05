using Tetris.Model.Persistence;

namespace Tetris.Model;


/// <summary>
/// Manages the next random blocks in an order
/// </summary>
public class BlockQueue
{
    private readonly Block[] _blocks;

    private readonly Random _random = new();

    public Block NextBlock { get; private set; }

    public BlockQueue(GameGrid grid)
    {
        _blocks = new Block[]
        {
            new IBlock(grid),
            new JBlock(grid),
            new LBlock(grid),
            new OBlock(grid),
            new SBlock(grid),
            new TBlock(grid),
            new ZBlock(grid)
        };
        NextBlock = RandomBlock();
    }

    private Block RandomBlock()
    {
        return _blocks[_random.Next(_blocks.Length)];
    }

    /// <summary>
    /// Gets the current random block and updates the next
    /// </summary>
    public Block GetAndUpdate()
    {
        Block current = NextBlock;

        NextBlock = RandomBlock();
        while (current.Id == NextBlock.Id)
        {
            NextBlock = RandomBlock();
        }

        return current;
    }
}