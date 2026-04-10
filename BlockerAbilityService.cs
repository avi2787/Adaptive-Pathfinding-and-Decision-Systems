using System;
using System.Collections.Generic;
using System.Drawing;

namespace NEA_ai_pathfinding
{
    // blocker tools live here so they are not in the Form
    internal class BlockerAbilityService
    {
        private readonly ChallengeMazeState state;
        private readonly Random random = new Random();
        private const int MinSpawnDistance = 6;
        private const int MonsterLifetimeSeconds = 30;
        private const int WindPushMaxSteps = 3;

        public BlockerAbilityService(ChallengeMazeState MazeFormState)
        {
            state = MazeFormState;
        }

        public void ConfigureForLevel(int level, BlockerHeuristicController helper)
        {
            state.BlockerHelper = helper;
            state.WallsLeft = Math.Min(Math.Max(level, 1), 10); // 1 wall at level 1, +1 per level
            state.WindCharges = Math.Min(Math.Max(level, 1), 10); // keep it simple, one push per level
            state.MaxMonsters = Math.Min(Math.Max(level, 1), 5); // L1:1, L2:2, L3:3, L4:4, L5+:5
            state.MonsterCharges = state.MaxMonsters;
            state.Monsters = new List<MonsterController>();

            state.WallLifetimeSeconds = (level >= 5) ? 6 : 5;
            state.WallCooldownSeconds = 4;
            state.WindCooldownSeconds = 5;
            state.MonsterCooldownSeconds = 2; // small cooldown so multiple can be spawned
            state.WallCooldownUntil = DateTime.MinValue;
            state.WindCooldownUntil = DateTime.MinValue;
            state.MonsterCooldownUntil = DateTime.MinValue;
            state.TempWalls = new List<TempWall>();
        }

        // remove temporary walls when their timer runs out
        public bool ExpireWalls(Maze maze, DateTime now)
        {
            bool removedAny = false;

            for (int i = state.TempWalls.Count - 1; i >= 0; i--)
            {
                TempWall wall = state.TempWalls[i];
                if (now >= wall.RemoveTime)
                {
                    if (maze != null)
                    {
                        if (wall.X >= 0 && wall.Y >= 0 &&
                            wall.X < maze.width && wall.Y < maze.height)
                        {
                            maze.grid[wall.X, wall.Y] = 1;
                            removedAny = true;
                        }
                    }

                    state.TempWalls.RemoveAt(i);
                }
            }

            return removedAny;
        }

        // place a temporary wall by human click
        public PlaceWallResult TryPlaceWall(Maze maze, Point runnerCell, Point wallCell, DateTime now)
        {
            if (maze == null)
            {
                return PlaceWallResult.NotAPath;
            }

            // Guard: ignore clicks that land outside the maze to prevent crashes during blocker input/AI suggestions.
            if (wallCell.X < 0 || wallCell.Y < 0 || wallCell.X >= maze.width || wallCell.Y >= maze.height)
            {
                return PlaceWallResult.NotAPath;
            }

            if (state.BlockerHelper == null)
            {
                return PlaceWallResult.NotAPath;
            }

            if (state.WallsLeft <= 0)
            {
                return PlaceWallResult.NoWalls;
            }

            if (now < state.WallCooldownUntil)
            {
                return PlaceWallResult.Cooldown;
            }

            // keep walls away from spawn/exit so paths stay fair
            int distStart = Math.Abs(wallCell.X - maze.start.X) + Math.Abs(wallCell.Y - maze.start.Y);
            int distExit = Math.Abs(wallCell.X - maze.exit.X) + Math.Abs(wallCell.Y - maze.exit.Y);
            if (distStart < 5 || distExit < 5)
            {
                return PlaceWallResult.NotAPath;
            }

            int oldValue = maze.grid[wallCell.X, wallCell.Y];

            BlockerContext ctx = new BlockerContext(maze, runnerCell, wallCell, null);
            state.BlockerHelper.UseAbilityAsHuman(BlockerActionType.PlaceWall, ctx, now);

            if (oldValue == 1 && maze.grid[wallCell.X, wallCell.Y] == 0)
            {
                state.WallsLeft = state.WallsLeft - 1;
                state.WallCooldownUntil = now.AddSeconds(state.WallCooldownSeconds);

                TempWall temp = new TempWall();
                temp.X = wallCell.X;
                temp.Y = wallCell.Y;
                temp.RemoveTime = now.AddSeconds(state.WallLifetimeSeconds);
                state.TempWalls.Add(temp);

                if (state.RunnerAgent != null)
                {
                    state.RunnerAgent.RefreshHintPathFromCurrent(false);
                }

                return PlaceWallResult.Success;
            }

            return PlaceWallResult.NotAPath;
        }

