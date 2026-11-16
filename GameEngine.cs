using System.Diagnostics;

namespace Tetris;

/// <summary>
/// Coordinates the real-time game loop, input handling, gravity timing, scoring, and 7-bag piece generation.
/// </summary>
public class GameEngine
{
    private readonly Board _board = new();
    private readonly ConsoleRenderer _renderer = new();
    private readonly ScoreManager _scoreManager = new();
    private readonly Queue<TetrominoType> _bag = new();
    private readonly Random _random = new();

    private Tetromino _currentPiece = null!;
    private Tetromino _nextPiece = null!;

    private int _score;
    private int _lines;
    private int _level = 1;
    private bool _isPaused;
    private bool _isGameOver;
    private bool _isNewRecord;
    private bool _isRunning = true;

    // Candidate horizontal/upward offsets tested when a rotation is blocked near a wall or stack
    private static readonly (int dx, int dy)[] WallKickOffsets =
    {
        (0, 0), (-1, 0), (1, 0), (-2, 0), (2, 0), (0, -1)
    };

    /// <summary>
    /// Starts and runs the main game loop until the player presses Escape.
    /// </summary>
    public void Run()
    {
        _renderer.Initialize();
        StartNewGame();

        var gravityWatch = Stopwatch.StartNew();

        while (_isRunning)
        {
            HandleInput();

            if (!_isPaused && !_isGameOver)
            {
                // Gravity interval decreases by 70ms per level down to a 100ms cap
                int gravityDelayMs = Math.Max(100, 800 - (_level - 1) * 70);
                if (gravityWatch.ElapsedMilliseconds >= gravityDelayMs)
                {
                    StepDown();
                    gravityWatch.Restart();
                }
            }

            int ghostY = _board.GetGhostY(_currentPiece);
            _renderer.Render(
                _board,
                _currentPiece,
                _nextPiece,
                ghostY,
                _score,
                Math.Max(_score, _scoreManager.BestRecord.HighScore),
                _level,
                _lines,
                _isPaused,
                _isGameOver,
                _isNewRecord);

            // Cap loop at ~60 FPS so input stays responsive without maxing out a CPU core
            Thread.Sleep(16);
        }

        Console.CursorVisible = true;
    }

    private void StartNewGame()
    {
        _board.Reset();
        _bag.Clear();
        _score = 0;
        _lines = 0;
        _level = 1;
        _isPaused = false;
        _isGameOver = false;
        _isNewRecord = false;

        _currentPiece = SpawnFromBag();
        _nextPiece = SpawnFromBag();
    }

    private void HandleInput()
    {
        while (Console.KeyAvailable)
        {
            ConsoleKey key = Console.ReadKey(intercept: true).Key;

            if (key == ConsoleKey.Escape)
            {
                _isRunning = false;
                return;
            }

            if (_isGameOver)
            {
                if (key == ConsoleKey.R)
                    StartNewGame();
                continue;
            }

            if (key == ConsoleKey.P)
            {
                _isPaused = !_isPaused;
                continue;
            }

            if (_isPaused)
                continue;

            switch (key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.A:
                    TryMove(-1, 0);
                    break;

                case ConsoleKey.RightArrow:
                case ConsoleKey.D:
                    TryMove(1, 0);
                    break;

                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    if (TryMove(0, 1))
                        _score += 1;
                    break;

                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                case ConsoleKey.X:
                    TryRotate(clockwise: true);
                    break;

                case ConsoleKey.Z:
                    TryRotate(clockwise: false);
                    break;

                case ConsoleKey.Spacebar:
                    HardDrop();
                    break;
            }
        }
    }

    private bool TryMove(int dx, int dy)
    {
        if (!_board.IsValidPosition(_currentPiece, dx, dy))
            return false;

        _currentPiece.X += dx;
        _currentPiece.Y += dy;
        return true;
    }

    private void TryRotate(bool clockwise)
    {
        int[,] rotated = _currentPiece.GetRotatedMatrix(clockwise);

        foreach (var (kickX, kickY) in WallKickOffsets)
        {
            if (_board.IsValidPosition(_currentPiece, kickX, kickY, rotated))
            {
                _currentPiece.ApplyRotation(rotated, kickX, kickY);
                return;
            }
        }
    }

    private void HardDrop()
    {
        int droppedRows = 0;
        while (TryMove(0, 1))
        {
            droppedRows++;
        }

        _score += droppedRows * 2;
        LockAndSpawnNext();
    }

    private void StepDown()
    {
        if (!TryMove(0, 1))
            LockAndSpawnNext();
    }

    private void LockAndSpawnNext()
    {
        _board.LockPiece(_currentPiece);

        int cleared = _board.ClearFullLines();
        if (cleared > 0)
        {
            _lines += cleared;
            _score += CalculateLineScore(cleared) * _level;
            _level = (_lines / 10) + 1;
        }

        _currentPiece = _nextPiece;
        _nextPiece = SpawnFromBag();

        // Game over occurs when the newly spawned piece immediately collides at the top
        if (!_board.IsValidPosition(_currentPiece))
        {
            _isGameOver = true;
            _isNewRecord = _scoreManager.TryUpdateHighScore(_score, _lines);
        }
    }

    private int CalculateLineScore(int cleared) => cleared switch
    {
        1 => 100,
        2 => 300,
        3 => 500,
        4 => 800,
        _ => 0
    };

    private Tetromino SpawnFromBag()
    {
        if (_bag.Count == 0)
            RefillBag();

        TetrominoType type = _bag.Dequeue();
        int size = type == TetrominoType.I ? 4 : (type == TetrominoType.O ? 2 : 3);
        int spawnX = (Board.Width - size) / 2;

        return new Tetromino(type, spawnX, 0);
    }

    /// <summary>
    /// Refills the piece queue using a Fisher-Yates shuffle (standard 7-bag randomizer)
    /// so every sequence of seven pieces contains all seven shapes without long droughts.
    /// </summary>
    private void RefillBag()
    {
        TetrominoType[] allTypes = Enum.GetValues<TetrominoType>();

        for (int i = allTypes.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (allTypes[i], allTypes[j]) = (allTypes[j], allTypes[i]);
        }

        foreach (var type in allTypes)
        {
            _bag.Enqueue(type);
        }
    }
}