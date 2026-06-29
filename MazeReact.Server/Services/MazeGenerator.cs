using MazeReact.Server.Models;

namespace MazeReact.Server.Services;

public class MazeGenerator
{
    public MazeCell[,] Generate(int size, int xCount, int yCount)
    {
        MazeCell[,] maze = GenerateMazeWithWalls(size, xCount, yCount);
        GenerateMaze(maze, 0, 0);

        BraidMaze(maze, 0.05);
        AddEntrances(maze, xCount, yCount, 5);

        int leftIndex = Random.Shared.Next(yCount);
        int rightIndex = Random.Shared.Next(yCount);

        AddEntrances(maze, 0, leftIndex, xCount - 1, rightIndex);

        return maze;
    }

    private static void GenerateMaze(MazeCell[,] maze, int startX, int startY)
    {
        Stack<(int X, int Y)> stack = new();

        MarkCellAsVisited(maze, startX, startY);
        stack.Push((startX, startY));

        int maxX = maze.GetLength(0);
        int maxY = maze.GetLength(1);

        while (stack.Count > 0)
        {
            var (x, y) = stack.Peek();
            MazeCell current = maze[x, y];

            List<MazeCell> unvisitedNeighbors = new(4);

            if (x + 1 < maxX && !maze[x + 1, y].Visited)
            {
                unvisitedNeighbors.Add(maze[x + 1, y]);
            }

            if (x - 1 >= 0 && !maze[x - 1, y].Visited)
            {
                unvisitedNeighbors.Add(maze[x - 1, y]);
            }

            if (y + 1 < maxY && !maze[x, y + 1].Visited)
            {
                unvisitedNeighbors.Add(maze[x, y + 1]);
            }

            if (y - 1 >= 0 && !maze[x, y - 1].Visited)
            {
                unvisitedNeighbors.Add(maze[x, y - 1]);
            }

            if (unvisitedNeighbors.Count > 0)
            {
                int index = Random.Shared.Next(unvisitedNeighbors.Count);
                MazeCell neighbor = unvisitedNeighbors[index];

                int xDiff = neighbor.X - current.X;
                int yDiff = neighbor.Y - current.Y;

                if (xDiff > 0)
                {
                    DemolishRightWall(maze, current, neighbor);
                }
                else if (xDiff < 0)
                {
                    DemolishLeftWall(maze, current, neighbor);
                }
                else if (yDiff > 0)
                {
                    DemolishBottomWall(maze, current, neighbor);
                }
                else if (yDiff < 0)
                {
                    DemolishTopWall(maze, current, neighbor);
                }

                MarkCellAsVisited(maze, neighbor.X, neighbor.Y);
                stack.Push((neighbor.X, neighbor.Y));
            }
            else
            {
                stack.Pop();
            }
        }
    }

    private static void DemolishRightWall(MazeCell[,] maze, MazeCell cell, MazeCell neighbor)
    {
        maze[cell.X, cell.Y] = cell with { RightLocked = false };
        maze[neighbor.X, neighbor.Y] = neighbor with { LeftLocked = false };
    }

    private static void DemolishLeftWall(MazeCell[,] maze, MazeCell cell, MazeCell neighbor)
    {
        maze[cell.X, cell.Y] = cell with { LeftLocked = false };
        maze[neighbor.X, neighbor.Y] = neighbor with { RightLocked = false };
    }

    private static void DemolishTopWall(MazeCell[,] maze, MazeCell cell, MazeCell neighbor)
    {
        maze[cell.X, cell.Y] = cell with { TopLocked = false };
        maze[neighbor.X, neighbor.Y] = neighbor with { BottomLocked = false };
    }

    private static void DemolishBottomWall(MazeCell[,] maze, MazeCell cell, MazeCell neighbor)
    {
        maze[cell.X, cell.Y] = cell with { BottomLocked = false };
        maze[neighbor.X, neighbor.Y] = neighbor with { TopLocked = false };
    }

    private static void MarkCellAsVisited(MazeCell[,] maze, int x, int y)
    {
        MazeCell cell = maze[x, y];
        maze[x, y] = cell with { Visited = true };
    }

    private static void AddEntrances(MazeCell[,] maze, int x1, int y1, int x2, int y2)
    {
        maze[x1, y1] = maze[x1, y1] with { LeftLocked = false };
        maze[x2, y2] = maze[x2, y2] with { RightLocked = false };
    }

    private static void AddEntrances(MazeCell[,] maze, int cellCountX, int cellCountY, int exitCount)
    {
        int leftIndex = Random.Shared.Next(cellCountY);
        maze[0, leftIndex] = maze[0, leftIndex] with { LeftLocked = false };

        for (int i = 0; i < exitCount; i++)
        {
            int rightIndex = Random.Shared.Next(cellCountY);
            maze[cellCountX - 1, rightIndex] = maze[cellCountX - 1, rightIndex] with { RightLocked = false };
        }
    }

    private static void BraidMaze(MazeCell[,] maze, double braidChance = 0.05)
    {
        int maxX = maze.GetLength(0);
        int maxY = maze.GetLength(1);

        for (int x = 1; x < maxX - 1; x++)
        {
            for (int y = 1; y < maxY - 1; y++)
            {
                if (Random.Shared.NextDouble() < braidChance)
                {
                    MazeCell cell = maze[x, y];
                    int dir = Random.Shared.Next(4);

                    if (dir == 0 && cell.TopLocked)
                    {
                        DemolishTopWall(maze, cell, maze[x, y - 1]);
                    }
                    else if (dir == 1 && cell.RightLocked)
                    {
                        DemolishRightWall(maze, cell, maze[x + 1, y]);
                    }
                    else if (dir == 2 && cell.BottomLocked)
                    {
                        DemolishBottomWall(maze, cell, maze[x, y + 1]);
                    }
                    else if (dir == 3 && cell.LeftLocked)
                    {
                        DemolishLeftWall(maze, cell, maze[x - 1, y]);
                    }
                }
            }
        }
    }

    private static MazeCell[,] GenerateMazeWithWalls(int size, int cellCountX, int cellCountY)
    {
        MazeCell[,] maze = new MazeCell[cellCountX, cellCountY];
        for (int i = 0; i < cellCountX; i++)
        {
            for (int j = 0; j < cellCountY; j++)
            {
                maze[i, j] = new MazeCell(
                    X: i,
                    Y: j,
                    Size: size,
                    TopLocked: true,
                    RightLocked: true,
                    BottomLocked: true,
                    LeftLocked: true,
                    Visited: false,
                    IsPath: false,
                    Tried: false);
            }
        }

        return maze;
    }
}
