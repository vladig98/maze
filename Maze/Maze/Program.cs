const int size = 1;
const int cellCountX = 59;
const int cellCountY = 14;

MazeCell[,] maze = GenerateMazeWithWalls(size, cellCountX, cellCountY);

GenerateMaze(maze, 0, 0);
int leftIndex = Random.Shared.Next(cellCountY);
int rightIndex = Random.Shared.Next(cellCountY);

AddEntrances(maze, 0, leftIndex, cellCountX - 1, rightIndex);

Console.Clear();

Stack<State> states = [];
MazeCell cell = maze[0, leftIndex];

while (true)
{
    if (cell.X == cellCountX - 1 && cell.Y == rightIndex)
    {
        break;
    }

    List<MazeCell> routes = [];
    if (!cell.TopLocked && cell.Y - 1 >= 0 && !maze[cell.X, cell.Y - 1].Tried)
    {
        routes.Add(maze[cell.X, cell.Y - 1]);
    }

    if (!cell.BottomLocked && cell.Y + 1 < cellCountY && !maze[cell.X, cell.Y + 1].Tried)
    {
        routes.Add(maze[cell.X, cell.Y + 1]);
    }

    if (!cell.RightLocked && cell.X + 1 < cellCountX && !maze[cell.X + 1, cell.Y].Tried)
    {
        routes.Add(maze[cell.X + 1, cell.Y]);
    }

    if (!cell.LeftLocked && cell.X - 1 >= 0 && !maze[cell.X - 1, cell.Y].Tried)
    {
        routes.Add(maze[cell.X - 1, cell.Y]);
    }

    if (routes.Count > 0)
    {
        State state = new(cell.X, cell.Y, routes);
        states.Push(state);
        maze[cell.X, cell.Y] = maze[cell.X, cell.Y] with { IsPath = true, Tried = true };
        cell = routes[0];
        state.Routes.RemoveAt(0);
    }
    else
    {
        bool popped = states.TryPop(out State state);
        if (!popped)
        {
            break;
        }

        while (state.Routes.Count == 0)
        {
            popped = states.TryPop(out state);
            if (!popped)
            {
                break;
            }
        }

        cell = state.Routes[0];
        state.Routes.RemoveAt(0);
    }

    Console.SetCursorPosition(0, 0);

    PrintMaze(maze);
    Thread.Sleep(100);
}

PrintMaze(maze);

static void GenerateMaze(MazeCell[,] maze, int x, int y)
{
    MarkCellAsVisited(maze, x, y);

    int maxX = maze.GetLength(0);
    int maxY = maze.GetLength(1);

    while (true)
    {
        MazeCell cell = maze[x, y];
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

        if (unvisitedNeighbors.Count == 0)
        {
            break;
        }

        int index = Random.Shared.Next(unvisitedNeighbors.Count);
        MazeCell neighbor = unvisitedNeighbors[index];

        int xDiff = neighbor.X - cell.X;
        int yDiff = neighbor.Y - cell.Y;

        if (xDiff > 0)
        {
            DemolishRightWall(maze, cell, neighbor);
        }
        else if (xDiff < 0)
        {
            DemolishLeftWall(maze, cell, neighbor);
        }
        else if (yDiff > 0)
        {
            DemolishBottomWall(maze, cell, neighbor);
        }
        else if (yDiff < 0)
        {
            DemolishTopWall(maze, cell, neighbor);
        }

        GenerateMaze(maze, neighbor.X, neighbor.Y);
    }
}

static void DemolishRightWall(MazeCell[,] maze, MazeCell cell, MazeCell neighbor)
{
    maze[cell.X, cell.Y] = cell with { RightLocked = false };
    maze[neighbor.X, neighbor.Y] = neighbor with { LeftLocked = false };
}

static void DemolishLeftWall(MazeCell[,] maze, MazeCell cell, MazeCell neighbor)
{
    maze[cell.X, cell.Y] = cell with { LeftLocked = false };
    maze[neighbor.X, neighbor.Y] = neighbor with { RightLocked = false };
}

static void DemolishTopWall(MazeCell[,] maze, MazeCell cell, MazeCell neighbor)
{
    maze[cell.X, cell.Y] = cell with { TopLocked = false };
    maze[neighbor.X, neighbor.Y] = neighbor with { BottomLocked = false };
}

static void DemolishBottomWall(MazeCell[,] maze, MazeCell cell, MazeCell neighbor)
{
    maze[cell.X, cell.Y] = cell with { BottomLocked = false };
    maze[neighbor.X, neighbor.Y] = neighbor with { TopLocked = false };
}

