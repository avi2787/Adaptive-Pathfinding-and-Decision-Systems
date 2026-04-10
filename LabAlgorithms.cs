using System;
using System.Collections.Generic;
using System.Drawing;

namespace NEA_ai_pathfinding
{
    internal static class LabDirs
    {
        public static readonly (int dx, int dy)[] Four = new (int, int)[]
        {
            (1, 0), (-1, 0), (0, 1), (0, -1)
        };

        public static readonly (int dx, int dy)[] Four2 = new (int, int)[]
        {
            (2, 0), (-2, 0), (0, 2), (0, -2)
        };
    }

    internal static class LabBias
    {
        public static bool Hit(Random rng, int bias)
        {
            if (bias <= 0) return false;
            if (bias >= 100) return true;
            return rng.Next(100) < bias;
        }
    }

    internal static class LabTurnHelper
    {
        public static bool IsSameDir(int dx1, int dy1, int dx2, int dy2)
        {
            if (dx1 != 0) dx1 = dx1 / Math.Abs(dx1);
            if (dy1 != 0) dy1 = dy1 / Math.Abs(dy1);
            if (dx2 != 0) dx2 = dx2 / Math.Abs(dx2);
            if (dy2 != 0) dy2 = dy2 / Math.Abs(dy2);
            return dx1 == dx2 && dy1 == dy2;
        }

        public static bool IsPerpendicular(int dx1, int dy1, int dx2, int dy2)
        {
            bool aHorizontal = dx1 != 0 && dy1 == 0;
            bool aVertical = dy1 != 0 && dx1 == 0;
            bool bHorizontal = dx2 != 0 && dy2 == 0;
            bool bVertical = dy2 != 0 && dx2 == 0;
            return (aHorizontal && bVertical) || (aVertical && bHorizontal);
        }
    }

    // simple settings bag for generators
    public class LabGenSettings
    {
        public int Width;
        public int Height;
        public double BraidPerc;
        public double RoomPerc;
        public int TurnBias;
        public int StepsPerTick;
    }

    public interface ILabMazeGenerator
    {
        // start state for a new maze
        void Initialise(Maze maze, Random rng, LabGenSettings settings);
        // one animation step; true while still carving
        bool Step();
        string Name { get; }
    }

    public interface ILabMazeSolver
    {
        // start a fresh solve run
        void Initialise(Maze maze, Point start, Point exit, bool[,] exploredMask, bool[,] pathMask);
        // one animation step; true while still exploring
        bool Step();
        int VisitedCount { get; }
        int PathLength { get; }
        bool FoundPath { get; }
        string Name { get; }
    }

    // shared little tweaks so most generators can reuse braid/room logic
    internal static class LabMazeTweaks
    {
        public static void ApplyRooms(Maze maze, Random rng, double roomPerc)
        {
            if (maze == null)
            {
                return;
            }
            if (roomPerc <= 0)
            {
                return;
            }

            // roomPerc is a 0..1 slider; scale by area so changes are visible
            int area = maze.width * maze.height;
            int rooms = (int)(area * roomPerc / 80.0);
            if (rooms < 1) rooms = 1;
            if (rooms > 40) rooms = 40;

            for (int i = 0; i < rooms; i++)
            {
                int cx = rng.Next(2, maze.width - 2);
                int cy = rng.Next(2, maze.height - 2);
                for (int y = cy - 1; y <= cy + 1; y++)
                {
                    for (int x = cx - 1; x <= cx + 1; x++)
                    {
                        if (x > 0 && y > 0 && x < maze.width - 1 && y < maze.height - 1)
                        {
                            maze.grid[x, y] = 1;
                        }
                    }
                }
            }
        }

        public static void ApplyBraid(Maze maze, Random rng, double braidPerc)
        {
            if (maze == null)
            {
                return;
            }
            if (braidPerc <= 0)
            {
                return;
            }

            for (int y = 1; y < maze.height - 1; y++)
            {
                for (int x = 1; x < maze.width - 1; x++)
                {
                    if (maze.grid[x, y] != 1)
                    {
                        continue;
                    }

                    int openCount = 0;
                    for (int i = 0; i < LabDirs.Four.Length; i++)
                    {
                        int nx = x + LabDirs.Four[i].dx;
                        int ny = y + LabDirs.Four[i].dy;
                        if (maze.grid[nx, ny] == 1)
                        {
                            openCount = openCount + 1;
                        }
                    }

                    if (openCount == 1 && rng.NextDouble() < braidPerc)
                    {
                        List<(int wx, int wy)> closed = new List<(int wx, int wy)>();
                        for (int i = 0; i < LabDirs.Four.Length; i++)
                        {
                            int wx = x + LabDirs.Four[i].dx;
                            int wy = y + LabDirs.Four[i].dy;
                            if (wx > 0 && wy > 0 && wx < maze.width - 1 && wy < maze.height - 1)
                            {
                                if (maze.grid[wx, wy] == 0)
                                {
                                    closed.Add((wx, wy));
                                }
                            }
                        }

                        if (closed.Count > 0)
                        {
                            var pick = closed[rng.Next(closed.Count)];
                            maze.grid[pick.wx, pick.wy] = 1;
                        }
                    }
                }
            }
        }
    }