        // wind push driven by a direction (WASD/arrows)
        public WindResult TryWindPush(Maze maze, Point runnerCell, int stepX, int stepY, DateTime now, out List<Point> pushPath)
        {
            pushPath = new List<Point>();

            if (maze == null)
            {
                return WindResult.NotAdjacent;
            }

            if (state.WindCharges <= 0)
            {
                return WindResult.NoCharges;
            }

            if (now < state.WindCooldownUntil)
            {
                return WindResult.Cooldown;
            }

            if ((stepX == 0 && stepY == 0) || (stepX != 0 && stepY != 0))
            {
                return WindResult.NotAdjacent;
            }

            Point current = runnerCell;
            for (int i = 0; i < WindPushMaxSteps; i++)
            {
                int nx = current.X + stepX;
                int ny = current.Y + stepY;
                if (nx < 0 || ny < 0 || nx >= maze.width || ny >= maze.height)
                {
                    break; // out of bounds
                }
                if (!maze.IsPath(nx, ny))
                {
                    break; // hit a wall
                }
                current = new Point(nx, ny);
                pushPath.Add(current);
                if (MonsterHelpers.IsOnMonster(state, current.X, current.Y))
                {
                    break; // stop if you slam into a monster
                }
            }

            if (pushPath.Count == 0)
            {
                return WindResult.Blocked;
            }

            state.WindCooldownUntil = now.AddSeconds(state.WindCooldownSeconds);
            state.WindCharges = Math.Max(0, state.WindCharges - 1);

            if (state.RunnerAgent != null)
            {
                state.RunnerAgent.SetPositionAndHints(pushPath[pushPath.Count - 1], false);
            }

            return WindResult.Success;
        }

        // Spawns a monster far from the runner, avoiding start/exit/tokens/other monsters so it stays fair.
        public SpawnResult TrySpawnMonster(DateTime now, bool preferNear)
        {
            if (state.Monsters == null)
            {
                state.Monsters = new List<MonsterController>();
            }
            if (now < state.MonsterCooldownUntil)
            {
                return SpawnResult.Cooldown;
            }
            if (state.Monsters.Count >= state.MaxMonsters)
            {
                return SpawnResult.NoPath; // already at max active
            }
            if (state.MonsterCharges <= 0)
            {
                return SpawnResult.NoPath;
            }

            Point spawn;
            if (!TryPickSpawnCell(state.Maze, new Point(state.PlayerX, state.PlayerY), MinSpawnDistance, out spawn))
            {
                return SpawnResult.NoPath;
            }

            MonsterController controller = new MonsterController();
            controller.Active = true;
            controller.X = spawn.X;
            controller.Y = spawn.Y;
            controller.PrevX = spawn.X;
            controller.PrevY = spawn.Y;
            controller.RemoveTime = now.AddSeconds(MonsterLifetimeSeconds); // keep monsters on the maze longer (30s)

            state.Monsters.Add(controller);
            state.MonsterCharges = Math.Max(0, state.MonsterCharges - 1);
            state.MonsterActive = true;
            state.MonsterX = controller.X;
            state.MonsterY = controller.Y;
            state.MonsterPrevX = controller.PrevX;
            state.MonsterPrevY = controller.PrevY;
            state.MonsterRemoveTime = controller.RemoveTime;
            state.MonsterCooldownUntil = now.AddSeconds(state.MonsterCooldownSeconds);
            return SpawnResult.Success;
        }

        // Picks a random open cell across the maze that is at least minDistance from the runner.
        // Avoids start, exit, tokens, and existing monsters so spawns don't overlap or block spawns unfairly.
        private bool TryPickSpawnCell(Maze maze, Point runner, int minDistance, out Point spawn)
        {
            spawn = Point.Empty;
            if (maze == null)
            {
                return false;
            }

            List<Point> options = new List<Point>();
            for (int y = 0; y < maze.height; y++)
            {
                for (int x = 0; x < maze.width; x++)
                {
                    if (!maze.IsPath(x, y))
                    {
                        continue;
                    }
                    if (x == maze.start.X && y == maze.start.Y)
                    {
                        continue;
                    }
                    if (x == maze.exit.X && y == maze.exit.Y)
                    {
                        continue;
                    }
                    int dist = Math.Abs(x - runner.X) + Math.Abs(y - runner.Y);
                    if (dist < minDistance)
                    {
                        continue;
                    }
                    options.Add(new Point(x, y));
                }
            }

            if (options.Count == 0)
            {
                return false;
            }

            HashSet<string> occupied = new HashSet<string>();
            if (state.Monsters != null)
            {
                for (int i = 0; i < state.Monsters.Count; i++)
                {
                    occupied.Add(state.Monsters[i].X + "," + state.Monsters[i].Y);
                }
            }

            if (maze.tokens != null)
            {
                for (int i = 0; i < maze.tokens.Count; i++)
                {
                    occupied.Add(maze.tokens[i].x + "," + maze.tokens[i].y);
                }
            }

            List<Point> filtered = new List<Point>();
            for (int i = 0; i < options.Count; i++)
            {
                string key = options[i].X + "," + options[i].Y;
                if (!occupied.Contains(key))
                {
                    filtered.Add(options[i]);
                }
            }

            if (filtered.Count == 0)
            {
                return false;
            }

            spawn = filtered[random.Next(filtered.Count)];
            return true;
        }
    }

    public enum PlaceWallResult
    {
        Success,
        NoWalls,
        Cooldown,
        NotAPath
    }

    public enum SpawnResult
    {
        Success,
        Cooldown,
        NoPath
    }

    public enum WindResult
    {
        Success,
        Cooldown,
        NotAdjacent,
        Blocked,
        NoCharges
    }
}
