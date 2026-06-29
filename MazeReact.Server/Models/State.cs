namespace MazeReact.Server.Models;

readonly record struct State
(
    int X,
    int Y,
    List<MazeCell> Routes
);
