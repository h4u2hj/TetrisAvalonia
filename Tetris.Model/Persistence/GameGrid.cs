namespace Tetris.Model.Persistence
{
    /// <summary>
    /// Grid of the Tetris game using integer matrix
    /// </summary>
    public class GameGrid
    {
        private readonly int[,] grid;

        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public int this[int r, int c]
        {
            get => grid[r, c];
            set { grid[r, c] = value; }
        }

        /// <summary>
        /// Instantiate Game grid
        /// </summary>
        /// <param name="rows">Number of rows in the grid</param>
        /// <param name="columns">Number of columns in the grid</param>
        public GameGrid(int rows, int columns)
        {
            if (rows < 0 || columns < 0)
                throw new ArgumentOutOfRangeException(nameof(rows), "The grid row or column is less than zero");

            Rows = rows;
            Columns = columns;
            grid = new int[rows, columns];
        }


        //A given index(cell) is inside the grid or not
        public bool IsInside(int r, int c)
        {
            return r >= 0 && c >= 0 && r < Rows && c < Columns;
        }

        //Checks whether there is a block on this index
        public bool IsEmpty(int r, int c)
        {
            return IsInside(r, c) && grid[r, c] == 0;
        }

        //Checks whether the row is full of blocks
        public bool IsRowFull(int r)
        {
            for (int i = 0; i < Columns; i++)
            {
                if (grid[r, i] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        //Checks whether the row is empty from blocks
        public bool IsRowEmpty(int r)
        {
            for (int i = 0; i < Columns; i++)
            {
                if (grid[r, i] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        //Clear the given row (when row is full)
        public void ClearRow(int row)
        {
            for (int i = 0; i < Columns; i++)
            {
                grid[row, i] = 0;
            }
        }

        //Move a row down after clearing a full one
        public void MoveRowDown(int r, int numToMove)
        {
            for (int i = 0; i < Columns; i++)
            {
                grid[r + numToMove, i] = grid[r, i];
                grid[r, i] = 0;
            }
        }

        //Clear all full rows after a block was placed down
        public int ClearFullRows()
        {
            int cleared = 0;
            for (int i = Rows - 1; i >= 0; i--)
            {
                if (IsRowFull(i))
                {
                    ClearRow(i);
                    cleared++;
                }
                else if (cleared > 0)
                {
                    MoveRowDown(i, cleared);
                }
            }
            return cleared;
        }

        public bool IsSmall()
        {
            return Columns < 6;
        }
    }
}
