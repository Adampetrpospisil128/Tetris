namespace Tetris;

/// <summary>
/// Manages the 10x20 playfield grid, collision checks, piece locking, and line clearing.
/// </summary>
public class Board
{
    public const int Width = 10;
    public const int Height = 20;

    // 0 represents an empty cell; values 1-7 store the ColorId of locked blocks
    private readonly int[,] _grid = new int[Height, Width];

    public int GetCell(int x, int y) => _grid[y, x];

    public void Reset() => Array.Clear(_grid, 0, _grid.Length);

    /// <summary>
    /// Checks whether a piece (or candidate rotated matrix) fits within bounds and does not overlap locked blocks.
    /// </summary>
    public bool IsValidPosition(Tetromino piece, int offsetX = 0, int offsetY = 0, int[,]? overrideMatrix = null)
    {
        int[,] matrix = overrideMatrix ?? piece.Matrix;
        int size = matrix.GetLength(0);

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                if (matrix[r, c] == 0)
                    continue;

                int boardX = piece.X + c + offsetX;
                int boardY = piece.Y + r + offsetY;

                // Enforce left, right, and floor boundaries
                if (boardX < 0 || boardX >= Width || boardY >= Height)
                    return false;

                // Allow blocks above the top edge (boardY < 0) during spawn, otherwise check grid occupancy
                if (boardY >= 0 && _grid[boardY, boardX] != 0)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Permanently writes the active piece's blocks into the board grid upon landing.
    /// </summary>
    public void LockPiece(Tetromino piece)
    {
        int size = piece.Matrix.GetLength(0);

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                if (piece.Matrix[r, c] == 0)
                    continue;

                int boardX = piece.X + c;
                int boardY = piece.Y + r;

                if (boardY >= 0 && boardY < Height && boardX >= 0 && boardX < Width)
                    _grid[boardY, boardX] = piece.ColorId;
            }
        }
    }

    /// <summary>
    /// Scans the board from bottom to top, removes completed rows, and returns the total cleared count.
    /// </summary>
    public int ClearFullLines()
    {
        int linesCleared = 0;

        for (int y = Height - 1; y >= 0; y--)
        {
            if (!IsRowFull(y))
                continue;

            linesCleared++;
            ShiftRowsDown(y);

            // Re-evaluate the same row index because upper rows just shifted down into it
            y++;
        }

        return linesCleared;
    }

    /// <summary>
    /// Projects the active piece downward to find the lowest valid Y coordinate for the ghost preview.
    /// </summary>
    public int GetGhostY(Tetromino piece)
    {
        int ghostY = piece.Y;
        while (IsValidPosition(piece, 0, (ghostY - piece.Y) + 1))
        {
            ghostY++;
        }
        return ghostY;
    }

    private bool IsRowFull(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            if (_grid[y, x] == 0)
                return false;
        }
        return true;
    }

    private void ShiftRowsDown(int clearedRow)
    {
        for (int y = clearedRow; y > 0; y--)
        {
            for (int x = 0; x < Width; x++)
            {
                _grid[y, x] = _grid[y - 1, x];
            }
        }

        for (int x = 0; x < Width; x++)
        {
            _grid[0, x] = 0;
        }
    }
}