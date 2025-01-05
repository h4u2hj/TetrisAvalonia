using Tetris.Model.Persistence;

namespace Tetris.Model;

public class IBlock : Block
{
    private readonly Position[][] _tiles = new Position[][]
    {
        new Position[] { new(1, 0), new(1, 1), new(1,2), new(1, 3)},
        new Position[] {new(0,2), new(1,2), new(2,2), new(3,2)},
        new Position[] {new(2,0), new(2,1), new(2,2), new(2,3)},
        new Position[] {new(0,1), new(1,1), new(2,1), new(3,1)}
    };

    public IBlock(GameGrid grid) : base(grid) { }

    protected override Position[][] Tiles => _tiles;

    public override int Id => 1;

    protected override Position StartOffset => new(-1, 3);
    protected override Position SmallStartOffset => new(-1, 0);

}