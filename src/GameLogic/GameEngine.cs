using System;
using System.Drawing;

namespace NEA_ai_pathfinding
{
    // simple engine that owns the game rules (state + DB interactions)
    internal class GameEngine
    {
        private readonly SqlDatabase db;
        private readonly ChallengeMazeState state;

        public Action<string> ShowMessage;
        public Action Redraw;
        public Action CloseForm;
        public Action RunnerCaught;
        public Action<string> UpdateStatusLabel;
        public Action RefreshResources;

        public GameEngine(SqlDatabase database, ChallengeMazeState MazeFormState)
        {
            db = database;
            state = MazeFormState;
        }

        // Summary: prepare all values for a fresh run and align state with the new maze.
        public void StartGame(Maze maze, bool isBlocker, int timeLimitSeconds, Color baseColor)
        {
            // fresh state for a new run; keeps countdown and HUD aligned with maze
            state.ResetForNewGame(isBlocker, state.WallsLeft, baseColor);
            state.Maze = maze;
            state.PlayerX = maze.start.X;
            state.PlayerY = maze.start.Y;
            state.InGame = true;
            state.IsPaused = false;
            // StartTime is set when carving finishes so countdown is fair
            state.CountdownActive = false; // countdown starts when the maze is ready
            state.TimeLimitSeconds = timeLimitSeconds;
            state.RemainingSeconds = timeLimitSeconds;
            state.CurrentScore = 0;
            state.TimerAccumulator = 0.0;
            state.StartResetAfterCarve = false;
            state.StepsTaken = 0;
            state.TokensCollected = 0;
            if (maze != null)
            {
                state.ShortestPathLength = maze.ComputeShortestPathLength(maze.start, maze.exit);
            }
            Redraw?.Invoke();
            RefreshResources?.Invoke();
        }

        // used by Pause button
        public void SetPaused(bool paused)
        {
            state.IsPaused = paused;
        }

        // Guard: only tick the countdown when in-game and not paused.
        public void TickCountdown(double deltaSeconds)
        {
            if (!state.InGame || state.IsPaused)
            {
                return;
            }

            // only tick after carving is done
            if (!state.CountdownActive)
            {
                return;
            }

            double elapsed = (DateTime.UtcNow - state.StartTime).TotalSeconds;
            int remaining = state.TimeLimitSeconds - (int)Math.Floor(elapsed);

            if (remaining < 0)
            {
                remaining = 0;
            }

            state.RemainingSeconds = remaining;

            if (state.RemainingSeconds <= 0)
            {
                EndGame(GameEndReason.Timeout, 0.0, 0);
            }
        }

        // Summary: move the runner by dx,dy; ignores walls and paused state.
        public bool TryMoveRunner(int dx, int dy)
        {
            if (!state.InGame || state.IsPaused || state.Maze == null)
            {
                return false;
            }

            int nextX = state.PlayerX + dx;
            int nextY = state.PlayerY + dy;

            if (!state.Maze.IsPath(nextX, nextY))
            {
                return false;
            }

            state.PlayerX = nextX;
            state.PlayerY = nextY;
            state.StepsTaken = state.StepsTaken + 1;

            CollectTokenIfPresent();

            if (MonsterHelpers.IsOnMonster(state, state.PlayerX, state.PlayerY))
            {
                if (state.IsBlockerGame && RunnerCaught != null)
                {
                    RunnerCaught();
                }
                else
                {
            double seconds = (DateTime.UtcNow - state.StartTime).TotalSeconds;
            EndGame(GameEndReason.MonsterCaught, seconds, state.CurrentScore);
                }
                return true;
            }

            if (state.Maze.IsExit(state.PlayerX, state.PlayerY))
            {
            double seconds = (DateTime.UtcNow - state.StartTime).TotalSeconds;
            EndGame(GameEndReason.ReachedExit, seconds, state.CurrentScore);
            }

            Redraw?.Invoke();
            return true;
        }

        // used by AI runner steps
        public void SetRunnerPosition(Point pos)
        {
            if (state.Maze == null)
            {
                return;
            }

            state.PlayerX = pos.X;
            state.PlayerY = pos.Y;
            state.StepsTaken = state.StepsTaken + 1;
            CollectTokenIfPresent();

            if (MonsterHelpers.IsOnMonster(state, state.PlayerX, state.PlayerY))
            {
                if (state.IsBlockerGame && RunnerCaught != null)
                {
                    RunnerCaught();
                }
                else
                {
                    double seconds = (DateTime.Now - state.StartTime).TotalSeconds;
                    EndGame(GameEndReason.MonsterCaught, seconds, state.CurrentScore);
                }
                return;
            }

            if (state.Maze != null && state.Maze.IsExit(state.PlayerX, state.PlayerY))
            {
                double seconds = (DateTime.UtcNow - state.StartTime).TotalSeconds;
                EndGame(state.IsBlockerGame ? GameEndReason.ReachedExit : GameEndReason.RunnerEscaped, seconds, state.CurrentScore);
            }

            Redraw?.Invoke();
            RefreshResources?.Invoke();
        }

