namespace Tetris;

/// <summary>
/// Renders the playfield, active piece, ghost landing preview, and HUD sidebar to the console.
/// </summary>
public class ConsoleRenderer
{
    public void Initialize()
    {
        Console.CursorVisible = false;
        Console.Clear();
    }

    /// <summary>
    /// Draws a complete frame in-place to avoid terminal flicker.
    /// </summary>
    public void Render(
        Board board,
        Tetromino currentPiece,
        Tetromino nextPiece,
        int ghostY,
        int score,
        int highScore,
        int level,
        int lines,
        bool isPaused,
        bool isGameOver,
        bool isNewRecord)
    {
        try
        {
            // Overwrite the previous frame from (0, 0) instead of calling Console.Clear(), preventing flicker
            Console.SetCursorPosition(0, 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Skip frame if the user resizes the terminal window below minimum dimensions
            return;
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("╔════════════════════╗   CONSOLE TETRIS");

        for (int y = 0; y < Board.Height; y++)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("║");

            for (int x = 0; x < Board.Width; x++)
            {
                DrawBoardCell(board, currentPiece, ghostY, x, y);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("║   ");
            DrawSidebarRow(y, nextPiece, score, highScore, level, lines);
            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("╚════════════════════╝");

        DrawStatusLine(isPaused, isGameOver, isNewRecord);
    }

    private static void DrawBoardCell(Board board, Tetromino active, int ghostY, int x, int y)
    {
        // Render priority: 1. Active falling piece -> 2. Ghost preview -> 3. Locked blocks -> 4. Empty dot
        if (IsPartOfPiece(active, active.X, active.Y, x, y))
        {
            Console.ForegroundColor = GetColor(active.ColorId);
            Console.Write("██");
            return;
        }

        if (IsPartOfPiece(active, active.X, ghostY, x, y))
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("░░");
            return;
        }

        int cell = board.GetCell(x, y);
        if (cell != 0)
        {
            Console.ForegroundColor = GetColor(cell);
            Console.Write("██");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("· ");
        }
    }

    private static bool IsPartOfPiece(Tetromino piece, int originX, int originY, int targetX, int targetY)
    {
        int size = piece.Matrix.GetLength(0);
        int localX = targetX - originX;
        int localY = targetY - originY;

        if (localX < 0 || localX >= size || localY < 0 || localY >= size)
            return false;

        return piece.Matrix[localY, localX] != 0;
    }

    private static void DrawSidebarRow(int row, Tetromino nextPiece, int score, int highScore, int level, int lines)
    {
        Console.ForegroundColor = ConsoleColor.Gray;

        switch (row)
        {
            case 0:
                Console.Write($"SCORE: {score,-12}");
                break;
            case 1:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"BEST:  {highScore,-12}");
                break;
            case 3:
                Console.Write($"LEVEL: {level,-12}");
                break;
            case 4:
                Console.Write($"LINES: {lines,-12}");
                break;
            case 6:
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("NEXT PIECE:        ");
                break;
            case 7:
            case 8:
            case 9:
            case 10:
                DrawNextPiecePreviewRow(nextPiece, row - 7);
                break;
            case 12:
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("CONTROLS:          ");
                break;
            case 13:
                Console.Write("←/→ or A/D : Move  ");
                break;
            case 14:
                Console.Write("↑/W or Z   : Rotate");
                break;
            case 15:
                Console.Write("↓ or S     : Soft  ");
                break;
            case 16:
                Console.Write("Space      : Drop  ");
                break;
            case 17:
                Console.Write("P          : Pause ");
                break;
            case 18:
                Console.Write("Esc        : Quit  ");
                break;
            default:
                Console.Write(new string(' ', 19));
                break;
        }
    }

    private static void DrawNextPiecePreviewRow(Tetromino nextPiece, int previewRow)
    {
        int size = nextPiece.Matrix.GetLength(0);

        for (int c = 0; c < 4; c++)
        {
            if (previewRow < size && c < size && nextPiece.Matrix[previewRow, c] != 0)
            {
                Console.ForegroundColor = GetColor(nextPiece.ColorId);
                Console.Write("██");
            }
            else
            {
                Console.Write("  ");
            }
        }

        Console.Write(new string(' ', 11));
    }

    private static void DrawStatusLine(bool isPaused, bool isGameOver, bool isNewRecord)
    {
        // Trailing spaces ensure leftover characters from longer status messages are overwritten
        if (isGameOver)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            string recordMsg = isNewRecord ? " NEW HIGH SCORE!" : "";
            Console.WriteLine($"GAME OVER!{recordMsg} Press [R] to restart or [Esc] to quit.   ");
        }
        else if (isPaused)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("PAUSED - Press [P] to resume.                                     ");
        }
        else
        {
            Console.WriteLine("                                                                  ");
        }

        Console.ResetColor();
    }

    private static ConsoleColor GetColor(int colorId) => colorId switch
    {
        1 => ConsoleColor.Cyan,
        2 => ConsoleColor.Yellow,
        3 => ConsoleColor.Magenta,
        4 => ConsoleColor.Green,
        5 => ConsoleColor.Red,
        6 => ConsoleColor.Blue,
        7 => ConsoleColor.DarkYellow,
        _ => ConsoleColor.White
    };
}