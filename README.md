# Console Tetris (C#)

A classic Tetris implementation built entirely in pure C# (.NET 10) as a console application with clean object-oriented architecture and zero external dependencies.

## Features
- **7-Bag Randomizer:** Uses a Fisher-Yates shuffle to guarantee fair piece distribution without long droughts.
- **Matrix Rotation & Wall Kicks:** Supports clockwise and counter-clockwise 90° matrix rotation with automatic wall-kick offsets near borders and stacked blocks.
- **Ghost Piece:** Projects the active Tetromino's exact landing position onto the grid.
- **Flicker-Free Rendering:** Redraws the 10×20 playfield and HUD sidebar in-place via `Console.SetCursorPosition`.
- **Persistent High Scores:** Automatically saves the best score and line count to `highscores.json` using `System.Text.Json`.

## Controls
| Key | Action |
| :--- | :--- |
| `←` / `→` or `A` / `D` | Move left / right |
| `↑` / `W` / `X` | Rotate clockwise |
| `Z` | Rotate counter-clockwise |
| `↓` / `S` | Soft drop (+1 pt per cell) |
| `Space` | Hard drop (+2 pts per cell) |
| `P` | Pause / Resume |
| `R` | Restart after Game Over |
| `Esc` | Quit |

## Project Structure
- `Program.cs` – Entry point and UTF-8 console setup.
- `GameEngine.cs` – Real-time loop (`Stopwatch`), input handling, gravity scaling, and 7-bag queue.
- `Board.cs` – 10×20 grid state, collision validation, piece locking, and line-clearing logic.
- `Tetromino.cs` – Piece definitions and 2D matrix rotation math.
- `ConsoleRenderer.cs` – In-place console rendering, ghost preview, and sidebar UI.
- `ScoreManager.cs` – JSON serialization for persistent high-score records.

## How to Run
1. Clone the repository:
   ```bash
   git clone [https://github.com/Adampetrpospisil128/Tetris.git](https://github.com/Adampetrpospisil128/Tetris.git)
