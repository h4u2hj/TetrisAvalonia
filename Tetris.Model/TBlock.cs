using Tetris.Model.Persistence;

namespace Tetris.Model;

public class TBlock : Block
{
    private readonly Position[][] _tiles = new Position[][]
    {
        new Position[] { new(0, 1), new(1, 0), new(1,1), new(1, 2)},
        new Position[] {new(0,1), new(1,1), new(1,2), new(2,1)},
        new Position[] {new(1,0), new(1,1), new(1,2), new(2,1)},
        new Position[] {new(0,1), new(1,0), new(1,1), new(2,1)}
    };

    public TBlock(GameGrid grid) : base(grid) { }

    protected override Position[][] Tiles => _tiles;

    protected override Position StartOffset => new Position(0, 3);
    protected override Position SmallStartOffset => new(0, 0);

    public override int Id => 6;

}