    // Prim variant for lab sandbox
    public class LabPrimGenerator : ILabMazeGenerator
    {
        private Maze maze;
        private Random rng;
        private readonly List<(int X, int Y)> frontierCells = new List<(int X, int Y)>();
        private int stepsPerTick = 8;
        private double braidPerc;
        private double roomPerc;
        private bool tweaksDone;
        private int turnBias;
        private int lastDirX;
        private int lastDirY;
        public string Name => "Prim";

        // prepare frontier list and carve from (1,1)
        public void Initialise(Maze mazeToUse, Random randomSource, LabGenSettings settings)
        {
            maze = mazeToUse;
            rng = randomSource ?? new Random(Guid.NewGuid().GetHashCode());
            stepsPerTick = Math.Max(1, settings.StepsPerTick);
            braidPerc = settings.BraidPerc;
            roomPerc = settings.RoomPerc;
            tweaksDone = false;
            turnBias = settings.TurnBias;
            lastDirX = 0;
            lastDirY = 0;
            frontierCells.Clear();

            // start from a completely walled grid so every run is fresh
            for (int y = 0; y < maze.height; y++)
            {
                for (int x = 0; x < maze.width; x++)
                {
                    maze.grid[x, y] = 0;
                }
            }
            maze.tokens?.Clear();

            // textbook Prim start: open (1,1) then add its frontier cells
            maze.grid[1, 1] = 1;
            AddFrontier(3, 1);
            AddFrontier(1, 3);
        }

        // carve a few frontier cells each tick
        public bool Step()
        {
            for (int i = 0; i < stepsPerTick; i++)
            {
                if (frontierCells.Count == 0)
                {
                    // clean up frontier markers so final maze is only walls/paths
                    for (int y = 0; y < maze.height; y++)
                    {
                        for (int x = 0; x < maze.width; x++)
                        {
                            if (maze.grid[x, y] == 2) maze.grid[x, y] = 0;
                        }
                    }
                    if (!tweaksDone)
                    {
                        // Rooms/Braid change the style after generation. For testing TurnBias, set them to 0.
                        LabMazeTweaks.ApplyRooms(maze, rng, roomPerc);
                        LabMazeTweaks.ApplyBraid(maze, rng, braidPerc);
                        tweaksDone = true;
                    }
                    return false;
                }

                int pick = rng.Next(frontierCells.Count);
                var frontierCell = frontierCells[pick];
                frontierCells.RemoveAt(pick);

                // frontier cell must connect back to an already open neighbour
                var carvedNeighbours = BuildCarvedNeighbours(frontierCell);
                if (carvedNeighbours.Count == 0)
                {
                    continue;
                }

                var chosen = ChooseNeighbour(carvedNeighbours, frontierCell); // bias picks turn vs straight

                maze.grid[frontierCell.X, frontierCell.Y] = 1;
                maze.grid[chosen.WallX, chosen.WallY] = 1;

                lastDirX = chosen.CellX - frontierCell.X;
                lastDirY = chosen.CellY - frontierCell.Y;

                foreach (var dir in LabDirs.Four2)
                {
                    AddFrontier(frontierCell.X + dir.dx, frontierCell.Y + dir.dy);
                }
            }
            return true;
        }

        private void AddFrontier(int x, int y)
        {
            if (x > 0 && y > 0 && x < maze.width - 1 && y < maze.height - 1)
            {
                if (maze.grid[x, y] == 0)
                {
                    maze.grid[x, y] = 2;
                    frontierCells.Add((x, y));
                }
            }
        }

        private List<(int CellX, int CellY, int WallX, int WallY)> BuildCarvedNeighbours((int X, int Y) frontierCell)
        {
            var carvedNeighbours = new List<(int CellX, int CellY, int WallX, int WallY)>();
            foreach (var dir in LabDirs.Four2)
            {
                int neighbourX = frontierCell.X + dir.dx;
                int neighbourY = frontierCell.Y + dir.dy;
                int wallX = frontierCell.X + dir.dx / 2;
                int wallY = frontierCell.Y + dir.dy / 2;
                if (neighbourX > 0 && neighbourY > 0 && neighbourX < maze.width - 1 && neighbourY < maze.height - 1)
                {
                    if (maze.grid[neighbourX, neighbourY] == 1)
                    {
                        carvedNeighbours.Add((neighbourX, neighbourY, wallX, wallY));
                    }
                }
            }
            return carvedNeighbours;
        }

