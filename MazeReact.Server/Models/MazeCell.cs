namespace MazeReact.Server.Models;

public readonly record struct MazeCell
(
    int X,
    int Y,
    int Size,
    bool TopLocked,
    bool RightLocked,
    bool BottomLocked,
    bool LeftLocked,
    bool Visited,
    bool IsPath,
    bool Tried
);
