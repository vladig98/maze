using MazeReact.Server.Models;

namespace MazeReact.Server.Services;

public class MazeSolver
{
    public List<MazeMove> Solve(MazeCell[,] maze)
    {
        int cellCountX = maze.GetLength(0);
        int cellCountY = maze.GetLength(1);

        int leftIndex = -1;
        int rightIndex = -1;

        for (int i = 0; i < cellCountY; i++)
        {
            if (!maze[0, i].LeftLocked)
            {
                leftIndex = i;
            }

            if (!maze[cellCountX - 1, i].RightLocked)
            {
                rightIndex = i;
            }
        }

        Stack <State> states = [];
        MazeCell cell = maze[0, leftIndex];

        List<MazeMove> moves = [];

        while (true)
        {
            maze[cell.X, cell.Y] = maze[cell.X, cell.Y] with { IsPath = true, Tried = true };
            moves.Add(new MazeMove(cell.X, cell.Y, true));

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
                    moves.Add(new MazeMove(state.X, state.Y, false));

                    popped = states.TryPop(out state);
                    if (!popped)
                    {
                        break;
                    }
                }

                if (!popped)
                {
                    break;
                }

                moves.Add(new MazeMove(cell.X, cell.Y, false));

                cell = state.Routes[0];
                state.Routes.RemoveAt(0);
                states.Push(state);
            }
        }

        return moves;
    }
}