        private (int CellX, int CellY, int WallX, int WallY) ChooseNeighbour(List<(int CellX, int CellY, int WallX, int WallY)> options, (int X, int Y) frontierCell)
        {
            bool hasLast = (lastDirX != 0 || lastDirY != 0);
            if (hasLast)
            {
                bool preferTurn = LabBias.Hit(rng, turnBias);
                var preferred = new List<(int CellX, int CellY, int WallX, int WallY)>();
                for (int i = 0; i < options.Count; i++)
                {
                    int dx = options[i].CellX - frontierCell.X;
                    int dy = options[i].CellY - frontierCell.Y;
                    bool straight = LabTurnHelper.IsSameDir(dx, dy, lastDirX, lastDirY);
                    bool turn = LabTurnHelper.IsPerpendicular(dx, dy, lastDirX, lastDirY);
                    if (preferTurn && turn) preferred.Add(options[i]);
                    if (!preferTurn && straight) preferred.Add(options[i]);
                }
                if (preferred.Count > 0)
                {
                    return preferred[rng.Next(preferred.Count)];
                }
            }
            return options[rng.Next(options.Count)];
        }
    }

    // DFS backtracker generator
    public class LabDfsGenerator : ILabMazeGenerator
    {
        private Maze maze;
        private Random rng;
        private readonly Stack<(int x, int y, int lastDx, int lastDy)> stack = new Stack<(int, int, int, int)>();
        private int turnBias = 80;
        private int stepsPerTick = 8;
        private double braidPerc;
        private double roomPerc;
        private bool tweaksDone;
        public string Name => "DFS";

        public void Initialise(Maze m, Random r, LabGenSettings settings)
        {
            maze = m;
            rng = r ?? new Random(Guid.NewGuid().GetHashCode()); // defensive seed
            turnBias = settings.TurnBias;
            stepsPerTick = Math.Max(1, settings.StepsPerTick);
            braidPerc = settings.BraidPerc;
            roomPerc = settings.RoomPerc;
            tweaksDone = false;
            stack.Clear();

            // clean slate: all walls and no leftover tokens
            for (int y = 0; y < maze.height; y++)
            {
                for (int x = 0; x < maze.width; x++)
                {
                    maze.grid[x, y] = 0;
                }
            }
            maze.tokens?.Clear();

            // textbook recursive backtracker start
            maze.grid[1, 1] = 1;
            stack.Push((1, 1, 0, 0));
        }

        public bool Step()
        {
            for (int i = 0; i < stepsPerTick; i++)
            {
                if (stack.Count == 0)
                {
                    if (!tweaksDone)
                    {
                        // Rooms/Braid change the style after generation. For testing TurnBias, set them to 0.
                        LabMazeTweaks.ApplyRooms(maze, rng, roomPerc);
                        LabMazeTweaks.ApplyBraid(maze, rng, braidPerc);
                        tweaksDone = true;
                    }
                    return false;
                }

                var (x, y, lastDx, lastDy) = stack.Peek();
                var choices = BuildOptions(x, y);
                if (choices.Count > 0)
                {
                    // choose an unvisited neighbour two cells away
                    var dir = PickDirection(choices, lastDx, lastDy);
                    int nx = x + dir.dx;
                    int ny = y + dir.dy;
                    int wx = x + dir.dx / 2;
                    int wy = y + dir.dy / 2;

                    maze.grid[wx, wy] = 1;
                    maze.grid[nx, ny] = 1;
                    TryAddToken(nx, ny);
                    stack.Push((nx, ny, dir.dx, dir.dy));
                }
                else
                {
                    // no unvisited neighbours, backtrack
                    stack.Pop();
                }
            }
            return stack.Count > 0;
        }

        private List<(int dx, int dy)> BuildOptions(int x, int y)
        {
            var opts = new List<(int dx, int dy)>();
            foreach (var dir in LabDirs.Four2)
            {
                int nx = x + dir.dx;
                int ny = y + dir.dy;
                if (nx > 0 && ny > 0 && nx < maze.width - 1 && ny < maze.height - 1 && maze.grid[nx, ny] == 0)
                {
                    opts.Add((dir.dx, dir.dy));
                }
            }
            return opts;
        }

        private (int dx, int dy) PickDirection(List<(int dx, int dy)> options, int lastDx, int lastDy)
        {
            bool hasLast = (lastDx != 0 || lastDy != 0);
            if (hasLast)
            {
                bool preferTurn = LabBias.Hit(rng, turnBias);
                var preferred = new List<(int dx, int dy)>();
                for (int i = 0; i < options.Count; i++)
                {
                    bool straight = LabTurnHelper.IsSameDir(options[i].dx, options[i].dy, lastDx, lastDy);
                    bool turn = LabTurnHelper.IsPerpendicular(options[i].dx, options[i].dy, lastDx, lastDy);
                    if (preferTurn && turn) preferred.Add(options[i]);
                    if (!preferTurn && straight) preferred.Add(options[i]);
                }
                if (preferred.Count > 0)
                {
                    return preferred[rng.Next(preferred.Count)];
                }
            }
            return options[rng.Next(options.Count)];
        }

        private void TryAddToken(int x, int y)
        {
            if (maze.tokens == null)
            {
                return;
            }
            if (x <= 0 || y <= 0 || x >= maze.width - 1 || y >= maze.height - 1)
            {
                return;
            }
            maze.tokens.Add((x, y, 1));
        }
    }