static void MarkCellAsVisited(MazeCell[,] maze, int x, int y)
{
    MazeCell cell = maze[x, y];
    maze[x, y] = cell with { Visited = true };
}

static void PrintMaze(MazeCell[,] maze)
{
    Console.OutputEncoding = System.Text.Encoding.UTF8;

    int cellCountX = maze.GetLength(0);
    int cellCountY = maze.GetLength(1);
    int cellSize = (cellCountX > 0 && cellCountY > 0) ? maze[0, 0].Size : 1;

    int gridWidth = cellCountX * (cellSize + 1) + 1;
    int gridHeight = cellCountY * (cellSize + 1) + 1;

    char[,] visualGrid = new char[gridWidth, gridHeight];

    for (int x = 0; x < cellCountX; x++)
    {
        for (int y = 0; y < cellCountY; y++)
        {
            int startX = x * (cellSize + 1);
            int startY = y * (cellSize + 1);

            // Draw Corners
            visualGrid[startX, startY] = 'W';
            visualGrid[startX + cellSize + 1, startY] = 'W';
            visualGrid[startX, startY + cellSize + 1] = 'W';
            visualGrid[startX + cellSize + 1, startY + cellSize + 1] = 'W';

            // Draw Walls
            for (int offset = 1; offset <= cellSize; offset++)
            {
                if (maze[x, y].TopLocked) visualGrid[startX + offset, startY] = 'W';
                if (maze[x, y].BottomLocked) visualGrid[startX + offset, startY + cellSize + 1] = 'W';
                if (maze[x, y].LeftLocked) visualGrid[startX, startY + offset] = 'W';
                if (maze[x, y].RightLocked) visualGrid[startX + cellSize + 1, startY + offset] = 'W';
            }

            // --- THE NEW PATH DRAWING LOGIC ---
            if (maze[x, y].IsPath)
            {
                // 1. Fill the center of the cell
                for (int offsetX = 1; offsetX <= cellSize; offsetX++)
                {
                    for (int offsetY = 1; offsetY <= cellSize; offsetY++)
                    {
                        visualGrid[startX + offsetX, startY + offsetY] = 'P';
                    }
                }

                // 2. Spill the path through the open doors to connect the line!
                if (!maze[x, y].TopLocked && (y == 0 || maze[x, y - 1].IsPath))
                {
                    for (int offsetX = 1; offsetX <= cellSize; offsetX++)
                        visualGrid[startX + offsetX, startY] = 'P';
                }
                if (!maze[x, y].BottomLocked && (y == cellCountY - 1 || maze[x, y + 1].IsPath))
                {
                    for (int offsetX = 1; offsetX <= cellSize; offsetX++)
                        visualGrid[startX + offsetX, startY + cellSize + 1] = 'P';
                }
                if (!maze[x, y].LeftLocked && (x == 0 || maze[x - 1, y].IsPath))
                {
                    for (int offsetY = 1; offsetY <= cellSize; offsetY++)
                        visualGrid[startX, startY + offsetY] = 'P';
                }
                if (!maze[x, y].RightLocked && (x == cellCountX - 1 || maze[x + 1, y].IsPath))
                {
                    for (int offsetY = 1; offsetY <= cellSize; offsetY++)
                        visualGrid[startX + cellSize + 1, startY + offsetY] = 'P';
                }
            }
        }
    }

    // Draw the grid to the console with COLOR
    for (int y = 0; y < gridHeight; y++)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (visualGrid[x, y] == 'W')
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("█");
            }
            else if (visualGrid[x, y] == 'P')
            {
                Console.ForegroundColor = ConsoleColor.Red; // The path is now bright red!
                Console.Write("█"); // Using a solid block instead of a dot
            }
            else
            {
                Console.Write(" ");
            }
        }
        Console.WriteLine();
    }

    // Reset the color so your terminal doesn't stay red forever!
    Console.ResetColor();
}

static void AddEntrances(MazeCell[,] maze, int x1, int y1, int x2, int y2)
{
    maze[x1, y1] = maze[x1, y1] with { LeftLocked = false };
    maze[x2, y2] = maze[x2, y2] with { RightLocked = false };
}

static MazeCell[,] GenerateMazeWithWalls(int size, int cellCountX, int cellCountY)
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

readonly record struct MazeCell
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

readonly record struct State
(
    int X,
    int Y,
    List<MazeCell> Routes
);