using System;
using System.Collections.Generic;
using System.Drawing;

namespace NEA_ai_pathfinding
{

    // Blocker support: heuristic AI + ability plumbing (walls, wind, monster)
    public enum BlockerAbilityType
    {
        WindPush,
        Wall,
        Monster
    }

    public enum BlockerDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum BlockerActionType
    {
        None,
        UseWindPush,
        PlaceWall,
        SpawnMonster
    }

    public sealed class BlockerObservation
    {
        public Maze Maze { get; }
        public Point RunnerPosition { get; }
        public Point ExitPosition { get; }
        public bool MonsterActive { get; }
        public int WallsLeft { get; }

        public BlockerObservation(Maze maze, Point runnerPosition, Point exitPosition, bool monsterActive, int wallsLeft)
        {
            if (maze == null)
            {
                throw new ArgumentNullException(nameof(maze));
            }
            Maze = maze;
            RunnerPosition = runnerPosition;
            ExitPosition = exitPosition;
            MonsterActive = monsterActive;
            WallsLeft = wallsLeft;
        }
    }

    public sealed class BlockerDecision
    {
        public BlockerActionType ActionType { get; }
        public Point TargetCell { get; }
        public BlockerDirection? PushDirection { get; }

        public BlockerDecision(BlockerActionType actionType, Point targetCell, BlockerDirection? pushDirection)
        {
            ActionType = actionType;
            TargetCell = targetCell;
            PushDirection = pushDirection;
        }

        public static BlockerDecision NoAction()
        {
            return new BlockerDecision(BlockerActionType.None, Point.Empty, null);
        }
    }

    public sealed class BlockerContext
    {
        public Maze Maze { get; }
        public Point RunnerPosition { get; }
        public Point? TargetCell { get; }
        public BlockerDirection? Direction { get; }

        public BlockerContext(Maze maze, Point runnerPosition, Point? targetCell, BlockerDirection? direction)
        {
            if (maze == null)
            {
                throw new ArgumentNullException(nameof(maze));
            }
            Maze = maze;
            RunnerPosition = runnerPosition;
            TargetCell = targetCell;
            Direction = direction;
        }
    }

    public abstract class BlockerAbility
    {
        public BlockerAbilityType AbilityType { get; }
        public TimeSpan Cooldown { get; protected set; }
        public DateTime LastUsedUtc { get; protected set; }

        protected BlockerAbility(BlockerAbilityType abilityType, TimeSpan cooldown)
        {
            AbilityType = abilityType;
            Cooldown = cooldown;
            LastUsedUtc = DateTime.MinValue;
        }

        public bool CanUse(DateTime now)
        {
            TimeSpan elapsed = now - LastUsedUtc;
            return elapsed >= Cooldown;
        }

        public void Use(BlockerContext context, DateTime now)
        {
            if (!CanUse(now))
                return;

            Execute(context); // ability-specific behaviour
            LastUsedUtc = now;
        }

        protected abstract void Execute(BlockerContext context);
    }

    public sealed class WindPushAbility : BlockerAbility
    {
        public WindPushAbility(TimeSpan cooldown) : base(BlockerAbilityType.WindPush, cooldown)
        {
        }

        protected override void Execute(BlockerContext context)
        {
        }
    }

    public sealed class WallAbility : BlockerAbility
    {
        public WallAbility(TimeSpan cooldown) : base(BlockerAbilityType.Wall, cooldown)
        {
        }