    // Binary Tree: visit each cell and carve to one neighbour (simple patterns, gentle diagonals)
    // Style: tends to have diagonal-looking corridors; variant choice per run keeps it from repeating
    public class LabBinaryTreeGenerator : ILabMazeGenerator
    {
        private Maze maze;
        private Random rng;
        private int currentX;
        private int currentY;
        private int turnBias;
        private int lastDirX;
        private int lastDirY;
        private double braidPerc;
        private double roomPerc;
        private bool tweaksDone;
        private int stepsPerTick = 4;
        private bool useNorthEastVariant; // true = connect north/east, false = south/east
        private bool rowLeftToRight;
        public string Name => "Binary Tree";

        public void Initialise(Maze m, Random r, LabGenSettings settings)
        {
            maze = m;
            rng = r ?? new Random(Guid.NewGuid().GetHashCode()); // defensive so runs do not share a fixed seed
            turnBias = settings.TurnBias;
            braidPerc = settings.BraidPerc;
            roomPerc = settings.RoomPerc;
            tweaksDone = false;
            lastDirX = 0;
            lastDirY = 0;
            stepsPerTick = Math.Max(1, settings.StepsPerTick);
            useNorthEastVariant = rng.Next(2) == 0; // pick which textbook flavour we use this run
            rowLeftToRight = rng.Next(2) == 0; // pick scan direction for first row

            // start from all walls
            for (int y = 0; y < maze.height; y++)
            {
                for (int x = 0; x < maze.width; x++)
                {
                    maze.grid[x, y] = 0;
                }
            }
            maze.tokens?.Clear();

            // open every cell at odd coords ready for east/south carving
            for (int y = 1; y < maze.height - 1; y += 2)
            {
                for (int x = 1; x < maze.width - 1; x += 2)
                {
                    maze.grid[x, y] = 1;
                }
            }

            currentX = rowLeftToRight ? 1 : LastOddX();
            currentY = 1;
        }

        public bool Step()
        {
            for (int step = 0; step < stepsPerTick; step++)
            {
                bool finished = currentY >= maze.height - 1;
                if (finished)
                {
                    if (!tweaksDone)
                    {
                        // Rooms/Braid change the style after generation. For testing TurnBias, set them to 0.
                        LabMazeTweaks.ApplyRooms(maze, rng, roomPerc);
                        LabMazeTweaks.ApplyBraid(maze, rng, braidPerc);
                        tweaksDone = true;
                    }
                    return false;
                }

                // textbook binary tree: for each cell pick between two neighbour directions
                bool canEast = currentX + 2 < maze.width - 1;
                bool canNorth = currentY - 2 > 0;
                bool canSouth = currentY + 2 < maze.height - 1;
                var options = new List<(int dx, int dy)>();
                if (canEast)
                {
                    options.Add((2, 0));
                }
                if (useNorthEastVariant)
                {
                    if (canNorth) options.Add((0, -2));
                }
                else
                {
                    if (canSouth) options.Add((0, 2));
                }

                if (options.Count > 0)
                {
                    var dir = ChooseDirection(options);
                    int wallX = currentX + dir.dx / 2;
                    int wallY = currentY + dir.dy / 2;
                    int nx = currentX + dir.dx;
                    int ny = currentY + dir.dy;
                    maze.grid[wallX, wallY] = 1;
                    maze.grid[nx, ny] = 1; // already open but keeps intent clear
                    lastDirX = dir.dx;
                    lastDirY = dir.dy;
                }

                // move to next odd cell in the grid scan (row direction can flip each row)
                currentX += rowLeftToRight ? 2 : -2;
                if (rowLeftToRight && currentX >= maze.width - 1)
                {
                    rowLeftToRight = rng.Next(2) == 0;
                    currentX = rowLeftToRight ? 1 : LastOddX();
                    currentY += 2;
                }
                else if (!rowLeftToRight && currentX <= 0)
                {
                    rowLeftToRight = rng.Next(2) == 0;
                    currentX = rowLeftToRight ? 1 : LastOddX();
                    currentY += 2;
                }
            }
            return true;
        }

        private (int dx, int dy) ChooseDirection(List<(int dx, int dy)> options)
        {
            bool hasLast = (lastDirX != 0 || lastDirY != 0);
            if (hasLast)
            {
                bool preferAlternate = LabBias.Hit(rng, turnBias);
                if (preferAlternate)
                {
                    for (int i = 0; i < options.Count; i++)
                    {
                        bool same = LabTurnHelper.IsSameDir(options[i].dx, options[i].dy, lastDirX, lastDirY);
                        if (!same)
                        {
                            return options[i];
                        }
                    }
                }
            }
            return options[rng.Next(options.Count)];
        }

        private int LastOddX()
        {
            int v = maze.width - 2;
            if (v % 2 == 0) v -= 1;
            return v;
        }
    }

