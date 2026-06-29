using MazeReact.Server.Models;
using MazeReact.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MazeReact.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MazeController(MazeGenerator generator, MazeSolver solver) : ControllerBase
    {
        [HttpPost("Solve")]
        public IEnumerable<MazeMove> Solve([FromBody] MazeCell[] flatMaze)
        {
            if (flatMaze == null || flatMaze.Length == 0)
            {
                return [];
            }

            int maxX = flatMaze.Max(c => c.X) + 1;
            int maxY = flatMaze.Max(c => c.Y) + 1;

            MazeCell[,] maze2D = new MazeCell[maxX, maxY];
            foreach (MazeCell cell in flatMaze)
            {
                maze2D[cell.X, cell.Y] = cell;
            }

            return solver.Solve(maze2D);
        }

        [HttpGet("Generate")]
        public IEnumerable<MazeCell> Generate(int size = 1, int xCount = 30, int yCount = 15)
        {
            MazeCell[,] maze2D = generator.Generate(size, xCount, yCount);
            return maze2D.Cast<MazeCell>();
        }
    }
}
