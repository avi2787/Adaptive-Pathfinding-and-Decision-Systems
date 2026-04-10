using System;
using System.Collections.Generic;
using System.Drawing;

namespace NEA_ai_pathfinding;

// This class builds the maze grid and tokens for the game.
public class Maze
{
    // total grid dimensions, including both walls and paths
    public int width {get;}
    public int height {get;}
    public int Level { get; set; } = 1;

    // grid[x,y] == 0 means wall, 1 means open path, 2 is used as a temporary marker
    public int[,] grid {get;}

    // list of tokens dropped into the maze, with their position and score value
    public List<(int x, int y, int value)> tokens { get; } = new List<(int x, int y, int value)>();

    // starting cell for the runner on the left edge
    public Point start {get; private set;} = new Point(1, 1);

    // exit cell, normally on the right edge; picked in Generate()
    public Point exit {get; private set;}

    // single random number generator for the maze
    private Random randn = new Random();

    // fields used for step-by-step carving so the maze can grow over time
    // these are used when I want the maze to build slowly on screen
    private class StepState
    {
        public bool StepModeOn;
        public List<(int x, int y)> Frontier = new List<(int x, int y)>();
        public bool CarvingDone;
        public bool PocketsDone;
        public bool BraidDone;
        public bool TokensDone;
        public int StepsPerCall = 12;
        public bool StartExitPicked;
        public double BraidProbability = 0.05;
        public double RoomDensity = 0.08;
        public int TargetTokenCount;
    }

    private StepState step = new StepState();

    public Maze(int width, int height)
    {
        // clamp input so the maze is never smaller than 5x5
        this.width = Math.Max(5, width);
        this.height = Math.Max(5, height);

        // dimensions odd so cell-wall-cell line up nicely and the path doesnt end up on the edge

        if (this.width % 2 == 0)
            this.width= this.width +1;
        if (this.height % 2 == 0)
            this.height = this.height +1;

        // initialise grid and a default exit position

        grid = new int[this.width, this.height];
        exit = new Point(this.width - 2, this.height - 2);
        ResetStepState(); // make sure step-by-step flags start clean
    }

    public void ResetStepState()
    {
        // set all step-mode flags back to their defaults so a new maze has no leftovers
        step = new StepState();
    }

    private int RollTokenValue()
    {
        // simple weighted roll for token score value
        int roll = randn.Next(100);
        if (roll < 50)
        {
            return 1;
        }
        if (roll < 80)
        {
            return 3;
        }
        return 5;
    }

    private bool HasTokenAt(int x, int y)
    {
        foreach (var t in tokens)
        {
            if (t.x == x && t.y == y)
            {
                return true;
            }
        }
        return false;
    }