    // Kruskal generator
    public class LabKruskalGenerator : ILabMazeGenerator
    {
        private Maze maze;
        private Random rng;
        private readonly List<(int x1, int y1, int x2, int y2, int wx, int wy)> edges = new List<(int, int, int, int, int, int)>();
        private int index;
        private int[] parent;
        private int[] rank;
        private int cellsAcross;
        private int cellsDown;
        private double braidPerc;
        private double roomPerc;
        private bool tweaksDone;
        private int turnBias;
        private bool? lastAcceptedVertical;
        private int stepsPerTick = 8;
        public string Name => "Kruskal";

        public void Initialise(Maze m, Random r, LabGenSettings settings)
        {
            maze = m;
            rng = r ?? new Random(Guid.NewGuid().GetHashCode()); // defensive seed so repeats are unlikely
            edges.Clear();
            index = 0;
            braidPerc = settings.BraidPerc;
            roomPerc = settings.RoomPerc;
            tweaksDone = false;
            turnBias = settings.TurnBias;
            lastAcceptedVertical = null;
            stepsPerTick = Math.Max(1, settings.StepsPerTick);
            cellsAcross = (maze.width - 1) / 2;
            cellsDown = (maze.height - 1) / 2;
            int totalCells = cellsAcross * cellsDown;
            parent = new int[totalCells];
            rank = new int[totalCells];

            // start from all walls
            for (int y = 0; y < maze.height; y++)
            {
                for (int x = 0; x < maze.width; x++)
                {
                    maze.grid[x, y] = 0;
                }
            }
            maze.tokens?.Clear();

            for (int i = 0; i < totalCells; i++)
            {
                parent[i] = i;
                rank[i] = 0;
            }

            // open every cell at odd coords
            for (int y = 1; y < maze.height - 1; y += 2)
            {
                for (int x = 1; x < maze.width - 1; x += 2)
                {
                    maze.grid[x, y] = 1;
                }
            }

            // build edge list (each wall between two cells)
            for (int cy = 0; cy < cellsDown; cy++)
            {
                for (int cx = 0; cx < cellsAcross; cx++)
                {
                    int gx = cx * 2 + 1;
                    int gy = cy * 2 + 1;
                    if (cx + 1 < cellsAcross)
                    {
                        edges.Add((gx, gy, gx + 2, gy, gx + 1, gy));
                    }
                    if (cy + 1 < cellsDown)
                    {
                        edges.Add((gx, gy, gx, gy + 2, gx, gy + 1));
                    }
                }
            }

            Shuffle(edges);
        }

        public bool Step()
        {
            for (int s = 0; s < stepsPerTick && index < edges.Count; s++)
            {
                if (lastAcceptedVertical.HasValue && edges.Count - index > 1)
                {
                    // preferTurn => flip orientation, preferStraight => keep it
                    bool preferTurn = LabBias.Hit(rng, turnBias);
                    bool targetVertical = preferTurn ? !lastAcceptedVertical.Value : lastAcceptedVertical.Value;
                    int stop = Math.Min(edges.Count, index + 200);
                    for (int look = index; look < stop; look++)
                    {
                        var cand = edges[look];
                        bool candVertical = cand.x1 == cand.x2;
                        if (candVertical != targetVertical) continue;
                        int ca = CellIndex(cand.x1, cand.y1);
                        int cb = CellIndex(cand.x2, cand.y2);
                        if (Find(ca) != Find(cb))
                        {
                            var swap = edges[index];
                            edges[index] = cand;
                            edges[look] = swap;
                            break;
                        }
                    }
                }

                var e = edges[index];
                index++;

                int a = CellIndex(e.x1, e.y1);
                int b = CellIndex(e.x2, e.y2);
                if (Find(a) != Find(b))
                {
                    Union(a, b);
                    maze.grid[e.wx, e.wy] = 1;
                    lastAcceptedVertical = (e.x1 == e.x2);
                }
            }

            if (index >= edges.Count)
            {
                if (!tweaksDone)
                {
                    // Rooms/Braid change the style after generation. For testing TurnBias, set them to 0.
                    LabMazeTweaks.ApplyRooms(maze, rng, roomPerc);
                    LabMazeTweaks.ApplyBraid(maze, rng, braidPerc);
                    tweaksDone = true;
                }
                return false;
            }

            return true;
        }

        private int CellIndex(int gx, int gy)
        {
            int cx = (gx - 1) / 2;
            int cy = (gy - 1) / 2;
            return cy * cellsAcross + cx;
        }

        private int Find(int x)
        {
            if (parent[x] != x)
            {
                parent[x] = Find(parent[x]);
            }
            return parent[x];
        }

        private void Union(int a, int b)
        {
            int ra = Find(a);
            int rb = Find(b);
            if (ra == rb) return;
            if (rank[ra] < rank[rb])
            {
                parent[ra] = rb;
            }
            else if (rank[ra] > rank[rb])
            {
                parent[rb] = ra;
            }
            else
            {
                parent[rb] = ra;
                rank[ra]++;
            }
        }

        private void Shuffle(List<(int, int, int, int, int, int)> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }

