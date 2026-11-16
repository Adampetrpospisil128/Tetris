using System.Text;
using Tetris;

// Enable UTF-8 so Unicode box-drawing and block characters (██, ░░) render properly
Console.OutputEncoding = Encoding.UTF8;
Console.Title = "Console Tetris - C#";

var engine = new GameEngine();
engine.Run();