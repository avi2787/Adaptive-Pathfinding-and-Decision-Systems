using System;
using System.Collections.Generic;
using System.Drawing;

namespace NEA_ai_pathfinding
{
    // carries all the mutable game data so the Form itself can stay thin
    internal class ChallengeMazeState
    {
        public Maze Maze;
        public int PlayerX;
        public int PlayerY;
        public bool InGame;
        public bool IsPaused;
        public DateTime StartTime;
        public bool CountdownActive;
        public int CurrentScore;
        public int TimeLimitSeconds;
        public int RemainingSeconds;
        public double TimerAccumulator;
        public bool IsCarving;
        public bool StartResetAfterCarve;
        public bool IsBlockerGame;
        public bool BlockerAgentActive;
        public BlockerActionType CurrentBlockerAction;
        public RunnerQAgent RunnerAgent;
        public int RunnerStepCounter;
        public bool RunnerAgentChosen;
        public int BlockerAgentStepCounter;
        public int MonsterStepCounter;
        public Color MazeBaseColor = Color.DarkGreen;
        public BlockerHeuristicController BlockerHelper;
        public BlockerAbilityService BlockerService;
        public List<MonsterController> Monsters = new List<MonsterController>();

        // blocker tools and cooldowns
        public int WallsLeft;
        public List<TempWall> TempWalls = new List<TempWall>();
        public DateTime WallCooldownUntil = DateTime.MinValue;
        public DateTime WindCooldownUntil = DateTime.MinValue;
        public DateTime MonsterCooldownUntil = DateTime.MinValue;
        public int WallLifetimeSeconds = 5;
        public int WallCooldownSeconds = 4;
        public int WindCooldownSeconds = 5;
        public int MonsterCooldownSeconds = 8;
        public int MaxMonsters = 1;
        public int MonsterCharges = 1;
        public int WindCharges = 1;
        public bool MonsterActive;
        public int MonsterX;
        public int MonsterY;
        public DateTime MonsterRemoveTime;
        public int MonsterPrevX = -1;
        public int MonsterPrevY = -1;
        public int StepsTaken;
        public int TokensCollected; // coins collected this run
        public int ShortestPathLength;

        // helper to clear everything when starting a level
        public void ResetForNewGame(bool isBlocker, int walls, Color baseColor)
        {
            // core flags/time
            Maze = null;
            PlayerX = 0;
            PlayerY = 0;
            InGame = false;
            IsPaused = false;
            StartTime = DateTime.UtcNow; // set to now; countdown will reset when play begins
            CountdownActive = false;
            CurrentScore = 0;
            TimeLimitSeconds = 0;
            RemainingSeconds = 0;
            TimerAccumulator = 0.0;
            IsCarving = false;
            StartResetAfterCarve = false;
            IsBlockerGame = isBlocker;
            BlockerAgentActive = false;
            CurrentBlockerAction = BlockerActionType.None;
            RunnerAgent = null;
            RunnerStepCounter = 0;
            RunnerAgentChosen = false;
            BlockerAgentStepCounter = 0;
            MonsterStepCounter = 0;
            MazeBaseColor = baseColor;
            BlockerHelper = null;
            WallsLeft = walls;
            // reset blockers/monsters
            Monsters = new List<MonsterController>();
            TempWalls = new List<TempWall>();
            WallCooldownUntil = DateTime.MinValue;
            WindCooldownUntil = DateTime.MinValue;
            MonsterCooldownUntil = DateTime.MinValue;
            MaxMonsters = 1;
            WindCharges = 1;
            MonsterActive = false;
            MonsterX = 0;
            MonsterY = 0;
            MonsterPrevX = -1;
            MonsterPrevY = -1;
            // counters
            StepsTaken = 0;
            TokensCollected = 0;
            ShortestPathLength = 0;
        }
    }

    public class TempWall
    {
        public int X;
        public int Y;
        public DateTime RemoveTime;
    }
}