    public void Generate(double braidProbability = 0.05, double roomDensity = 0.08)
    {
        // reset step flags because this is the classic instant generator
        ResetStepState();

        // fill the grid with walls everywhere to start with

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[x, y] = 0;
            }
                
        }

        // choose a random starting row on the left and make sure it is odd,
        // so it lines up with the carving pattern of wall-cell-wall
        start = new Point(1, make_odd(randn.Next(1, height - 1)));

        GeneratePrim();

        PickStartAndExit();

        // add small pocket rooms that expand from existing corridors (keeps connectivity)

        if (roomDensity > 0)
            AddPockets(roomDensity);

        if (braidProbability > 0)
            Braid(braidProbability); // braid removes some dead ends to make loops

        PlaceTokens();
    }

    // prepare for step-by-step generation (maze grows over time)
    public void StartStepGeneration(double braidProbability = 0.05, double roomDensity = 0.08, int stepsPerCall = 12)
    {
        // reset any old state
        ResetStepState();

        step.BraidProbability = braidProbability;
        step.RoomDensity = roomDensity;
        step.StepsPerCall = Math.Max(1, stepsPerCall);

        // fill grid with walls ready to carve
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[x, y] = 0;
            }
        }

        // pick fixed endpoints up front so animation does not shuffle them
        start = new Point(1, make_odd(randn.Next(1, height - 1)));
        exit = new Point(width - 2, make_odd(randn.Next(1, height - 1)));

        grid[start.X, start.Y] = 1;
        grid[exit.X, exit.Y] = 1;            // pre-open exit so it stays put
        if (exit.X - 1 >= 0) grid[exit.X - 1, exit.Y] = 1; // small bridge so path can reach exit

        // set up frontier list for Prim carving
        AddFrontierCell(start.X + 2, start.Y);
        AddFrontierCell(start.X, start.Y + 2);
        AddFrontierCell(start.X, start.Y - 2);

        // target number of tokens to slowly drop while carving
        int area = width * height;
        int approxTokens = area / 45; // simple formula so bigger mazes get more tokens
        if (approxTokens < 2)
        {
            approxTokens = 2;
        }
        if (approxTokens > 30)
        {
            approxTokens = 30;
        }
        step.TargetTokenCount = CalculateTargetTokens(approxTokens);
        tokens.Clear();

        step.StartExitPicked = true; // keep chosen endpoints; skip later re-pick
        step.StepModeOn = true;
    }

    // one step of the slow generator; returns true while still working
    public bool StepGenerate()
    {
        if (!step.StepModeOn)
        {
            return false;
        }

        // keep carving small chunks each call (phase 1)
        if (!step.CarvingDone)
        {
            DoPrimChunk(); // carve a handful of Prim steps so the maze grows on screen
            if (step.Frontier.Count == 0)
            {
                // clean up any frontier markers left over
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (grid[x, y] == 2)
                        {
                            grid[x, y] = 0;
                        }
                    }
                }
                step.CarvingDone = true;
            }

            if (step.Frontier.Count > 0)
            {
                // still carving, so keep ticking
                return true;
            }
        }

        // pick start and exit based on carved paths (phase 2)
        if (!step.StartExitPicked)
        {
            PickStartAndExit();
            step.StartExitPicked = true;
            return true;
        }

        // add pockets in one go after carving (phase 3)
        if (!step.PocketsDone)
        {
            if (step.RoomDensity > 0)
            {
                AddPockets(step.RoomDensity);
            }
            step.PocketsDone = true;
            return true;
        }

        // braid dead ends if wanted (phase 4)
        if (!step.BraidDone)
        {
            if (step.BraidProbability > 0)
            {
                Braid(step.BraidProbability);
            }
            step.BraidDone = true;
            return true;
        }

        // place any missing tokens once carving and pockets are finished (phase 5)
        if (!step.TokensDone)
        {
            PlaceRemainingTokens();
            step.TokensDone = true;
            step.StepModeOn = false;
            return false;
        }

        step.StepModeOn = false;
        return false;
    }

    public bool IsPath(int x, int y)
    {

        // true if (x,y) is inside the grid AND that cell is open (1)
        // used by the player and AI to see if they can move into a cell

        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            return (false);
        }
        else
        {
            return grid[x, y] == 1;
        }
            

            

    }

    public bool IsExit(int x, int y)
    {
        if (x == exit.X && y == exit.Y)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    // randomized prims algorithm for denser mazes
    private void GeneratePrim()
    {
        List<(int x, int y)> frontier = new List<(int x, int y)>();

        void AddFrontier(int x, int y)
        {
            if (x > 0 && y > 0 && x < width - 1 && y < height - 1 && grid[x, y] == 0)
            {
                grid[x, y] = 2; // mark as part of frontier set
                frontier.Add((x, y));
            }
        }

        grid[start.X, start.Y] = 1;
        AddFrontier(start.X + 2, start.Y);
        AddFrontier(start.X - 2, start.Y);
        AddFrontier(start.X, start.Y + 2);
        AddFrontier(start.X, start.Y - 2);

        // pick random frontier cells until none left
        while (frontier.Count > 0)
        {
            int frontier_index = randn.Next(frontier.Count);
            var (frontier_x, frontier_y) = frontier[frontier_index];
            frontier.RemoveAt(frontier_index);

            var neighbors = new List<(int x, int y, int wall_x, int wall_y)>();
            foreach (var (stepin_x, stepin_y) in new (int stepin_x, int stepin_y)[] {(2,0), (-2,0), (0,2), (0,-2)})
            {
                int neighbour_x = frontier_x + stepin_x;          
                int neighbour_y = frontier_y + stepin_y;

                int wall_x = frontier_x + stepin_x / 2;
                int wall_y = frontier_y + stepin_y / 2;

                if (neighbour_x > 0 && neighbour_y > 0 && neighbour_x < width - 1 && neighbour_y < height - 1 && grid[neighbour_x, neighbour_y] == 1)
                {
                    neighbors.Add((neighbour_x, neighbour_y, wall_x, wall_y));
                }
                    
            }

            if (neighbors.Count > 0) // choose a random neighbour cell
            {
                (int x, int y, int wall_x, int wall_y) chosen = neighbors[randn.Next(neighbors.Count)];
                grid[frontier_x, frontier_y] = 1;
                grid[chosen.wall_x, chosen.wall_y] = 1;

                foreach (var (stepin_x, stepin_y) in new (int stepin_x, int stepin_y)[] { (2,0), (-2,0), (0,2), (0,-2) })
                {
                    AddFrontier(frontier_x + stepin_x, frontier_y + stepin_y);
                }
                    
            }
        }

        // convert any remaining frontier markers to walls

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == 2)
                {
                    grid[x, y] = 0;
                }  
            }     
        }      
    }

    private void AddPockets(double density)
    {
        // choose random carved cells and expand a small 3x3 pocket around them
        // this creates little rooms off the corridors to make the maze feel less linear

        int pockets = Math.Max(1, (int)(width * height * density / 30));
        for (int i = 0; i < pockets; i++)
        {
            // find a random carved cell not on the border

            int attempts = 0;
            int pocketcenter_x = 3;
            int pocketcenter_y = 3;

            while ((grid[pocketcenter_x, pocketcenter_y] == 0) && attempts < 50)
            {
                pocketcenter_x = randn.Next(2, width - 2);
                pocketcenter_y = randn.Next(2, height - 2);
                attempts = attempts +1;
            } 

            for (int y = pocketcenter_y - 1; y <= pocketcenter_y + 1; y++)
            {
                for (int x = pocketcenter_x - 1; x <= pocketcenter_x + 1; x++)
                {
                    if (x > 0 && y > 0 && x < width - 1 && y < height - 1)
                    {
                        grid[x, y] = 1;
                    }
                }     
            }    
        }
    }

    private static int make_odd(int value)
    {
        if (value % 2 == 0)
        {
            value = value+1;
        }
        return value;
    }


    // Add occasional extra connections to create loops and confusion

    private void Braid(double prob)
    {
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                if (grid[x, y] != 1)
                    continue;
                int openNeighbors = 0;
                var walls = new List<(int wall_x, int wall_y)>();
                foreach (var (stepin_x, stepin_y) in new (int stepin_x, int stepin_y)[] { (1,0), (-1,0), (0,1), (0,-1) })
                {
                    int neighbour_x = x + stepin_x;
                    int neighbour_y = y + stepin_y;

                    if (grid[neighbour_x, neighbour_y] == 1)
                    {
                        openNeighbors = openNeighbors + 1;
                    }
                    else
                    {
                        walls.Add((neighbour_x,neighbour_y));
                    }
                }

                // dead end is openNeighbors == 1; occasionally remove a wall to add a loop

                if (openNeighbors == 1 && randn.NextDouble() < prob && walls.Count > 0)
                {
                    var (wall_x, wall_y) = walls[randn.Next(walls.Count)];

                    // ensure carving into a wall doesn't break bounds

                    if (wall_x > 0 && wall_y > 0 && wall_x < width - 1 && wall_y < height - 1)
                    {
                        grid[wall_x, wall_y] = 1;
                    }
                        
                }
            }
        }
    }

    private void PlaceTokens()
    {
        tokens.Clear();

        List<(int x, int y)> candidates = new List<(int x, int y)>();

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                if (grid[x, y] == 1)
                {
                    if (!(x == start.X && y == start.Y) && !(x == exit.X && y == exit.Y))
                    {
                        candidates.Add((x, y));
                    }
                }
            }
        }

        if (candidates.Count == 0)
        {
            return;
        }

        int area = width * height;
        int approxTokens = area / 45;

        if (approxTokens < 2)
        {
            approxTokens = 2;
        }

        if (approxTokens > 30)
        {
            approxTokens = 30;
        }

        int maxTokens = candidates.Count;
        int targetTokens = Math.Min(CalculateTargetTokens(approxTokens), maxTokens);

        ShufflePositions(candidates);

        for (int i = 0; i < targetTokens; i++)
        {
            var (tx, ty) = candidates[i];

            int tokenValue = RollTokenValue();
            tokens.Add((tx, ty, tokenValue));
        }
    }

    private void ShufflePositions(List<(int x, int y)> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = randn.Next(i + 1);
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void Shuffle((int dx, int dy)[] arr)
    {
        //shuffle to randomise the order of directions
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = randn.Next(i + 1);
            var temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }
    }

    private void AddFrontierCell(int x, int y)
    {
        // add a frontier spot if it is inside the maze and still a wall
        if (x > 0 && y > 0 && x < width - 1 && y < height - 1)
        {
            if (grid[x, y] == 0)
            {
                grid[x, y] = 2; // mark as frontier so we do not double add
                step.Frontier.Add((x, y));
            }
        }
    }

    private void DoPrimChunk()
    {
        // carve a small batch of cells so the maze grows slowly on screen
        int stepsPerCall = step.StepsPerCall;
        for (int stepIndex = 0; stepIndex < stepsPerCall; stepIndex++)
        {
            if (step.Frontier.Count == 0)
            {
                return;
            }

            int pickIndex = randn.Next(step.Frontier.Count);
            var frontier = step.Frontier[pickIndex];
            step.Frontier.RemoveAt(pickIndex);

            List<(int x, int y, int wall_x, int wall_y)> neighbors = new List<(int x, int y, int wall_x, int wall_y)>();

            foreach (var (stepin_x, stepin_y) in new (int stepin_x, int stepin_y)[] { (2, 0), (-2, 0), (0, 2), (0, -2) })
            {
                int neighbour_x = frontier.x + stepin_x;
                int neighbour_y = frontier.y + stepin_y;
                int wall_x = frontier.x + stepin_x / 2;
                int wall_y = frontier.y + stepin_y / 2;

                // only connect to already carved cells
                if (neighbour_x > 0 && neighbour_y > 0 && neighbour_x < width - 1 && neighbour_y < height - 1)
                {
                    if (grid[neighbour_x, neighbour_y] == 1)
                    {
                        neighbors.Add((neighbour_x, neighbour_y, wall_x, wall_y));
                    }
                }
            }

            if (neighbors.Count > 0)
            {
                var chosen = neighbors[randn.Next(neighbors.Count)];

                // open the frontier cell and the wall between
                grid[frontier.x, frontier.y] = 1;
                grid[chosen.wall_x, chosen.wall_y] = 1;

                // add new frontier neighbours now that this cell is open
                foreach (var (stepin_x, stepin_y) in new (int stepin_x, int stepin_y)[] { (2, 0), (-2, 0), (0, 2), (0, -2) })
                {
                    AddFrontierCell(frontier.x + stepin_x, frontier.y + stepin_y);
                }
            }
        }
    }

    private void PickStartAndExit()
    {
        // pick start on left edge where a path exists
        List<int> startCandidates = new List<int>();
        for (int y = 1; y < height - 1; y++)
        {
            if (grid[1, y] == 1)
            {
                startCandidates.Add(y);
            }
        }

        if (startCandidates.Count > 0)
        {
            start = new Point(1, startCandidates[randn.Next(startCandidates.Count)]);
        }
        else
        {
            grid[start.X, start.Y] = 1; // keep original start open
        }

        // pick exit on right edge where there is a path at x = width - 3
        List<int> exitCandidates = new List<int>();
        for (int y = 1; y < height - 1; y++)
        {
            if (grid[width - 3, y] == 1)
            {
                exitCandidates.Add(y);
            }
        }

        if (exitCandidates.Count > 0)
        {
            int exitRow = exitCandidates[randn.Next(exitCandidates.Count)];
            exit = new Point(width - 2, exitRow);
            grid[exit.X, exit.Y] = 1;
        }
        else
        {
            exit = new Point(width - 2, make_odd(randn.Next(1, height - 1)));
            grid[exit.X, exit.Y] = 1;
            grid[exit.X - 1, exit.Y] = 1;
        }
    }

    private void PlaceRemainingTokens()
    {
        // top up tokens so we reach the target count after carving finishes
        List<(int x, int y)> candidates = new List<(int x, int y)>();
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                if (grid[x, y] == 1)
                {
                    if (!(x == start.X && y == start.Y) && !(x == exit.X && y == exit.Y))
                    {
                        candidates.Add((x, y));
                    }
                }
            }
        }

        ShufflePositions(candidates);

        foreach (var (x, y) in candidates)
        {
            if (tokens.Count >= step.TargetTokenCount)
            {
                break;
            }

            // skip if a token is already on this cell
            if (HasTokenAt(x, y))
            {
                continue;
            }

            int tokenValue = RollTokenValue();
            tokens.Add((x, y, tokenValue));
        }
    }

    private int CalculateTargetTokens(int baseTokens)
    {
        int lvl = Level < 1 ? 1 : Level;
        double factor = 1.0 + 0.25 * (lvl - 1); // simple scale so higher levels get more coins
        int boosted = (int)Math.Round(baseTokens * factor);
        if (boosted < 2) boosted = 2;
        return boosted;
    }

    public void SetStart(Point p)
    {
        if (p.X <= 0 || p.Y <= 0 || p.X >= width - 1 || p.Y >= height - 1)
        {
            return;
        }
        grid[p.X, p.Y] = 1;
        start = p;
    }

    public void SetExit(Point p)
    {
        if (p.X <= 0 || p.Y <= 0 || p.X >= width - 1 || p.Y >= height - 1)
        {
            return;
        }
        grid[p.X, p.Y] = 1;
        exit = p;
    }

    public int ComputeShortestPathLength(Point startPoint, Point goalPoint)
    {
        if (!IsPath(startPoint.X, startPoint.Y) || !IsPath(goalPoint.X, goalPoint.Y))
        {
            return -1;
        }

        int[,] dist = new int[width, height];
        for (int yy = 0; yy < height; yy++)
        {
            for (int xx = 0; xx < width; xx++)
            {
                dist[xx, yy] = -1;
            }
        }

        Queue<Point> q = new Queue<Point>();
        q.Enqueue(startPoint);
        dist[startPoint.X, startPoint.Y] = 0;
        int[] dx = new int[] { 1, -1, 0, 0 };
        int[] dy = new int[] { 0, 0, 1, -1 };

        while (q.Count > 0)
        {
            Point p = q.Dequeue();
            if (p.X == goalPoint.X && p.Y == goalPoint.Y)
            {
                return dist[p.X, p.Y];
            }

            for (int i = 0; i < 4; i++)
            {
                int nx = p.X + dx[i];
                int ny = p.Y + dy[i];
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                {
                    continue;
                }
                if (!IsPath(nx, ny))
                {
                    continue;
                }
                if (dist[nx, ny] != -1)
                {
                    continue;
                }
                dist[nx, ny] = dist[p.X, p.Y] + 1;
                q.Enqueue(new Point(nx, ny));
            }
        }

        return -1;
    }

    // shared BFS so other classes can reuse a single maze walk
    public bool TryBuildPath(Point startPoint, Point goalPoint, out List<Point> pathCells)
    {
        pathCells = new List<Point>();
        if (!IsPath(startPoint.X, startPoint.Y) || !IsPath(goalPoint.X, goalPoint.Y))
        {
            return false;
        }

        int[,] distance = new int[width, height];
        Point[,] parent = new Point[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                distance[x, y] = -1;
                parent[x, y] = new Point(-1, -1);
            }
        }

        Queue<Point> queue = new Queue<Point>();
        queue.Enqueue(startPoint);
        distance[startPoint.X, startPoint.Y] = 0;

        int[] moveX = new int[] { 1, -1, 0, 0 };
        int[] moveY = new int[] { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            Point current = queue.Dequeue();
            if (current.X == goalPoint.X && current.Y == goalPoint.Y)
            {
                // rebuild path from goal back to start
                Point walk = goalPoint;
                while (walk.X != -1 && walk.Y != -1)
                {
                    pathCells.Add(walk);
                    Point prev = parent[walk.X, walk.Y];
                    walk = prev;
                }
                pathCells.Reverse();
                return true;
            }

            for (int i = 0; i < 4; i++)
            {
                int nextX = current.X + moveX[i];
                int nextY = current.Y + moveY[i];
                if (nextX < 0 || nextY < 0 || nextX >= width || nextY >= height)
                {
                    continue;
                }
                if (!IsPath(nextX, nextY))
                {
                    continue;
                }
                if (distance[nextX, nextY] != -1)
                {
                    continue;
                }

                distance[nextX, nextY] = distance[current.X, current.Y] + 1;
                parent[nextX, nextY] = current;
                queue.Enqueue(new Point(nextX, nextY));
            }
        }

        return false;
    }

    // quick reachability check so start can reach exit
    public bool IsSolvable()
    {
        if (start.X < 0 || start.Y < 0 || exit.X < 0 || exit.Y < 0)
        {
            return false;
        }

        if (!IsPath(start.X, start.Y) || !IsPath(exit.X, exit.Y))
        {
            return false;
        }

        if (TryBuildPath(start, exit, out List<Point> pathCells))
        {
            return pathCells != null && pathCells.Count > 0;
        }

        return false;
    }
}