        // Summary: collects a token on the player's current cell and updates score/state.
        // Marks: keeps token removal central so both manual and AI movement stay consistent.
        private void CollectTokenIfPresent()
        {
            if (state.Maze == null || state.Maze.tokens == null)
            {
                return;
            }

            for (int i = state.Maze.tokens.Count - 1; i >= 0; i--)
            {
                var token = state.Maze.tokens[i];
                if (token.x == state.PlayerX && token.y == state.PlayerY)
                {
                    state.CurrentScore += token.value;
                    state.TokensCollected = state.TokensCollected + 1;
                    state.Maze.tokens.RemoveAt(i);
                    RefreshResources?.Invoke();
                }
            }
        }

        // Called when monster AI reports its result.
        public void HandleMonsterResult(MonsterStepResult result)
        {
            if (result == MonsterStepResult.CaughtRunner)
            {
                double seconds = (DateTime.Now - state.StartTime).TotalSeconds;
                EndGame(GameEndReason.MonsterCaught, seconds, state.CurrentScore);
            }
            else
            {
                Redraw?.Invoke();
            }
        }

        // Summary: shared finish logic; updates session arrays and shows status text.
        public void EndGame(GameEndReason reason, double elapsedSeconds, int score)
        {
            if (!state.InGame)
            {
                return;
            }

            if (reason == GameEndReason.MonsterCaught && !state.IsBlockerGame && !GameSession.IsLabMode)
            {
                // stop normal play flow; the form will reset on the next tick
                state.InGame = false;
                if (RunnerCaught != null)
                {
                    RunnerCaught();
                }
                return;
            }

            state.InGame = false;

            int level = GameSession.CurrentLevel;
            if (level < 1) level = 1;
            if (level > 8) level = 8;
            int index = level - 1;

            if (GameSession.IsLabMode)
            {
                string controller = GameSession.LabUseAgent ? "Runner AI" : "Manual runner";
                string summary = "";
                if (reason == GameEndReason.ReachedExit)
                {
                    summary = "Lab run finished. You reached the exit.";
                }
                else if (reason == GameEndReason.Timeout)
                {
                    summary = "Lab run ended. Timer hit zero.";
                }
                else if (reason == GameEndReason.MonsterCaught)
                {
                    summary = "Lab run ended. Monster caught the runner.";
                }
                else if (reason == GameEndReason.RunnerEscaped)
                {
                    summary = "Lab run ended. Runner escaped.";
                }
                else
                {
                    summary = "Lab run ended.";
                }

                string pathInfo = state.ShortestPathLength > 0 ? state.ShortestPathLength.ToString() + " steps" : "not known";
                string stats =
                    summary + "\n" +
                    "Controller: " + controller + "\n" +
                    "Time taken: " + elapsedSeconds.ToString("0.0") + "s\n" +
                    "Steps taken: " + state.StepsTaken + "\n" +
                    "Shortest path estimate: " + pathInfo + "\n" +
                    "Tokens collected: " + state.TokensCollected + " (score " + state.CurrentScore + ")";

                ShowMessage?.Invoke(stats);
                return;
            }

            string status = "Failed";
            switch (reason)
            {
                case GameEndReason.Timeout:
                    if (state.IsBlockerGame)
                    {
                        status = "Success";
                        GameSession.BlockerTimeoutWins[index] = true;
                        GameSession.BlockerLevelCompleted[index] = true;
                        GameSession.BlockerLevelRatings[index] = "D"; // timeout win is always grade D
                        GameSession.BlockerLevelTimes[index] = state.TimeLimitSeconds;
                        GameSession.BlockerLevelScores[index] = 0;
                        SaveScoreIfLoggedIn(state.TimeLimitSeconds, 0, level, "Blocker");
                        ShowMessage?.Invoke("Level cleared! Held runner for full timer.");
                        CloseForm?.Invoke();
                    }
                    else
                    {
                        status = "Failed";
                        GameSession.RunnerLevelRatings[index] = "Failed";
                        GameSession.RunnerLevelCompleted[index] = false;
                        GameSession.RunnerTimedOutFound[index] = true;
                        ShowMessage?.Invoke("Time's up! Try again.");
                        CloseForm?.Invoke();
                    }
                    break;
                case GameEndReason.ReachedExit:
                    if (state.IsBlockerGame)
                    {
                        status = "Failed";
                        GameSession.BlockerLevelCompleted[index] = false;
                        GameSession.BlockerLevelRatings[index] = "Failed";
                        GameSession.BlockerLevelTimes[index] = 0;
                        GameSession.BlockerLevelScores[index] = 0;
                        GameSession.BlockerTimeoutWins[index] = false;
                        ShowMessage?.Invoke("Runner reached the exit. You lose.");
                        CloseForm?.Invoke();
                    }
                    else
                    {
                        status = "Success";
                        GameSession.RunnerTimedOutFound[index] = false;
                        GameSession.StoreLevelResult(GameSession.CurrentRole, level, elapsedSeconds, score);
                        SaveScoreIfLoggedIn(elapsedSeconds, score, level, GameSession.CurrentRole);
                        ShowMessage?.Invoke($"Level cleared! Finished in {elapsedSeconds:F1}s");
                    }
                    break;
                case GameEndReason.RunnerEscaped:
                    // runner got away, mark as failed but keep level locked
                    status = "Failed";
                    GameSession.BlockerTimeoutWins[index] = false;
                    GameSession.BlockerLevelRatings[index] = "Failed";
                    GameSession.BlockerLevelTimes[index] = 0;
                    GameSession.BlockerLevelScores[index] = 0;
                    GameSession.BlockerLevelCompleted[index] = false;
                    ShowMessage?.Invoke("Level " + level + ": unsuccessful, Lost alone");
                    CloseForm?.Invoke();
                    break;
                case GameEndReason.MonsterCaught:
                    if (state.IsBlockerGame)
                    {
                        status = "Success";
                        GameSession.BlockerTimeoutWins[index] = false;
                        GameSession.StoreLevelResult("Blocker", level, elapsedSeconds, score);
                        SaveScoreIfLoggedIn(elapsedSeconds, score, level, "Blocker");
                        ShowMessage?.Invoke("Monster caught the runner. You win.");
                        CloseForm?.Invoke();
                    }
                    else
                    {
                        status = "Failed";
                        GameSession.BlockerLevelCompleted[index] = false;
                        GameSession.BlockerLevelRatings[index] = "Failed";
                        GameSession.BlockerLevelTimes[index] = 0;
                        GameSession.BlockerLevelScores[index] = 0;
                        GameSession.BlockerTimeoutWins[index] = false;
                        ShowMessage?.Invoke("Monster caught the runner! Maze resets.");
                    }
                    break;
                case GameEndReason.RunnerLivesDepleted:
                    if (state.IsBlockerGame)
                    {
                        status = "Success";
                        GameSession.BlockerTimeoutWins[index] = false;
                        GameSession.StoreLevelResult("Blocker", level, elapsedSeconds, score);
                        SaveScoreIfLoggedIn(elapsedSeconds, score, level, "Blocker");
                        ShowMessage?.Invoke("Runner ran out of lives. You win.");
                        CloseForm?.Invoke();
                    }
                    else
                    {
                        status = "Failed";
                        GameSession.RunnerLevelCompleted[index] = false;
                        GameSession.RunnerLevelRatings[index] = "Failed";
                        GameSession.RunnerLevelTimes[index] = 0;
                        GameSession.RunnerLevelScores[index] = 0;
                        GameSession.RunnerTimedOutFound[index] = false;
                        CloseForm?.Invoke();
                    }
                    break;
                case GameEndReason.FormClosed:
                    if (state.IsBlockerGame)
                    {
                        status = "Failed";
                        GameSession.BlockerLevelCompleted[index] = false;
                        GameSession.BlockerLevelRatings[index] = "Failed";
                        GameSession.BlockerLevelTimes[index] = 0;
                        GameSession.BlockerLevelScores[index] = 0;
                        GameSession.BlockerTimeoutWins[index] = false;
                    }
                    else
                    {
                        status = "Failed";
                        GameSession.RunnerLevelCompleted[index] = false;
                        GameSession.RunnerLevelRatings[index] = "Failed";
                        GameSession.RunnerLevelTimes[index] = 0;
                        GameSession.RunnerLevelScores[index] = 0;
                        GameSession.RunnerTimedOutFound[index] = false;
                    }
                    break;
            }
            UpdateStatusLabel?.Invoke(status);
        }

        private void SaveScoreIfLoggedIn(double seconds, int score, int level, string role)
        {
            string username = GameSession.CurrentUsername;
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }
            if (GameSession.IsLabMode)
            {
                return;
            }

            db.SaveUserScore(username, seconds, score, level, role);
            if (db.LastCallFailed)
            {
                ShowMessage?.Invoke("Database unavailable. Continuing in guest mode. Scores not saved.");
            }
        }
    }

    public enum GameEndReason
    {
        Timeout,
        ReachedExit,
        RunnerEscaped,
        MonsterCaught,
        RunnerLivesDepleted,
        FormClosed
    }

    // shared helper so both move paths can check all monsters
    internal static class MonsterHelpers
    {
        public static bool IsOnMonster(ChallengeMazeState state, int x, int y)
        {
            if (state == null)
            {
                return false;
            }

            if (state.Monsters != null && state.Monsters.Count > 0)
            {
                for (int i = 0; i < state.Monsters.Count; i++)
                {
                    var m = state.Monsters[i];
                    if (m.Active && m.X == x && m.Y == y)
                    {
                        return true;
                    }
                }
            }

            if (state.MonsterActive && state.MonsterX == x && state.MonsterY == y)
            {
                return true;
            }

            return false;
        }
    }
}
