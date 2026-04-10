using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace NEA_ai_pathfinding
{

    // Q-learning runner: learns in-session only (no DB), used by blocker mode and lab agent
 
    internal enum RunnerAction
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight
    }

    internal class RunnerState
    {
        public int X;
        public int Y;

        public RunnerState(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            // check if the other object is a RunnerState and compare positions

            RunnerState other = obj as RunnerState;
            if (other == null)
            {
                return false;
            }

            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            // keep hash simple so dictionary lookups work (no fancy helpers)
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                return hash;
            }
        }
    }

    internal class RunnerStateAction
    {
        public RunnerState State;
        public RunnerAction Action;

        public RunnerStateAction(RunnerState state, RunnerAction action)
        {
            State = state;
            Action = action;
        }

        public override bool Equals(object obj)
        {
            // check if the other object is a RunnerStateAction and compare both parts

            RunnerStateAction other = obj as RunnerStateAction;
            if (other == null)
            {
                return false;
            }

            return State.Equals(other.State) && Action == other.Action;
        }

        public override int GetHashCode()
        {
            // simple hash so we can store State+Action pairs in a dictionary
            unchecked
            {
                int hash = 23;
                hash = hash * 31 + State.GetHashCode();
                hash = hash * 31 + (int)Action;
                return hash;
            }
        }
    }

    internal class RunnerQAgent
    {
        // the maze instance the agent is currently exploring

        private Maze attachedMaze;

        // agent's internal (x,y) position
        private int currentX;
        private int currentY;

        // Q-value table: State+Action -> score
        private readonly Dictionary<RunnerStateAction, double> qValues = new Dictionary<RunnerStateAction, double>();

        // learning hyperparameters - small by design so learning looks gradual on screen
        private double learningRate = 0.06;      // lower means it needs more tries to learn
        private double discountFactor = 0.75;    // lower means it cares more about short term than future
        private double explorationRate = 0.35;   // legacy default, replaced by epsilon per episode
        private double greedyBias = 0.25;        // chance to try the move that looks closest to exit

        private readonly Random random = new Random();
        private const double UnreachableDistance = 999999.0;
        private int[,] distToExit; // cache of exit distances so reward check is cheaper

        // hint path from simple A* (token biased)
        private List<Point> hintPath = new List<Point>();
        private Dictionary<string, Point> hintNextStep = new Dictionary<string, Point>();
        private int hintCounter = 0; // how many steps until we rebuild the hint
        private int episodeCount = 0;
        private double epsilon = 0.35; // exploration rate per episode
        private double alpha = 0.06;   // learning rate per episode
        private List<Point> dangerCells = new List<Point>();

        public void AttachMaze(Maze maze, Point startPosition)
        {
            attachedMaze = maze;
            currentX = startPosition.X;
            currentY = startPosition.Y;
            epsilon = explorationRate;
            alpha = learningRate;

            // build a gentle hint path and seed a tiny bit of Q so the AI has a clue
            BuildHintPath(startPosition, true);
            BuildDistanceMapFromExit(); // cache exits so reward uses a quick lookup
        }

        public void ConfigureForLevel(int levelNumber)
        {
            // nudge learning/exploration depending on level
            int lvl = levelNumber;
            if (lvl < 1) lvl = 1;
            if (lvl > 5) lvl = 5; // tuning aimed at the first 5 challenge levels

            // early levels a bit smarter; later levels ease off to slow the AI down
            double[] lvlExploration = { 0.45, 0.38, 0.32, 0.26, 0.20 };
            double[] lvlLearning =    { 0.08, 0.08, 0.07, 0.06, 0.05 };
            double[] lvlDiscount =    { 0.70, 0.72, 0.74, 0.76, 0.78 };
            double[] lvlGreedy =      { 0.26, 0.30, 0.32, 0.34, 0.36 };

            explorationRate = lvlExploration[lvl - 1];
            learningRate = lvlLearning[lvl - 1];
            discountFactor = lvlDiscount[lvl - 1];
            greedyBias = lvlGreedy[lvl - 1];

            // keep episode copies in sync so StartNewEpisode keeps using the tuned values
            epsilon = explorationRate;
            alpha = learningRate;
        }

        public void ConfigureCustom(double learning, double discount, double exploration, double greedy)
        {
            // clamp so sliders cannot break the agent
            learningRate = Clamp(learning, 0.01, 0.30);
            discountFactor = Clamp(discount, 0.30, 0.99);
            explorationRate = Clamp(exploration, 0.01, 0.80);
            greedyBias = Clamp(greedy, 0.01, 0.80);
        }

        public void SetLearningRate(double learning)
        {
            // tiny helper so caller can slow the learner for fairness
            learningRate = Clamp(learning, 0.01, 0.30);
            alpha = learningRate;
        }

        public void SetDangerCells(List<Point> dangers)
        {
            if (dangers == null)
            {
                dangerCells = new List<Point>();
                return;
            }
            dangerCells = new List<Point>(dangers);
        }

        public void StartNewEpisode(Point newStart)
        {
            episodeCount++;
            // Early episodes are more random (learning the maze), later ones trust Q-table more
            epsilon = Math.Max(0.05, 1.0 - episodeCount * 0.02);
            // Learning rate eases off so it stabilises after many tries
            alpha = Math.Max(0.05, 0.6 * Math.Pow(0.97, episodeCount));
            learningRate = alpha;
            currentX = newStart.X;
            currentY = newStart.Y;
        }

        private double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

        public RunnerAction ChooseAction()
        {
            // epsilon-greedy: sometimes pick a random move to explore, but only valid cells
            List<RunnerAction> validActions = GetValidActionsFrom(currentX, currentY); // valid only so we stop wall-bashing
            if (validActions.Count == 0)
            {
                return RunnerAction.MoveUp; // fail-safe if boxed in
            }

            double exploreChance = epsilon > 0 ? epsilon : explorationRate;
            if (random.NextDouble() < exploreChance)
            {
                int index = random.Next(validActions.Count);
                return validActions[index];
            }

            RunnerState state = new RunnerState(currentX, currentY);
            RunnerAction bestAction = validActions[0];
            double bestValue = double.NegativeInfinity;

            foreach (RunnerAction action in validActions)
            {
                RunnerStateAction stateAction = new RunnerStateAction(state, action);
                double value = 0.0;
                if (qValues.TryGetValue(stateAction, out double existing))
                {
                    value = existing;
                }

                if (value > bestValue)
                {
                    bestValue = value;
                    bestAction = action;
                }
            }

            return bestAction;
        }

        public void PerformLearningStep()
        {
            // one learning step: pick a move, update position, then update the Q-value
            if (attachedMaze == null)
            {
                return;
            }

            RunnerState oldState = new RunnerState(currentX, currentY);

            RunnerAction action = ChooseAction();

            RunnerAction greedyAction;
            if (random.NextDouble() < greedyBias && TryGreedyAction(out greedyAction))
            {
                action = greedyAction;
            }

            // work out the target cell for the action we picked
            int nextX;
            int nextY;
            GetNextForAction(currentX, currentY, action, out nextX, out nextY); // helper keeps movement logic in one spot
            bool hitWall = false;

            if (IsInsideMaze(nextX, nextY) && attachedMaze.IsPath(nextX, nextY))
            {
                currentX = nextX;
                currentY = nextY;
            }
            else
            {
                hitWall = true; // keep position but still learn this was a bad choice
            }

            RunnerState newState = new RunnerState(currentX, currentY);
            double reward = ComputeReward(currentX, currentY);

            if (hitWall)
            {
                const double wallPenalty = -0.35; // push it away from bumping walls
                reward += wallPenalty;
            }

            // tiny nudge if we follow the hint path
            AdjustRewardWithHint(oldState, newState, ref reward);
            RunnerStateAction oldStateAction = new RunnerStateAction(oldState, action);

            double oldQ = 0.0;
            if (qValues.TryGetValue(oldStateAction, out double existing))
            {
                oldQ = existing;
            }

            double bestNextQ = double.NegativeInfinity; // avoid fake optimism when all values are negative
            List<RunnerAction> validNext = GetValidActionsFrom(newState.X, newState.Y);
            foreach (RunnerAction possible in validNext)
            {
                RunnerStateAction nextStateAction = new RunnerStateAction(newState, possible);
                if (qValues.TryGetValue(nextStateAction, out double value))
                {
                    if (value > bestNextQ)
                    {
                        bestNextQ = value;
                    }
                }
            }
            if (bestNextQ == double.NegativeInfinity)
            {
                bestNextQ = 0.0; // prevents fake optimism if all rewards/Q are negative
            }

            double updatedQ = (1 - learningRate) * oldQ + learningRate * (reward + discountFactor * bestNextQ);

            qValues[oldStateAction] = updatedQ;

            // occasionally refresh the hint so it reacts to blocker walls
            hintCounter++;
            if (hintCounter >= 8)
            {
                BuildHintPath(new Point(currentX, currentY), false);
                BuildDistanceMapFromExit(); // maze may change when blocker adds walls
                hintCounter = 0;
            }
        }

        private double ComputeReward(int x, int y)
        {
            double reward = -0.15; // small step penalty so it keeps moving but feels cautious

            if (attachedMaze.tokens != null)
            {
                foreach (var token in attachedMaze.tokens)
                {
                    if (token.x == x && token.y == y)
                    {
                        reward += token.value;
                    }
                }
            }

            if (attachedMaze.IsExit(x, y))
            {
                reward += 30.0; // smaller bonus so it does not laser-focus only on the exit
            }

            // heavy penalty if we step onto a monster, lighter if close
            if (dangerCells != null && dangerCells.Count > 0)
            {
                int closest = 999;
                for (int i = 0; i < dangerCells.Count; i++)
                {
                    int dist = Math.Abs(dangerCells[i].X - x) + Math.Abs(dangerCells[i].Y - y);
                    if (dist < closest)
                    {
                        closest = dist;
                    }
                }
                if (closest == 0)
                {
                    reward -= 18.0; // really avoid standing on a monster
                }
                else if (closest == 1)
                {
                    reward -= 7.0; // strong fear when one step away
                }
                else if (closest == 2)
                {
                    reward -= 2.5; // still keep some distance
                }
            }

            double distanceToExit = GetCachedDistanceToExit(x, y);
            reward -= 0.02 * distanceToExit; // gentle pull toward the exit

            return reward;
        }

        private bool TryGreedyAction(out RunnerAction action)
        {
            action = RunnerAction.MoveUp;
            if (attachedMaze == null)
            {
                return false;
            }

            double bestDistance = double.PositiveInfinity;
            bool found = false;

            // explicit checks so the loop stays small
            TryGreedyCandidate(RunnerAction.MoveUp, ref bestDistance, ref action, ref found);
            TryGreedyCandidate(RunnerAction.MoveDown, ref bestDistance, ref action, ref found);
            TryGreedyCandidate(RunnerAction.MoveLeft, ref bestDistance, ref action, ref found);
            TryGreedyCandidate(RunnerAction.MoveRight, ref bestDistance, ref action, ref found);

            return found;
        }

        private void TryGreedyCandidate(RunnerAction possible, ref double bestDistance, ref RunnerAction action, ref bool found)
        {
            // helper to test one move for greedy pick (keeps TryGreedyAction short)
            int nextX;
            int nextY;
            GetNextForAction(currentX, currentY, possible, out nextX, out nextY);
            if (!IsInsideMaze(nextX, nextY) || !attachedMaze.IsPath(nextX, nextY))
            {
                return;
            }

            double dist = GetCachedDistanceToExit(nextX, nextY);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                action = possible;
                found = true;
            }
        }

        public Point GetCurrentPosition()
        {
            return new Point(currentX, currentY);
        }

        public void RefreshHintPathFromCurrent(bool reseed)
        {
            BuildHintPath(new Point(currentX, currentY), reseed);
            BuildDistanceMapFromExit(); // if walls changed in editor/blocker, distance map needs rebuilding
        }

        public void SetPositionAndHints(Point pos, bool reseed)
        {
            currentX = pos.X;
            currentY = pos.Y;
            BuildHintPath(pos, reseed);
            BuildDistanceMapFromExit(); // if walls changed in editor/blocker, distance map needs rebuilding
        }

        private void BuildHintPath(Point start, bool seedQ)
        {
            if (attachedMaze == null)
            {
                return;
            }

            HashSet<string> tokenCells = CollectTokenCells();

            // run a tiny A* to get a path that slightly prefers tokens
            hintPath = ComputeHintPath(attachedMaze, start, attachedMaze.exit, tokenCells);
            BuildHintNextSteps();

            if (seedQ)
            {
                SeedHintQValues();
            }
        }

        private HashSet<string> CollectTokenCells()
        {
            HashSet<string> set = new HashSet<string>();
            if (attachedMaze == null || attachedMaze.tokens == null)
            {
                return set;
            }

            // manual loop so it is obvious what is happening
            for (int i = 0; i < attachedMaze.tokens.Count; i++)
            {
                var token = attachedMaze.tokens[i];
                string key = MakeKey(token.x, token.y);
                set.Add(key);
            }

            return set;
        }

        private void BuildHintNextSteps()
        {
            hintNextStep = new Dictionary<string, Point>();

            if (hintPath == null || hintPath.Count < 2)
            {
                return;
            }

            for (int i = 0; i < hintPath.Count - 1; i++)
            {
                Point from = hintPath[i];
                Point to = hintPath[i + 1];
                hintNextStep[MakeKey(from.X, from.Y)] = to;
            }
        }

        private void SeedHintQValues()
        {
            if (hintPath == null || hintPath.Count < 2)
            {
                return;
            }

            double totalSeeded = 0.0;

            for (int i = 0; i < hintPath.Count - 1; i++)
            {
                Point from = hintPath[i];
                Point to = hintPath[i + 1];
                RunnerAction action;
                if (!TryGetActionForStep(from, to, out action))
                {
                    continue;
                }

                RunnerState state = new RunnerState(from.X, from.Y);
                RunnerStateAction stateAction = new RunnerStateAction(state, action);

                double existing = 0.0;
                qValues.TryGetValue(stateAction, out existing);
                double bumped = existing + 0.3;
                qValues[stateAction] = bumped;

                totalSeeded += 0.3;
                if (totalSeeded >= 1.0)
                {
                    break; // keep the hint tiny
                }
            }
        }

        private void AdjustRewardWithHint(RunnerState oldState, RunnerState newState, ref double reward)
        {
            if (hintNextStep == null || hintNextStep.Count == 0)
            {
                return;
            }

            string key = MakeKey(oldState.X, oldState.Y);
            if (hintNextStep.TryGetValue(key, out Point nextPoint))
            {
                if (nextPoint.X == newState.X && nextPoint.Y == newState.Y)
                {
                    reward += 0.08; // tiny bonus for following the hint
                }
                else
                {
                    reward -= 0.05; // tiny penalty for going away
                }
            }
        }

        private bool TryGetActionForStep(Point from, Point to, out RunnerAction action)
        {
            action = RunnerAction.MoveUp;

            int dx = to.X - from.X;
            int dy = to.Y - from.Y;

            if (dx == 1 && dy == 0)
            {
                action = RunnerAction.MoveRight;
                return true;
            }
            if (dx == -1 && dy == 0)
            {
                action = RunnerAction.MoveLeft;
                return true;
            }
            if (dx == 0 && dy == 1)
            {
                action = RunnerAction.MoveDown;
                return true;
            }
            if (dx == 0 && dy == -1)
            {
                action = RunnerAction.MoveUp;
                return true;
            }

            return false;
        }

        private List<Point> ComputeHintPath(Maze maze, Point start, Point exit, HashSet<string> tokenCells)
        {
            List<Point> empty = new List<Point>();

            if (maze == null)
            {
                return empty;
            }

            int width = maze.width;
            int height = maze.height;

            // cost trackers for A*
            double[,] pathCostSoFar = new double[width, height]; // sometimes called g-score
            double[,] totalEstimatedCost = new double[width, height]; // g + heuristic, sometimes f-score           
            bool[,] visited = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pathCostSoFar[x, y] = double.PositiveInfinity;
                    totalEstimatedCost[x, y] = double.PositiveInfinity;
                    visited[x, y] = false;
                }
            }

            Dictionary<string, Point> cameFrom = new Dictionary<string, Point>();

            // very simple open list (just a List, not a priority queue)
            List<Point> openList = new List<Point>();

            if (!maze.IsPath(start.X, start.Y))
            {
                return empty;
            }

            pathCostSoFar[start.X, start.Y] = 0.0;
            totalEstimatedCost[start.X, start.Y] = Heuristic(start, exit);
            openList.Add(start);

            int[] dirX = new int[] { 1, -1, 0, 0 };
            int[] dirY = new int[] { 0, 0, 1, -1 };

            // plain A* loop, picking the lowest estimated cost node each time (not the fastest, but easy to read)
            while (openList.Count > 0)
            {
                // pick node with lowest estimated cost (slow but simple)
                int bestIndex = 0;
                double bestScoreValue = totalEstimatedCost[openList[0].X, openList[0].Y];
                for (int i = 1; i < openList.Count; i++)
                {
                    Point candidate = openList[i];
                    double candidateScore = totalEstimatedCost[candidate.X, candidate.Y];
                    if (candidateScore < bestScoreValue)
                    {
                        bestScoreValue = candidateScore;
                        bestIndex = i;
                    }
                }

                Point current = openList[bestIndex];
                openList.RemoveAt(bestIndex);

                if (current.X == exit.X && current.Y == exit.Y)
                {
                    return ReconstructPath(cameFrom, current);
                }

                if (visited[current.X, current.Y])
                {
                    continue;
                }

                visited[current.X, current.Y] = true;

                for (int dirIndex = 0; dirIndex < 4; dirIndex++)
                {
                    int nextX = current.X + dirX[dirIndex];
                    int nextY = current.Y + dirY[dirIndex];

                    if (nextX < 0 || nextY < 0 || nextX >= width || nextY >= height)
                    {
                        continue;
                    }

                    if (!maze.IsPath(nextX, nextY))
                    {
                        continue;
                    }

                    // token cells are a bit cheaper, so the path might collect them
                    double stepCost = 1.0;
                    if (tokenCells.Contains(MakeKey(nextX, nextY)))
                    {
                        stepCost = 0.8;
                    }

                    double tentativeCost = pathCostSoFar[current.X, current.Y] + stepCost;

                    if (tentativeCost < pathCostSoFar[nextX, nextY])
                    {
                        pathCostSoFar[nextX, nextY] = tentativeCost;
                        totalEstimatedCost[nextX, nextY] = tentativeCost + Heuristic(new Point(nextX, nextY), exit);
                        cameFrom[MakeKey(nextX, nextY)] = current;

                        bool alreadyInOpen = false;
                        for (int j = 0; j < openList.Count; j++)
                        {
                            if (openList[j].X == nextX && openList[j].Y == nextY)
                            {
                                alreadyInOpen = true;
                                break;
                            }
                        }

                        if (!alreadyInOpen)
                        {
                            openList.Add(new Point(nextX, nextY));
                        }
                    }
                }
            }

            return empty; // no path found
        }

        private double Heuristic(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private List<Point> ReconstructPath(Dictionary<string, Point> cameFrom, Point current)
        {
            List<Point> path = new List<Point>();
            path.Add(current);

            while (cameFrom.TryGetValue(MakeKey(current.X, current.Y), out Point prev))
            {
                current = prev;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private string MakeKey(int x, int y)
        {
            return x + "," + y;
        }

        private bool IsInsideMaze(int x, int y)
        {
            // guard so we never ask IsPath on an out of bounds cell
            if (attachedMaze == null)
            {
                return false;
            }
            return x >= 0 && y >= 0 && x < attachedMaze.width && y < attachedMaze.height;
        }

        private List<RunnerAction> GetValidActionsFrom(int x, int y)
        {
            // only keep moves that stay in bounds and on a path (stops wall-bashing + crashes)
            List<RunnerAction> actions = new List<RunnerAction>();
            AddIfValid(actions, x, y, RunnerAction.MoveUp);
            AddIfValid(actions, x, y, RunnerAction.MoveDown);
            AddIfValid(actions, x, y, RunnerAction.MoveLeft);
            AddIfValid(actions, x, y, RunnerAction.MoveRight);
            return actions;
        }

        private void GetNextForAction(int x, int y, RunnerAction action, out int nx, out int ny)
        {
            // small helper to keep movement maths in one place (less duplication)
            nx = x;
            ny = y;
            if (action == RunnerAction.MoveUp)
            {
                ny = y - 1;
            }
            else if (action == RunnerAction.MoveDown)
            {
                ny = y + 1;
            }
            else if (action == RunnerAction.MoveLeft)
            {
                nx = x - 1;
            }
            else if (action == RunnerAction.MoveRight)
            {
                nx = x + 1;
            }
        }

        private void AddIfValid(List<RunnerAction> actions, int x, int y, RunnerAction action)
        {
            // helper so validity checks are not repeated everywhere
            int nx;
            int ny;
            GetNextForAction(x, y, action, out nx, out ny);
            if (IsInsideMaze(nx, ny) && attachedMaze.IsPath(nx, ny))
            {
                actions.Add(action);
            }
        }

        private void BuildDistanceMapFromExit()
        {
            // cache BFS from the exit so reward calls are faster
            if (attachedMaze == null)
            {
                return;
            }

            distToExit = new int[attachedMaze.width, attachedMaze.height]; // stored as [x,y] (width,height)
            for (int y = 0; y < attachedMaze.height; y++)
            {
                for (int x = 0; x < attachedMaze.width; x++)
                {
                    distToExit[x, y] = int.MaxValue;
                }
            }

            if (!IsInsideMaze(attachedMaze.exit.X, attachedMaze.exit.Y))
            {
                return;
            }
            if (!attachedMaze.IsPath(attachedMaze.exit.X, attachedMaze.exit.Y))
            {
                return;
            }

            Queue<Point> q = new Queue<Point>();
            q.Enqueue(attachedMaze.exit);
            distToExit[attachedMaze.exit.X, attachedMaze.exit.Y] = 0;

            int[] dx = new int[] { 1, -1, 0, 0 };
            int[] dy = new int[] { 0, 0, 1, -1 };

            while (q.Count > 0)
            {
                Point cur = q.Dequeue();
                int baseDist = distToExit[cur.X, cur.Y];
                for (int i = 0; i < 4; i++)
                {
                    int nx = cur.X + dx[i];
                    int ny = cur.Y + dy[i];
                    if (!IsInsideMaze(nx, ny))
                    {
                        continue;
                    }
                    if (!attachedMaze.IsPath(nx, ny))
                    {
                        continue;
                    }
                    if (distToExit[nx, ny] != int.MaxValue)
                    {
                        continue;
                    }
                    distToExit[nx, ny] = baseDist + 1;
                    q.Enqueue(new Point(nx, ny));
                }
            }
        }

        private double GetCachedDistanceToExit(int x, int y)
        {
            // use cache; unreachable stays huge so reward penalises it
            if (distToExit == null)
            {
                return UnreachableDistance;
            }
            if (!IsInsideMaze(x, y))
            {
                return UnreachableDistance;
            }
            int d = distToExit[x, y];
            if (d == int.MaxValue)
            {
                return UnreachableDistance;
            }
            return d;
        }
    }

    // simple episodic Q-learning agent for Lab mode runs
    internal class LabQLearningAgent
    {
        private double[,,] qValues; // row, col, action
        private readonly Random rng = new Random();
        public bool StopRequested { get; set; }
        private readonly double revisitPenalty = 0.08; // scales with repeat visits
        private int[,] distToExit;
        private const int INF = 999999;

        private void EnsureTable(Maze maze)
        {
            if (maze == null)
            {
                return;
            }
            if (qValues == null || qValues.GetLength(0) != maze.height || qValues.GetLength(1) != maze.width)
            {
                qValues = new double[maze.height, maze.width, 4];
            }
        }

        private void ClearQTable(Maze maze)
        {
            if (maze == null || qValues == null)
            {
                return;
            }

            // fresh run so it does not remember a different maze layout
            for (int y = 0; y < maze.height; y++)
            {
                for (int x = 0; x < maze.width; x++)
                {
                    for (int action = 0; action < 4; action++)
                    {
                        qValues[y, x, action] = 0.0;
                    }
                }
            }
        }

        private void BuildDistanceMap(Maze maze, Point exit)
        {
            distToExit = new int[maze.height, maze.width];
            for (int y = 0; y < maze.height; y++)
            {
                for (int x = 0; x < maze.width; x++)
                {
                    distToExit[y, x] = INF;
                }
            }

            if (!maze.IsPath(exit.X, exit.Y))
            {
                return;
            }

            Queue<Point> open = new Queue<Point>();
            distToExit[exit.Y, exit.X] = 0;
            open.Enqueue(exit);

            int[] moveX = { 1, -1, 0, 0 };
            int[] moveY = { 0, 0, 1, -1 };

            while (open.Count > 0)
            {
                Point current = open.Dequeue();
                int currentDist = distToExit[current.Y, current.X];

                for (int i = 0; i < 4; i++)
                {
                    int nx = current.X + moveX[i];
                    int ny = current.Y + moveY[i];
                    if (nx < 0 || ny < 0 || nx >= maze.width || ny >= maze.height)
                    {
                        continue;
                    }
                    if (!maze.IsPath(nx, ny))
                    {
                        continue;
                    }
                    if (distToExit[ny, nx] != INF)
                    {
                        continue;
                    }
                    distToExit[ny, nx] = currentDist + 1;
                    open.Enqueue(new Point(nx, ny));
                }
            }
        }

        // keep status text formatting in one place so the UI knows where it comes from
        private string BuildLiveStatusText(int episode, int stepNumber, double totalReward, bool exploring, double epsilon, int stepsTaken)
        {
            return "Episode " + episode
                   + " | Step " + stepNumber
                   + " | Reward " + totalReward.ToString("0.00")
                   + " | Exploration " + epsilon.ToString("0.00");
        }

        private string BuildEndStatusText(int episode, bool success, int stepsTaken, int explorePercent, int last10Successes, int avgSuccessSteps)
        {
            string result = success ? "SUCCESS" : "FAIL";
            return "Episode " + episode + " ended: " + result
                   + " | Steps " + stepsTaken
                   + " | Exploration " + explorePercent + "%";
        }

        public async Task RunEpisodesAsync(
            Maze maze,
            Point start,
            Point exit,
            double alpha,
            double gamma,
            double epsilon,
            double decay,
            int episodes,
            int maxSteps,
            int delayMs,
            double bias,
            Action<Point, string> updateStatus,
            Func<Task> redrawUi,
            bool recordExperiment = false,
            int experimentEpisodes = 50)
        {
            if (maze == null)
            {
                return;
            }
            EnsureTable(maze);
            ClearQTable(maze);
            BuildDistanceMap(maze, exit);
            if (distToExit == null || distToExit[start.Y, start.X] == INF)
            {
                updateStatus?.Invoke(start, "No path from start to exit, agent cannot learn this maze.");
                return;
            }
            int runEpisodes = episodes;
            if (recordExperiment && runEpisodes < experimentEpisodes)
            {
                runEpisodes = experimentEpisodes;
            }
            if (epsilon < 0) epsilon = 0;
            if (epsilon > 1) epsilon = 1;
            double noiseChance = Math.Max(0.0, epsilon); // use the user's exploration choice as-is
            int warmupEpisodes = 10; // keep exploration high early so learning is visible
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            StopRequested = false;
            int totalSuccesses = 0;
            int totalSuccessSteps = 0;
            List<bool> last10 = new List<bool>();
            List<int> stepsPerEpisode = new List<int>();
            List<bool> successPerEpisode = new List<bool>();

            for (int ep = 1; ep <= runEpisodes; ep++)
            {
                if (StopRequested)
                {
                    updateStatus?.Invoke(start, "Agent stopped.");
                    return;
                }

                Point agentPos = start;
                Point lastPos = agentPos;
                double totalReward = 0;
                bool success = false;
                int stepsTaken = 0;
                int exploreCount = 0;
                int exploitCount = 0;
                Dictionary<int, int> visitCounts = new Dictionary<int, int>(); // count visits this episode
                visitCounts[agentPos.X * 10000 + agentPos.Y] = 1;
                updateStatus?.Invoke(agentPos, "Episode " + ep + " starting");
                await redrawUi();

                int stepsThisEpisode = Math.Max(1, maxSteps); // honour the step cap from the UI

                for (int step = 0; step < stepsThisEpisode; step++)
                {
                    if (StopRequested)
                    {
                        updateStatus?.Invoke(agentPos, "Agent stopped.");
                        return;
                    }

                    List<int> validActions = GetValidActions(maze, agentPos);
                    if (validActions.Count == 0)
                    {
                        updateStatus?.Invoke(agentPos, "No valid moves, episode ends.");
                        break;
                    }

                    bool exploring;
                    int action = ChooseAction(agentPos, epsilon, noiseChance, validActions, out exploring);
                    if (exploring)
                    {
                        exploreCount++;
                    }
                    else
                    {
                        exploitCount++;
                    }
                    Point nextPos;
                    double reward;
                    TakeStep(maze, agentPos, lastPos, action, exit, bias, visitCounts, out nextPos, out reward);
                    totalReward += reward;
                    int key = nextPos.X * 10000 + nextPos.Y;
                    if (!visitCounts.ContainsKey(key))
                    {
                        visitCounts[key] = 0;
                    }
                    visitCounts[key] = visitCounts[key] + 1;
                    UpdateQ(maze, agentPos, action, nextPos, reward, alpha, gamma);
                    lastPos = agentPos;
                    agentPos = nextPos;
                    stepsTaken = step + 1;

                    string text = BuildLiveStatusText(ep, step + 1, totalReward, exploring, epsilon, stepsTaken);
                    updateStatus?.Invoke(agentPos, text);
                    await redrawUi();
                    await Task.Delay(delayMs);

                    if (agentPos.X == exit.X && agentPos.Y == exit.Y)
                    {
                        success = true;
                        stepsTaken = step + 1;
                        break; // reached goal
                    }
                }

                // brief pause between episodes so you can see it try again
                if (StopRequested)
                {
                    updateStatus?.Invoke(start, "Agent stopped.");
                    return;
                }

                if (!success && stepsTaken == 0)
                {
                    stepsTaken = stepsThisEpisode; // treat early end as full length
                }

                stepsPerEpisode.Add(stepsTaken);
                successPerEpisode.Add(success);

                totalSuccesses += success ? 1 : 0;
                if (success)
                {
                    totalSuccessSteps += stepsTaken;
                }

                last10.Add(success);
                if (last10.Count > 10)
                {
                    last10.RemoveAt(0);
                }
                int last10Successes = 0;
                for (int i = 0; i < last10.Count; i++)
                {
                    if (last10[i])
                    {
                        last10Successes++;
                    }
                }
                int avgSuccessSteps = totalSuccesses > 0 ? (int)Math.Round(totalSuccessSteps / (double)totalSuccesses) : 0;
                int exploreTotal = exploreCount + exploitCount;
                int explorePercent = exploreTotal > 0 ? (int)Math.Round((exploreCount / (double)exploreTotal) * 100.0) : 0;

                string endText = BuildEndStatusText(ep, success, stepsTaken, explorePercent, last10Successes, avgSuccessSteps);
                updateStatus?.Invoke(agentPos, endText);

                await Task.Delay(150);

                if (ep > warmupEpisodes)
                {
                    epsilon = Math.Max(0.0, Math.Min(1.0, epsilon * decay)); // allow epsilon to reach zero if chosen
                    noiseChance = Math.Max(0.0, Math.Min(noiseChance * decay, epsilon)); // noise falls away with epsilon
                }
            }

            watch.Stop();
            if (recordExperiment)
            {
                int finalLast10 = 0;
                for (int i = Math.Max(0, successPerEpisode.Count - 10); i < successPerEpisode.Count; i++)
                {
                    if (successPerEpisode[i])
                    {
                        finalLast10++;
                    }
                }
                int avgSuccessStepsFinal = totalSuccesses > 0 ? (int)Math.Round(totalSuccessSteps / (double)totalSuccesses) : 0;
                string summary = "Experiment summary: successes " + totalSuccesses + "/" + runEpisodes;
                summary += " | last10 " + finalLast10 + "/10";
                summary += " | avg success steps " + avgSuccessStepsFinal;
                summary += " | elapsed " + watch.ElapsedMilliseconds + " ms.";
                updateStatus?.Invoke(exit, summary);
            }
            else
            {
                int finalLast10 = 0;
                for (int i = Math.Max(0, successPerEpisode.Count - 10); i < successPerEpisode.Count; i++)
                {
                    if (successPerEpisode[i])
                    {
                        finalLast10++;
                    }
                }
                int avgSuccessStepsFinal = totalSuccesses > 0 ? (int)Math.Round(totalSuccessSteps / (double)totalSuccesses) : 0;
                string wrapUp = "Finished " + runEpisodes + " episodes. Successes " + totalSuccesses + "/" + runEpisodes;
                wrapUp += " | last10 " + finalLast10 + "/10";
                wrapUp += " | avg success steps " + avgSuccessStepsFinal;
                updateStatus?.Invoke(exit, wrapUp);
            }
        }

        private List<int> GetValidActions(Maze maze, Point pos)
        {
            List<int> actions = new List<int>();
            int[] moveX = { 0, 0, -1, 1 };
            int[] moveY = { -1, 1, 0, 0 };
            for (int a = 0; a < 4; a++)
            {
                int nextX = pos.X + moveX[a];
                int nextY = pos.Y + moveY[a];
                if (nextX < 0 || nextY < 0 || nextX >= maze.width || nextY >= maze.height)
                {
                    continue;
                }
                if (maze.IsPath(nextX, nextY))
                {
                    actions.Add(a);
                }
            }
            return actions;
        }

        private int ChooseAction(Point pos, double epsilon, double noiseChance, List<int> validActions, out bool exploring)
        {
            if (validActions.Count == 0)
            {
                exploring = false;
                return 0;
            }

            if (rng.NextDouble() < epsilon)
            {
                exploring = true;
                return validActions[rng.Next(validActions.Count)]; // random explore
            }

            exploring = false;
            double best = double.NegativeInfinity;
            List<int> bestActions = new List<int>(); // random tie break
            for (int i = 0; i < validActions.Count; i++)
            {
                int a = validActions[i];
                double score = qValues[pos.Y, pos.X, a];
                if (score > best)
                {
                    best = score;
                    bestActions.Clear();
                    bestActions.Add(a);
                }
                else if (score == best)
                {
                    bestActions.Add(a);
                }
            }
            int pick = rng.Next(bestActions.Count);
            int chosen = bestActions[pick];

            // small randomness so the agent does not look perfect early on (layered on top of user epsilon)
            if (rng.NextDouble() < noiseChance)
            {
                exploring = true;
                chosen = validActions[rng.Next(validActions.Count)];
            }

            return chosen;
        }

        private void TakeStep(Maze maze, Point pos, Point lastPos, int action, Point exit, double bias, Dictionary<int, int> visitCounts, out Point nextPos, out double reward)
        {
            int nx = pos.X;
            int ny = pos.Y;
            if (action == 0) ny -= 1; // up
            if (action == 1) ny += 1; // down
            if (action == 2) nx -= 1; // left
            if (action == 3) nx += 1; // right

            reward = -0.05; // small step penalty so shorter routes are better
            if (!maze.IsPath(nx, ny))
            {
                nextPos = pos;
                reward -= 0.35; // bumping a wall hurts more
                return;
            }

            nextPos = new Point(nx, ny);

            int distBefore = distToExit[pos.Y, pos.X];
            int distAfter = distToExit[nextPos.Y, nextPos.X];
            double biasContribution = Math.Max(0.0, bias);
            if (distAfter < distBefore)
            {
                reward += biasContribution; // distance map is a weak hint, not a perfect guide
                reward += 0.01;
            }
            else if (distAfter > distBefore)
            {
                reward -= 0.01;
            }
            else
            {
                reward -= 0.005;
            }

            int key = nextPos.X * 10000 + nextPos.Y;
            if (visitCounts != null && visitCounts.ContainsKey(key))
            {
                reward -= revisitPenalty * visitCounts[key]; // heavier penalty on repeats
            }

            if (nextPos.X == lastPos.X && nextPos.Y == lastPos.Y)
            {
                reward -= 0.12; // stop A->B->A backtracks
            }

            if (nextPos.X == exit.X && nextPos.Y == exit.Y)
            {
                reward += 8.0; // big reward for finishing
            }
        }

        private void UpdateQ(Maze maze, Point pos, int action, Point nextPos, double reward, double alpha, double gamma)
        {
            double oldQ = qValues[pos.Y, pos.X, action];
            double nextBest = 0.0;
            List<int> validNextActions = GetValidActions(maze, nextPos);
            if (validNextActions.Count > 0)
            {
                nextBest = double.NegativeInfinity;
                for (int i = 0; i < validNextActions.Count; i++)
                {
                    int nextAction = validNextActions[i];
                    double value = qValues[nextPos.Y, nextPos.X, nextAction];
                    if (value > nextBest)
                    {
                        nextBest = value;
                    }
                }
                if (nextBest == double.NegativeInfinity)
                {
                    nextBest = 0.0;
                }
            }

            // standard Q update uses best of next state
            double updated = oldQ + alpha * (reward + gamma * nextBest - oldQ);
            qValues[pos.Y, pos.X, action] = updated;
        }
    }
}
