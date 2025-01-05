namespace Tetris.Model.Persistence;

public class TetrisDataAccess : ITetrisDataAccess
{
    public void Save(Stream path, GameGrid grid, int score)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine(score);
                for (int r = 0; r < grid.Rows; r++)
                {
                    for (int c = 0; c < grid.Columns; c++)
                    {
                        writer.Write(grid[r, c] + " ");
                    }
                }
            }
        }
        catch (Exception e)
        {
            throw new TetrisDataException(e.Message);
        }
    }

    public int LoadScore(Stream path)
    {
        int score = 0;
        try
        {
            using (StreamReader reader = new StreamReader(path))
            {
                score = Convert.ToInt32(reader.ReadLine());
            }
        }
        catch (FormatException e)
        {
            throw new TetrisDataException("Failed to convert score " + e.Message);
        }
        catch (ArgumentException e)
        {
            throw new TetrisDataException("Failed to load file to read score " + e.Message);
        }

        return score;
    }


    public GameGrid Load(Stream path)
    {
        GameGrid grid;
        try
        {
            using (StreamReader reader = new StreamReader(path))
            {
                reader.ReadLine();

                string line = reader.ReadLine() ?? string.Empty;
                string[] cellValues = line.Split(" ");
                if (line.Length == 144)
                {
                    grid = new GameGrid(18, 4);
                }
                else if (line.Length == 288)
                {
                    grid = new GameGrid(18, 8);
                }
                else if (line.Length == 432)
                {
                    grid = new GameGrid(18, 12);
                }
                else
                {
                    throw new TetrisDataException("Error in file length");
                }

                int i = 0;
                for (int r = 0; r < grid.Rows; r++)
                {
                    for (int c = 0; c < grid.Columns; c++)
                    {
                        grid[r, c] = Convert.ToInt32(cellValues[i]);
                        i++;
                    }
                }

                return grid;
            }
        }
        catch (Exception e)
        {
            throw new TetrisDataException("Failed to load grid " + e.Message);
        }
    }
}