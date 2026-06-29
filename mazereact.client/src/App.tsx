import { useState } from 'react';
import './App.css';

interface MazeCell {
    x: number;
    y: number;
    size: number;
    topLocked: boolean;
    rightLocked: boolean;
    bottomLocked: boolean;
    leftLocked: boolean;
    visited: boolean;
    isPath: boolean;
    tried: boolean;
}

// Our new Delta Update model
interface MazeMove {
    x: number;
    y: number;
    isPath: boolean;
}

function App() {
    const [maze, setMaze] = useState<MazeCell[] | null>(null);
    const [dimensions, setDimensions] = useState({ x: 0, y: 0 });
    const [isLoading, setIsLoading] = useState(false);
    const [isPlaying, setIsPlaying] = useState(false);

    // Configuration state for the input boxes
    const [config, setConfig] = useState({
        xCount: 40,
        yCount: 20,
        size: 1
    });

    // Handle user typing in the config boxes
    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setConfig(prev => ({
            ...prev,
            [name]: parseInt(value) || 1 // Fallback to 1 if the user clears the box
        }));
    };

    // 1. Generate Maze
    const handleGenerate = async () => {
        setIsLoading(true);
        try {
            const response = await fetch(`maze/generate?size=${config.size}&xCount=${config.xCount}&yCount=${config.yCount}`);
            const data: MazeCell[] = await response.json();

            // Calculate grid dimensions for CSS Math
            const maxX = Math.max(...data.map(c => c.x)) + 1;
            const maxY = Math.max(...data.map(c => c.y)) + 1;

            setDimensions({ x: maxX, y: maxY });
            setMaze(data);
        } catch (error) {
            console.error("Failed to generate maze", error);
        }
        setIsLoading(false);
    };

    // 2. Solve Maze & Trigger Animation
    const handleSolve = async () => {
        if (!maze) return;
        setIsLoading(true);

        try {
            const response = await fetch('maze/solve', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(maze)
            });

            // We are now fetching the tiny Delta moves instead of gigabytes of grids!
            const moves: MazeMove[] = await response.json();
            playAnimation(moves);

        } catch (error) {
            console.error("Failed to solve maze", error);
            setIsLoading(false);
        }
    };

    // 3. The Delta Animation Loop
    const playAnimation = (moves: MazeMove[]) => {
        setIsPlaying(true);
        let frame = 0;

        const interval = setInterval(() => {
            if (frame >= moves.length) {
                clearInterval(interval);
                setIsPlaying(false);
                setIsLoading(false);
                return;
            }

            const move = moves[frame];

            // Update just the specific cell that changed
            setMaze(prevMaze => {
                if (!prevMaze) return null;

                // C# flattens arrays by iterating the Y axis first, so we calculate the index using this exact math
                const index = move.x * dimensions.y + move.y;

                const newMaze = [...prevMaze];
                newMaze[index] = { ...newMaze[index], isPath: move.isPath };
                return newMaze;
            });

            frame++;
        }, 10); // Running at a blistering 10ms per frame for massive mazes
    };

    // 4. The Renderer
    return (
        <div className="app-container">
            <h1>React Maze Solver</h1>

            <div className="input-group">
                <label>
                    Width (X):
                    <input type="number" name="xCount" value={config.xCount} onChange={handleInputChange} min="5" max="250" />
                </label>
                <label>
                    Height (Y):
                    <input type="number" name="yCount" value={config.yCount} onChange={handleInputChange} min="5" max="250" />
                </label>
                <label>
                    Size (Zoom):
                    <input type="number" name="size" value={config.size} onChange={handleInputChange} min="1" max="10" />
                </label>
            </div>

            <div className="controls">
                <button onClick={handleGenerate} disabled={isLoading || isPlaying}>
                    Generate Maze
                </button>
                <button onClick={handleSolve} disabled={!maze || isLoading || isPlaying}>
                    Solve Animation
                </button>
            </div>

            {maze && (
                <div
                    className="maze-container"
                    style={{
                        '--grid-x': dimensions.x,
                        '--grid-y': dimensions.y,
                        '--cell-size': `${config.size * 20}px` // Dynamically scaling the pixels
                    } as React.CSSProperties}
                >
                    {maze.map((cell, index) => {
                        let cellClasses = "maze-cell";
                        if (cell.topLocked) cellClasses += " wall-top";
                        if (cell.rightLocked) cellClasses += " wall-right";
                        if (cell.bottomLocked) cellClasses += " wall-bottom";
                        if (cell.leftLocked) cellClasses += " wall-left";
                        if (cell.isPath) cellClasses += " is-path";

                        return (
                            <div
                                key={index}
                                className={cellClasses}
                                style={{
                                    // Forcing CSS Grid to put the cell exactly where it belongs
                                    gridColumn: cell.x + 1,
                                    gridRow: cell.y + 1
                                }}
                            />
                        );
                    })}
                </div>
            )}
        </div>
    );
}

export default App;