    // Recursive Division: keep splitting the open area with walls + single gaps until regions are tiny
    // Style: clean rectangular rooms; weighted orientation + extra gaps stop heavy striping
    public class LabDivisionGenerator : ILabMazeGenerator
    {
        private Maze maze;
        private Random rng;
        private readonly Stack<(int x1, int y1, int x2, int y2)> regions = new Stack<(int, int, int, int)>();
        private int stepsPerTick = 4;
        private double braidPerc;
        private double roomPerc;
        private bool tweaksDone;
        private int turnBias;
        private bool? lastSplitHorizontal;
        public string Name => "Division";

        public void Initialise(Maze m, Random r, LabGenSettings settings)
        {
            maze = m;
            rng = r ?? new Random(Guid.NewGuid().GetHashCode()); // defensive so runs don't repeat if caller reuses a seed
            stepsPerTick = Math.Max(1, settings.StepsPerTick);
            braidPerc = settings.BraidPerc;
            roomPerc = settings.RoomPerc;
            tweaksDone = false;
            turnBias = settings.TurnBias;
            lastSplitHorizontal = null;
            regions.Clear();

            // clean grid: walls everywhere then open the interior box
            for (int y = 0; y < maze.height; y++)
            {
                for (int x = 0; x < maze.width; x++)
                {
                    maze.grid[x, y] = 0;
                }
            }
            maze.tokens?.Clear();

            for (int y = 1; y < maze.height - 1; y++)
            {
                for (int x = 1; x < maze.width - 1; x++)
                {
                    maze.grid[x, y] = 1;
                }
            }

            // push the whole interior region (inclusive bounds)
            regions.Push((1, 1, maze.width - 2, maze.height - 2));
        }

        public bool Step()
        {
            for (int i = 0; i < stepsPerTick; i++)
            {
                if (regions.Count == 0)
                {
                    if (!tweaksDone)
                    {
                        // Rooms/Braid change the style after generation. For testing TurnBias, set them to 0.
                        LabMazeTweaks.ApplyRooms(maze, rng, roomPerc);
                        LabMazeTweaks.ApplyBraid(maze, rng, braidPerc);
                        tweaksDone = true;
                    }
                    return false;
                }

                var region = regions.Pop();
                int w = region.x2 - region.x1;
                int h = region.y2 - region.y1;
                if (w < 2 || h < 2)
                {
                    continue;
                }

                bool horizontal = ChooseOrientationWeighted(w, h);

                if (horizontal)
                {
                    // split with a horizontal wall (even wall, odd gap)
                    int wallY = PickEvenBetween(region.y1 + 1, region.y2 - 1);
                    if (wallY <= region.y1 || wallY >= region.y2) continue;
                    var gaps = PickGaps(region.x1, region.x2);

                    for (int x = region.x1; x <= region.x2; x++)
                    {
                        bool isGap = gaps.Contains(x);
                        if (!isGap) maze.grid[x, wallY] = 0;
                    }

                    regions.Push((region.x1, region.y1, region.x2, wallY - 1));
                    regions.Push((region.x1, wallY + 1, region.x2, region.y2));
                }
                else
                {
                    // split with a vertical wall (even wall, odd gap)
                    int wallX = PickEvenBetween(region.x1 + 1, region.x2 - 1);
                    if (wallX <= region.x1 || wallX >= region.x2) continue;
                    var gaps = PickGaps(region.y1, region.y2);

                    for (int y = region.y1; y <= region.y2; y++)
                    {
                        bool isGap = gaps.Contains(y);
                        if (!isGap) maze.grid[wallX, y] = 0;
                    }

                    regions.Push((region.x1, region.y1, wallX - 1, region.y2));
                    regions.Push((wallX + 1, region.y1, region.x2, region.y2));
                }

                // remember what we actually placed
                if (horizontal)
                {
                    lastSplitHorizontal = true;
                }
                else
                {
                    lastSplitHorizontal = false;
                }
            }

            return regions.Count > 0;
        }

        // choose orientation using width/height bias, then turnBias nudges alternation
        private bool ChooseOrientationWeighted(int width, int height)
        {
            bool horizontal;
            double roll = rng.NextDouble();
            if (width > height) horizontal = roll < 0.30; // wider => usually vertical walls
            else if (height > width) horizontal = roll < 0.70; // taller => usually horizontal walls
            else horizontal = roll < 0.50;

            if (lastSplitHorizontal.HasValue)
            {
                bool preferTurn = LabBias.Hit(rng, turnBias);
                bool target = preferTurn ? !lastSplitHorizontal.Value : lastSplitHorizontal.Value;
                bool canH = height >= 2;
                bool canV = width >= 2;
                if ((target && canH) || (!target && canV))
                {
                    horizontal = target;
                }
            }
            return horizontal;
        }

        // keep walls/gaps inside the region and on the correct parity
        private int PickEvenBetween(int minInclusive, int maxInclusive)
        {
            var choices = new List<int>();
            for (int v = minInclusive; v <= maxInclusive; v++)
            {
                if (v % 2 == 0) choices.Add(v);
            }
            if (choices.Count == 0) return minInclusive;
            return choices[rng.Next(choices.Count)];
        }

