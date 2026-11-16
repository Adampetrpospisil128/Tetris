namespace Tetris;

/// <summary>
/// Represents the seven standard Tetromino shapes and their color identifiers.
/// </summary>
public enum TetrominoType
{
    I = 1,
    O = 2,
    T = 3,
    S = 4,
    Z = 5,
    J = 6,
    L = 7
}

/// <summary>
/// Models an active falling Tetromino piece, including its 2D shape matrix and board coordinates.
/// </summary>
public class Tetromino
{
    public TetrominoType Type { get; }
    public int ColorId => (int)Type;
    public int[,] Matrix { get; private set; }
    public int X { get; set; }
    public int Y { get; set; }

    public Tetromino(TetrominoType type, int spawnX, int spawnY)
    {
        Type = type;
        Matrix = GetInitialMatrix(type);
        X = spawnX;
        Y = spawnY;
    }

    /// <summary>
    /// Computes a new 90-degree rotated copy of the current piece matrix without mutating state.
    /// </summary>
    public int[,] GetRotatedMatrix(bool clockwise = true)
    {
        int size = Matrix.GetLength(0);
        int[,] rotated = new int[size, size];

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                // Rotate 90 degrees by transposing indices and reversing rows (CW) or columns (CCW)
                if (clockwise)
                    rotated[c, size - 1 - r] = Matrix[r, c];
                else
                    rotated[size - 1 - c, r] = Matrix[r, c];
            }
        }

        return rotated;
    }

    /// <summary>
    /// Commits a validated rotation matrix and applies any wall-kick coordinate offset.
    /// </summary>
    public void ApplyRotation(int[,] newMatrix, int kickX, int kickY)
    {
        Matrix = newMatrix;
        X += kickX;
        Y += kickY;
    }

    private static int[,] GetInitialMatrix(TetrominoType type) => type switch
    {
        TetrominoType.I => new[,]
        {
            { 0, 0, 0, 0 },
            { 1, 1, 1, 1 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        },
        TetrominoType.O => new[,]
        {
            { 2, 2 },
            { 2, 2 }
        },
        TetrominoType.T => new[,]
        {
            { 0, 3, 0 },
            { 3, 3, 3 },
            { 0, 0, 0 }
        },
        TetrominoType.S => new[,]
        {
            { 0, 4, 4 },
            { 4, 4, 0 },
            { 0, 0, 0 }
        },
        TetrominoType.Z => new[,]
        {
            { 5, 5, 0 },
            { 0, 5, 5 },
            { 0, 0, 0 }
        },
        TetrominoType.J => new[,]
        {
            { 6, 0, 0 },
            { 6, 6, 6 },
            { 0, 0, 0 }
        },
        TetrominoType.L => new[,]
        {
            { 0, 0, 7 },
            { 7, 7, 7 },
            { 0, 0, 0 }
        },
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}