        protected override void Execute(BlockerContext context)
        {
            if (context == null)
            {
                return;
            }

            if (context.Maze == null)
            {
                return;
            }

            if (!context.TargetCell.HasValue)
            {
                return;
            }

            Maze maze = context.Maze;
            Point target = context.TargetCell.Value;

            // make sure inside grid
            if (target.X < 0 || target.Y < 0 || target.X >= maze.width || target.Y >= maze.height)
            {
                return;
            }

            // do not block runner or exit
            if (target.X == context.RunnerPosition.X && target.Y == context.RunnerPosition.Y)
            {
                return;
            }

            if (target.X == maze.exit.X && target.Y == maze.exit.Y)
            {
                return;
            }

            // only place wall on a path cell
            if (maze.grid[target.X, target.Y] == 1)
            {
                maze.grid[target.X, target.Y] = 0;

                // remove any token on this cell so it is not hidden inside a wall
                if (maze.tokens != null)
                {
                    for (int i = maze.tokens.Count - 1; i >= 0; i--)
                    {
                        var token = maze.tokens[i];
                        if (token.x == target.X && token.y == target.Y)
                        {
                            maze.tokens.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }

    public sealed class MonsterAbility : BlockerAbility
    {
        public MonsterAbility(TimeSpan cooldown) : base(BlockerAbilityType.Monster, cooldown)
        {
        }

        protected override void Execute(BlockerContext context)
        {
        }
    }

    public sealed class BlockerHeuristicController
    {
        private Maze? attachedMaze;
        private readonly Random random = new Random();

        public WindPushAbility WindPush { get; }
        public WallAbility Wall { get; }
        public MonsterAbility Monster { get; }

        public BlockerHeuristicController()
        {
            WindPush = new WindPushAbility(TimeSpan.FromSeconds(3));
            Wall = new WallAbility(TimeSpan.FromSeconds(5));
            Monster = new MonsterAbility(TimeSpan.FromSeconds(0)); // instant reuse limit handled elsewhere
        }

        public void AttachMaze(Maze maze)
        {
            if (maze == null)
            {
                throw new ArgumentNullException(nameof(maze));
            }
            attachedMaze = maze;
        }

        public BlockerDecision DecideNextAction(BlockerObservation observation)
        {
            if (observation == null)
            {
                throw new ArgumentNullException(nameof(observation));
            }

            if (attachedMaze == null)
            {
                attachedMaze = observation.Maze;
            }

            Maze maze = observation.Maze;
            Point runner = observation.RunnerPosition;
            Point exit = observation.ExitPosition;
            DateTime now = DateTime.UtcNow;

            maze.TryBuildPath(runner, exit, out List<Point> path);
            int distanceToExit = path == null ? int.MaxValue : path.Count - 1;

            // try to drop a wall a couple of steps in front of the runner if we have any left
            if (path != null && path.Count >= 3 && observation.WallsLeft > 0 && Wall.CanUse(now))
            {
                int idx = Math.Min(2, path.Count - 2); // a few steps ahead, but not the exit itself
                Point target = path[idx];
                if (maze.grid[target.X, target.Y] == 1)
                {
                    return new BlockerDecision(BlockerActionType.PlaceWall, target, null);
                }
            }

            // spawn monsters whenever under cap and runner is not too far (allows multiple)
            if (Monster.CanUse(now) && distanceToExit <= 40)
            {
                return new BlockerDecision(BlockerActionType.SpawnMonster, runner, null);
            }

            // gentle wind push: if a neighbouring cell is further from the exit, push there
            if (WindPush.CanUse(now) && path != null && path.Count > 1)
            {
                Point best = FindFarthestNeighbourFromExit(maze, runner, exit);
                if (best != Point.Empty)
                {
                    return new BlockerDecision(BlockerActionType.UseWindPush, best, null);
                }
            }

            return BlockerDecision.NoAction();
        }

        public void UseAbilityAsHuman(BlockerActionType actionType, BlockerContext context, DateTime now)
        {
            switch (actionType)
            {
                case BlockerActionType.UseWindPush:
                    WindPush.Use(context, now);
                    break;
                case BlockerActionType.PlaceWall:
                    Wall.Use(context, now);
                    break;
                case BlockerActionType.SpawnMonster:
                    Monster.Use(context, now);
                    break;
                default:
                    break;
            }
        }

        private Point FindFarthestNeighbourFromExit(Maze maze, Point runner, Point exit)
        {
            if (maze == null)
            {
                return Point.Empty;
            }

                // reuse shared path builder to estimate distance from exit
                if (!maze.TryBuildPath(exit, runner, out List<Point> pathToRunner) || pathToRunner.Count == 0)
                {
                    return Point.Empty;
                }

            int currentDist = pathToRunner.Count - 1;
            Point best = Point.Empty;
            int bestDist = currentDist;
            int[] dx = new int[] { 1, -1, 0, 0 };
            int[] dy = new int[] { 0, 0, 1, -1 };

            for (int i = 0; i < 4; i++)
            {
                int nx = runner.X + dx[i];
                int ny = runner.Y + dy[i];
                if (nx < 0 || ny < 0 || nx >= maze.width || ny >= maze.height)
                {
                    continue;
                }
                if (!maze.IsPath(nx, ny))
                {
                    continue;
                }
                // simple distance guess: path length from exit to neighbour (or keep current)
                int guessed = currentDist;
                if (maze.TryBuildPath(exit, new Point(nx, ny), out List<Point> pathNeighbour) && pathNeighbour.Count > 0)
                {
                    guessed = pathNeighbour.Count - 1;
                }

                if (guessed > bestDist)
                {
                    bestDist = guessed;
                    best = new Point(nx, ny);
                }
            }

            return best;
        }
    }
}