        private int PickOddBetween(int minInclusive, int maxInclusive)
        {
            var choices = new List<int>();
            for (int v = minInclusive; v <= maxInclusive; v++)
            {
                if (v % 2 != 0) choices.Add(v);
            }
            if (choices.Count == 0) return minInclusive;
            return choices[rng.Next(choices.Count)];
        }

        // pick one or two spaced gaps (odd coords) to stop stripey walls
        private List<int> PickGaps(int minInclusive, int maxInclusive)
        {
            var odds = new List<int>();
            for (int v = minInclusive; v <= maxInclusive; v++)
            {
                if (v % 2 != 0) odds.Add(v);
            }
            if (odds.Count == 0)
            {
                return new List<int>() { minInclusive };
            }

            var gaps = new List<int>();
            int first = odds[rng.Next(odds.Count)];
            gaps.Add(first);

            int span = maxInclusive - minInclusive + 1;
            bool allowSecond = span >= 9 && rng.Next(100) < 30;
            if (allowSecond && odds.Count > 1)
            {
                var spaced = new List<int>();
                for (int i = 0; i < odds.Count; i++)
                {
                    if (Math.Abs(odds[i] - first) >= 2)
                    {
                        spaced.Add(odds[i]);
                    }
                }
                if (spaced.Count > 0)
                {
                    gaps.Add(spaced[rng.Next(spaced.Count)]);
                }
            }
            return gaps;
        }
    }

    // ---------------- solvers ----------------

    public abstract class LabSolverBase : ILabMazeSolver
    {
        protected Maze maze;
        protected Point start;
        protected Point exit;
        protected bool[,] explored;
        protected bool[,] path;
        protected bool found;
        protected int exploredCount;
        protected int pathLength;

        public int VisitedCount => exploredCount;
        public int PathLength => pathLength;
        public bool FoundPath => found;
        public abstract string Name { get; }

        public virtual void Initialise(Maze m, Point s, Point e, bool[,] exploredMask, bool[,] pathMask)
        {
            maze = m;
            start = s;
            exit = e;
            explored = exploredMask;
            path = pathMask;
            found = false;
            exploredCount = 0;
            pathLength = -1;
            ClearMasks();
        }

        protected void MarkPath(Dictionary<(int, int), (int, int)> parent)
        {
            if (!found) return;
            var cur = (exit.X, exit.Y);
            int len = 0;
            while (parent.ContainsKey(cur))
            {
                path[cur.Item1, cur.Item2] = true;
                cur = parent[cur];
                len++;
            }
            path[start.X, start.Y] = true;
            pathLength = len;
        }

        public abstract bool Step();

        private void ClearMasks()
        {
            if (explored != null)
            {
                for (int y = 0; y < maze.height; y++)
                {
                    for (int x = 0; x < maze.width; x++)
                    {
                        explored[x, y] = false;
                    }
                }
            }
            if (path != null)
            {
                for (int y = 0; y < maze.height; y++)
                {
                    for (int x = 0; x < maze.width; x++)
                    {
                        path[x, y] = false;
                    }
                }
            }
        }
    }

    public class LabBfsSolver : LabSolverBase
    {
        private Queue<Point> q = new Queue<Point>();
        private Dictionary<(int, int), (int, int)> parent = new Dictionary<(int, int), (int, int)>();
        public override string Name => "BFS";

        public override void Initialise(Maze m, Point s, Point e, bool[,] exploredMask, bool[,] pathMask)
        {
            base.Initialise(m, s, e, exploredMask, pathMask);
            q.Clear();
            parent.Clear();
            q.Enqueue(start);
            explored[start.X, start.Y] = true;
            exploredCount = 1;
        }

        public override bool Step()
        {
            if (q.Count == 0)
            {
                return false;
            }

            var p = q.Dequeue();
            if (p.X == exit.X && p.Y == exit.Y)
            {
                found = true;
                MarkPath(parent);
                return false;
            }

            foreach (var dir in LabDirs.Four)
            {
                int nx = p.X + dir.dx;
                int ny = p.Y + dir.dy;
                if (nx < 0 || ny < 0 || nx >= maze.width || ny >= maze.height)
                {
                    continue;
                }
                if (!maze.IsPath(nx, ny))
                {
                    continue;
                }
                if (explored[nx, ny])
                {
                    continue;
                }
                explored[nx, ny] = true;
                exploredCount++;
                parent[(nx, ny)] = (p.X, p.Y);
                q.Enqueue(new Point(nx, ny));
            }

            return q.Count > 0;
        }
    }

    public class LabDfsSolver : LabSolverBase
    {
        private Stack<Point> stack = new Stack<Point>();
        private Dictionary<(int, int), (int, int)> parent = new Dictionary<(int, int), (int, int)>();
        public override string Name => "DFS";

        public override void Initialise(Maze m, Point s, Point e, bool[,] exploredMask, bool[,] pathMask)
        {
            base.Initialise(m, s, e, exploredMask, pathMask);
            stack.Clear();
            parent.Clear();
            stack.Push(start);
            explored[start.X, start.Y] = true;
            exploredCount = 1;
        }

