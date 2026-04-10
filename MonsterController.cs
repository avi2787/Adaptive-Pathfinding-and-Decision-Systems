using System;
using System.Collections.Generic;
using System.Drawing;

namespace NEA_ai_pathfinding
{
    // keeps monster movement in one class so it is easier to read
    public class MonsterController
    {
        private readonly Random random = new Random();
        public bool Active;
        public int X;
        public int Y;
        public int PrevX = -1;
        public int PrevY = -1;
        public DateTime RemoveTime;
        private int throttleCounter = 0;

        public void Reset()
        {
            Active = false;
            X = 0;
            Y = 0;
            PrevX = -1;
            PrevY = -1;
            RemoveTime = DateTime.MinValue;
        }

        // pick a path cell for the monster. I prefer near the runner if possible
        public bool Spawn(Maze maze, Point runnerPos, bool preferNear, int lifetimeSeconds, int minDistanceFromRunner)
        {
            if (maze == null)
            {
                return false;
            }

            List<Point> allPaths = new List<Point>();
            List<Point> near = new List<Point>();

            for (int y = 0; y < maze.height; y++)
            {
                for (int x = 0; x < maze.width; x++)
                {
                    if (maze.grid[x, y] == 1)
                    {
                        Point p = new Point(x, y);
                        int manhattan = Math.Abs(x - runnerPos.X) + Math.Abs(y - runnerPos.Y);
                        if (manhattan < minDistanceFromRunner)
                        {
                            continue;
                        }

                        allPaths.Add(p);
                        if (manhattan <= 6)
                        {
                            near.Add(p);
                        }
                    }
                }
            }

            if (allPaths.Count == 0)
            {
                return false;
            }

            List<Point> pickFrom = allPaths;
            if (preferNear && near.Count > 0)
            {
                pickFrom = near;
            }

            Point spawn = pickFrom[random.Next(pickFrom.Count)];
            X = spawn.X;
            Y = spawn.Y;
            PrevX = X;
            PrevY = Y;
            Active = true;
            RemoveTime = DateTime.UtcNow.AddSeconds(lifetimeSeconds);
            return true;
        }

        // move one step toward the runner using a simple BFS
        public MonsterStepResult Step(Maze maze, Point runnerPos)
        {
            if (!Active || maze == null)
            {
                return MonsterStepResult.None;
            }

            // throttle heavy BFS so big mazes do not lag
            throttleCounter++;
            if (throttleCounter < 3)
            {
                return MonsterStepResult.None;
            }
            throttleCounter = 0;

            int nextX;
            int nextY;

            if (!TryGetStep(maze, runnerPos, out nextX, out nextY))
            {
                return MonsterStepResult.None;
            }

            PrevX = X;
            PrevY = Y;
            X = nextX;
            Y = nextY;

            if (X == runnerPos.X && Y == runnerPos.Y)
            {
                return MonsterStepResult.CaughtRunner;
            }

            return MonsterStepResult.Moved;
        }

        private bool TryGetStep(Maze maze, Point runnerPos, out int nextX, out int nextY)
        {
            nextX = X;
            nextY = Y;

            if (!maze.TryBuildPath(new Point(X, Y), runnerPos, out List<Point> path))
            {
                return false;
            }

            // first cell is current, second is where to step
            if (path.Count < 2)
            {
                return false;
            }

            nextX = path[1].X;
            nextY = path[1].Y;
            return true;
        }
    }

    public enum MonsterStepResult
    {
        None,
        Moved,
        CaughtRunner
    }
}