        public override bool Step()
        {
            if (stack.Count == 0)
            {
                return false;
            }

            var p = stack.Pop();
            if (p.X == exit.X && p.Y == exit.Y)
            {
                found = true;
                MarkPath(parent);
                return false;
            }

            foreach (var dir in LabDirs.Four)
            {
                int nx = p.X + dir.dx;
                int ny = p.Y + dir.dy;
                if (nx < 0 || ny < 0 || nx >= maze.width || ny >= maze.height)
                {
                    continue;
                }
                if (!maze.IsPath(nx, ny))
                {
                    continue;
                }
                if (explored[nx, ny])
                {
                    continue;
                }
                explored[nx, ny] = true;
                exploredCount++;
                parent[(nx, ny)] = (p.X, p.Y);
                stack.Push(new Point(nx, ny));
            }

            return stack.Count > 0;
        }
    }

    public class LabGreedySolver : LabSolverBase
    {
        private PriorityQueue<Point, double> pq = new PriorityQueue<Point, double>();
        private Dictionary<(int, int), (int, int)> parent = new Dictionary<(int, int), (int, int)>();
        public override string Name => "Greedy";

        public override void Initialise(Maze m, Point s, Point e, bool[,] exploredMask, bool[,] pathMask)
        {
            base.Initialise(m, s, e, exploredMask, pathMask);
            pq = new PriorityQueue<Point, double>();
            parent.Clear();
            explored[start.X, start.Y] = true;
            exploredCount = 1;
            pq.Enqueue(start, Heuristic(start));
        }

        public override bool Step()
        {
            if (pq.Count == 0)
            {
                return false;
            }

            pq.TryDequeue(out Point p, out _);
            if (p.X == exit.X && p.Y == exit.Y)
            {
                found = true;
                MarkPath(parent);
                return false;
            }

            foreach (var dir in LabDirs.Four)
            {
                int nx = p.X + dir.dx;
                int ny = p.Y + dir.dy;
                if (nx < 0 || ny < 0 || nx >= maze.width || ny >= maze.height)
                {
                    continue;
                }
                if (!maze.IsPath(nx, ny))
                {
                    continue;
                }
                if (explored[nx, ny])
                {
                    continue;
                }
                explored[nx, ny] = true;
                exploredCount++;
                parent[(nx, ny)] = (p.X, p.Y);
                pq.Enqueue(new Point(nx, ny), Heuristic(new Point(nx, ny)));
            }

            return pq.Count > 0;
        }

        private double Heuristic(Point p)
        {
            return Math.Abs(p.X - exit.X) + Math.Abs(p.Y - exit.Y);
        }
    }

    public class LabAStarSolver : LabSolverBase
    {
        private PriorityQueue<(Point p, double g), double> pq = new PriorityQueue<(Point, double), double>();
        private Dictionary<(int, int), (int, int)> parent = new Dictionary<(int, int), (int, int)>();
        private Dictionary<(int, int), double> gScore = new Dictionary<(int, int), double>();
        public override string Name => "A*";

        public override void Initialise(Maze m, Point s, Point e, bool[,] exploredMask, bool[,] pathMask)
        {
            base.Initialise(m, s, e, exploredMask, pathMask);
            pq = new PriorityQueue<(Point, double), double>();
            parent.Clear();
            gScore.Clear();
            explored[start.X, start.Y] = true;
            exploredCount = 1;
            gScore[(start.X, start.Y)] = 0.0;
            pq.Enqueue((start, 0.0), Heuristic(start));
        }

        public override bool Step()
        {
            if (pq.Count == 0)
            {
                return false;
            }

            pq.TryDequeue(out var item, out _);
            Point p = item.p;
            if (!gScore.TryGetValue((p.X, p.Y), out double gHere))
            {
                return pq.Count > 0;
            }
            if (p.X == exit.X && p.Y == exit.Y)
            {
                found = true;
                MarkPath(parent);
                return false;
            }

            foreach (var dir in LabDirs.Four)
            {
                int nx = p.X + dir.dx;
                int ny = p.Y + dir.dy;
                if (nx < 0 || ny < 0 || nx >= maze.width || ny >= maze.height)
                {
                    continue;
                }
                if (!maze.IsPath(nx, ny))
                {
                    continue;
                }

                double tentative = gHere + 1.0;
                bool better = (!gScore.ContainsKey((nx, ny)) || tentative < gScore[(nx, ny)]);
                if (better)
                {
                    gScore[(nx, ny)] = tentative;
                    double f = tentative + Heuristic(new Point(nx, ny));
                    parent[(nx, ny)] = (p.X, p.Y);
                    if (!explored[nx, ny])
                    {
                        explored[nx, ny] = true;
                        exploredCount++;
                    }
                    pq.Enqueue((new Point(nx, ny), tentative), f);
                }
            }

            return pq.Count > 0;
        }

        private double Heuristic(Point p)
        {
            return Math.Abs(p.X - exit.X) + Math.Abs(p.Y - exit.Y);
        }
    }
}
