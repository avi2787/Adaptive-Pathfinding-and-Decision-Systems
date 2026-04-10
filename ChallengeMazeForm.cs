using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace NEA_ai_pathfinding
{
    // Challenge mode gameplay surface + loop
    public partial class ChallengeMazeForm : Form
    {
        private readonly SqlDatabase db = new SqlDatabase();
        private readonly ChallengeMazeState state = new ChallengeMazeState();
        private const int TimerIntervalMs = 100;
        private const int DefaultRunnerLives = 3;
        private const int DashDistanceTiles = 5;
        private const int DashCostCoins = 5;
        private const double FallbackBraidProbability = 0.20;
        private const double FallbackRoomDensity = 0.10;
        private const int SolvableRetryLimit = 3;
        private const int ResetRebuildLimit = 5;
        private GameEngine engine;
        private MazeRenderer renderer;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private string currentUser = "";
        private int currentUserId = 0;
        private bool isBlocker;
        private int runnerLives = DefaultRunnerLives;
        private bool handlingCatch = false;
        private bool isResettingMaze = false;
        private string modeLabelBase = "";
        private int lastMoveDx = 0; // remember last move for dash
        private int lastMoveDy = 0;
        private DateTime lastWallMessage = DateTime.MinValue;
        private readonly Dictionary<string, DateTime> messageCooldowns = new Dictionary<string, DateTime>();
        private bool popupPauseActive = false;
        private DateTime pauseStarted = DateTime.MinValue;

        public ChallengeMazeForm()
        {
            InitializeComponent();
            KeyPreview = true;
            timer.Interval = TimerIntervalMs;
            timer.Tick += GameTimer_Tick;
            this.KeyDown += Form1_KeyDown;
            pbMaze.Paint += PbMaze_Paint;
            pbMaze.MouseClick += pbMaze_MouseClick;
            this.FormClosed += ChallengeMazeForm_FormClosed;
            panelMaze.Visible = true;

            string role = GameSession.CurrentRole;
            if (string.IsNullOrWhiteSpace(role))
            {
                role = "";
            }
            string roleLower = role.ToLower();
            isBlocker = roleLower == "blocker";

            Text = isBlocker ? "Lost? (Blocker view)" : "Lost? (Runner view)";
            // hide blocker-only buttons when playing as runner
            if (btnPlaceWall != null) btnPlaceWall.Visible = isBlocker;
            if (btnWind != null) btnWind.Visible = isBlocker;
            if (btnMonster != null) btnMonster.Visible = isBlocker;

            ApplyMazeColour();
            state.IsBlockerGame = isBlocker;
            state.BlockerAgentActive = false;
            UpdateLivesLabel();


            string username = GameSession.CurrentUsername;
            if (!string.IsNullOrWhiteSpace(username))
            {
                int id = db.GetUserId(username);
                SetCurrentUser(username, id);
                if (db.LastCallFailed)
                {
                    MessageBox.Show("Database unavailable. Continuing in guest mode.");
                    GameSession.CurrentUsername = "";
                    lblUser.Text = "User: guest";
                }
            }
            else
            {
                lblUser.Text = "User: (not logged in)";
            }
            SetupModeLabel();

            // Marks: keep engine/render wiring together so dependencies are easy to follow.
            engine = new GameEngine(db, state);
            renderer = new MazeRenderer(state);
            engine.ShowMessage = ShowMessageBox;
            engine.Redraw = () => pbMaze.Invalidate();
            engine.CloseForm = () => this.Close();
            engine.RunnerCaught = HandleRunnerCaught;
            engine.UpdateStatusLabel = UpdateStatusLabel;
            engine.RefreshResources = UpdateResourceLabels;
            // keep countdown label on top of the maze surface
            if (lblCountdown != null && panelMaze != null)
            {
                lblCountdown.Parent = panelMaze;
                lblCountdown.BringToFront();
            }
            // hide blocker move label when blocker is the player (no need to show their own choices)
            if (lbl_DisplayMove != null) lbl_DisplayMove.Visible = !isBlocker;
            if (pbMaze != null)
            {
                pbMaze.SendToBack(); // ensure HUD labels are above the maze drawing
            }
            UpdateResourceLabels();
        }


        private void ShowMessageBox(string msg)
        {
            ShowGameplayMessage(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // pause-safe wrapper so popups do not let the game keep ticking underneath
        // Summary: pause timer + input during popups so gameplay doesn't keep ticking underneath.
        private DialogResult ShowGameplayMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            bool timerWasRunning = timer != null && timer.Enabled;
            bool wasPaused = state.IsPaused;

            if (popupPauseActive)
            {
                // already paused by another popup, just show it without changing state
                return MessageBox.Show(this, text, caption, buttons, icon);
            }

            popupPauseActive = true;
            if (timer != null) timer.Stop();
            state.IsPaused = true;

            var result = MessageBox.Show(this, text, caption, buttons, icon);

            if (timerWasRunning && timer != null)
            {
                timer.Start();
            }
            if (!wasPaused)
            {
                state.IsPaused = false;
            }

            popupPauseActive = false;
            return result;
        }

        // throttle duplicate popups by key
        private bool CanShowMessage(string key, int cooldownMs = 400)
        {
            DateTime now = DateTime.Now;
            if (messageCooldowns.TryGetValue(key, out DateTime last))
            {
                if ((now - last).TotalMilliseconds < cooldownMs)
                {
                    return false;
                }
            }
            messageCooldowns[key] = now;
            return true;
        }

        public void SetCurrentUser(string username, int userId)
        {
            currentUser = username;
            currentUserId = userId;
            lblUser.Text = "User: " + username;
        }

        public int[] GetLevelDimensions(int level)
        {
            if (GameSession.IsLabMode)
            {
                int lw = GameSession.LabMazeWidth;
                int lh = GameSession.LabMazeHeight;
                if (lw < 5) lw = 21;
                if (lh < 5) lh = 21;
                return new int[] { lh, lw };
            }

            int h = 21;
            int w = 21;

            if (level == 2 || level == 3)
            {
                h = 31;
                w = 21;
            }

            if (level == 4 || level == 5)
            {
                h = 41;
                w = 41;
            }

            if (level == 6 || level == 7)
            {
                h = 41;
                w = 51;
            }

            if (level == 8)
            {
                h = 75;
                w = 75;
            }

            return new int[] { h, w };
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int level = GameSession.CurrentLevel;
            int[] dims = GetLevelDimensions(level);
            int h = dims[0];
            int w = dims[1];
            runnerLives = DefaultRunnerLives; // restart lives each level
            UpdateLivesLabel();

            if (w % 2 == 0)
            {
                w = w + 1;
            }
            if (h % 2 == 0)
            {
                h = h + 1;
            }

            Maze maze = new Maze(w, h);
            maze.Level = level;
            // Early levels: loopier and roomier; turn bias starts lower so paths are straighter.
            // level-scaling: loopiness and rooms drop roughly 2.5%/1.2% per level
            double braid = GameSession.IsLabMode ? GameSession.LabBraidProbability : 0.28 - 0.025 * (level - 1);
            double roomDensity = GameSession.IsLabMode ? GameSession.LabRoomDensity : 0.16 - 0.012 * (level - 1);
            if (!GameSession.IsLabMode)
            {
                if (braid < 0.08) braid = 0.08;      // keep some loops later
                if (roomDensity < 0.05) roomDensity = 0.05;
            }
            maze.StartStepGeneration(braid, roomDensity, GameSession.IsLabMode ? GameSession.LabStepsPerTick : 12);
            int timeLimit = GameSession.GetTimeLimitSeconds(GameSession.CurrentRole, level);
            // make challenge timers tighter so difficulty ramps
            timeLimit = (int)(timeLimit * 0.7);
            if (timeLimit < 45)
            {
                timeLimit = 45;
            }

            state.WallsLeft = isBlocker ? Math.Min(level, 10) : 0;

            engine.StartGame(maze, isBlocker, timeLimit, state.MazeBaseColor);
            // init countdown once; will start ticking after carving finishes
            state.TimeLimitSeconds = timeLimit;
            state.RemainingSeconds = timeLimit;
            // StartTime intentionally not set here so countdown begins when carving ends
            state.CountdownActive = false;
            state.IsCarving = true;
            state.StartResetAfterCarve = false;
            UpdateCountdownLabel();

            if (isBlocker)
            {
                state.BlockerHelper = new BlockerHeuristicController();
                state.BlockerHelper.AttachMaze(maze);
                state.BlockerService = new BlockerAbilityService(state);
                state.BlockerService.ConfigureForLevel(level, state.BlockerHelper);
                UpdateResourceLabels();

                state.RunnerAgent = new RunnerQAgent();
                state.RunnerAgent.AttachMaze(maze, new Point(state.PlayerX, state.PlayerY));
                state.RunnerAgent.ConfigureForLevel(level); // level-based speed + learning now handled inside
            }
            else
            {
                // when player is runner, use the simple blocker helper
                state.BlockerHelper = new BlockerHeuristicController();
                state.BlockerHelper.AttachMaze(maze);
                state.BlockerService = new BlockerAbilityService(state);
                state.BlockerService.ConfigureForLevel(level, state.BlockerHelper);
                if (GameSession.IsLabMode && GameSession.LabUseAgent)
                {
                    state.RunnerAgent = new RunnerQAgent();
                    state.RunnerAgent.AttachMaze(maze, new Point(state.PlayerX, state.PlayerY));
                    state.RunnerAgent.ConfigureCustom(GameSession.LabAgentLearning, GameSession.LabAgentDiscount, GameSession.LabAgentExploration, GameSession.LabAgentGreedy);
                    state.RunnerAgentChosen = true;
                }
                else
                {
                    state.RunnerAgent = null;
                    state.RunnerAgentChosen = false;
                }
            }

            UpdateResourceLabels();

            state.Monsters.Clear();
            state.MonsterActive = false;
            state.MonsterPrevX = -1;
            state.MonsterPrevY = -1;
            state.MonsterStepCounter = 0;
            state.RunnerStepCounter = 0;
            state.BlockerAgentStepCounter = 0;

            if (btnPause != null)
            {
                btnPause.Text = "Pause";
            }

            timer.Start();
            pbMaze.Invalidate();
        }

        // Timer tick drives carving, countdown, AI, monsters. Guards stop re-entry and avoid running during popups/resets.
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // exit early if rebuild/pause/game over to keep timing clean
            if (isResettingMaze)
            {
                return; // never tick while rebuilding the maze
            }

            if (!state.InGame)
            {
                return;
            }

            if (state.IsPaused)
            {
                return;
            }

            // countdown only ticks when allowed; label still refreshes once per tick
            if (!state.IsCarving)
            {
                engine.TickCountdown(TimerIntervalMs / 1000.0);
            }
            UpdateCountdownLabel();

            bool finishedCarve = false;
            if (state.IsCarving && state.Maze != null)
            {
                bool more = state.Maze.StepGenerate();
                // do not place runner until carving ends
                pbMaze.Invalidate();
                if (!more)
                {
                    state.IsCarving = false;
                    finishedCarve = true;
                }
                else
                {
                    return;
                }
            }

            if (finishedCarve && state.Maze != null)
            {
                int level = GameSession.CurrentLevel;
                int attempts = 0;
                while (attempts < SolvableRetryLimit && !state.Maze.IsSolvable())
                {
                    // quick safety loop so every generated maze is actually solvable before play starts
                    state.Maze.Generate(FallbackBraidProbability, FallbackRoomDensity);
                    // keep runner off-grid until ready
                    // maze layout changed, reattach helpers so they don't use stale data
                    ReattachControllersAfterMazeChange(level);
                    attempts++;
                }

                if (!state.Maze.IsSolvable())
                {
                    state.InGame = false;
                    timer.Stop();
                    ShowGameplayMessage("Could not generate a solvable maze. Please try again.", "Maze", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // now that carving + solvable check are done, place runner at start
                state.PlayerX = state.Maze.start.X;
                state.PlayerY = state.Maze.start.Y;
                engine.SetRunnerPosition(new Point(state.PlayerX, state.PlayerY));
                state.StartResetAfterCarve = false;
                // start countdown now the maze is playable
                state.StartTime = DateTime.UtcNow; // Marks: countdown starts only when maze is ready
                state.RemainingSeconds = state.TimeLimitSeconds; // keep label synced from the start point
                state.TimerAccumulator = 0.0;
                state.CountdownActive = true;
                UpdateCountdownLabel(); // refresh immediately as play begins
            }

            if (state.BlockerService != null)
            {
                bool removed = state.BlockerService.ExpireWalls(state.Maze, DateTime.Now);
                if (removed)
                {
                    pbMaze.Invalidate();
                }
            }

            HandleMonsterSteps();

            // Only run the runner AI for Blocker games (Challenge mode)
            if (isBlocker)
            {
                if (state.RunnerAgent != null)
                {
                    // Challenge AI speeds up by level so early levels stay fair
                    int level = GameSession.CurrentLevel;
                    if (level < 1) level = 1;
                    if (level > 5) level = 5; // tuning is aimed at first 5 levels

                    // gate movement: early levels allowed to think a bit more, later levels slowed
                    state.RunnerStepCounter++;
                    int gate = 2 + level / 2; // grows with level so higher levels move less often
                    if (gate > 5) gate = 5;
                    if (state.RunnerStepCounter % gate != 0)
                    {
                        return; // skip this tick to keep AI slower on later levels
                    }

                    // single step per tick to keep pace gentle even when gate opens
                    int stepsThisTick = 1;

                    state.RunnerAgent.SetDangerCells(GetMonsterCells());

                    for (int i = 0; i < stepsThisTick; i++)
                    {
                        state.RunnerAgent.PerformLearningStep();
                    }

                    Point newRunnerPosition = state.RunnerAgent.GetCurrentPosition();
                    // if AI runner is about to step onto a monster, handle catch here
                    if (MonsterHelpers.IsOnMonster(state, newRunnerPosition.X, newRunnerPosition.Y))
                    {
                        HandleRunnerCaught();
                        return;
                    }
                    // In blocker view, the runner is AI. Keep state coords synced because
                    // monster + other logic reads state.PlayerX/Y as "runner position".
                    state.PlayerX = newRunnerPosition.X;
                    state.PlayerY = newRunnerPosition.Y;
                    engine.SetRunnerPosition(newRunnerPosition);
                }
            }

            // Blocker heuristic acts when the player is the runner
            if (!isBlocker)
            {
                if (state.BlockerHelper != null && state.BlockerService != null && state.Maze != null)
                {
                    state.BlockerAgentStepCounter = state.BlockerAgentStepCounter + 1;
                    if (state.BlockerAgentStepCounter >= 4)
                    {
                        state.BlockerAgentStepCounter = 0;
                        var obs = new BlockerObservation(
                            state.Maze,
                            new Point(state.PlayerX, state.PlayerY),
                            state.Maze.exit,
                            state.MonsterActive,
                            state.WallsLeft);

                        BlockerDecision decision = state.BlockerHelper.DecideNextAction(obs);

                        if (decision != null && decision.ActionType != BlockerActionType.None)
                        {
            DateTime now = DateTime.Now;
                            if (decision.ActionType == BlockerActionType.PlaceWall && decision.TargetCell != Point.Empty)
                            {
                                var wallResult = state.BlockerService.TryPlaceWall(state.Maze, new Point(state.PlayerX, state.PlayerY), decision.TargetCell, now);
                                if (wallResult == PlaceWallResult.Success)
                                {
                                    ShowBlockerMove("Place a wall");
                                }
                            }
                            else if (decision.ActionType == BlockerActionType.UseWindPush && decision.TargetCell != Point.Empty)
                            {
                                int dirX = Math.Sign(decision.TargetCell.X - state.PlayerX);
                                int dirY = Math.Sign(decision.TargetCell.Y - state.PlayerY);
                                if (dirX != 0 && dirY != 0)
                                {
                                    dirX = 0; dirY = 0; // ignore diagonals
                                }
                                if (dirX != 0 || dirY != 0)
                                {
                                    bool pushed = DoWindPush(dirX, dirY, false);
                                    if (pushed)
                                    {
                                        ShowBlockerMove("Use Wind Push");
                                    }
                                }
                            }
                            else if (decision.ActionType == BlockerActionType.SpawnMonster)
                            {
                                var spawnResult = state.BlockerService.TrySpawnMonster(now, true);
                                if (spawnResult == SpawnResult.Success)
                                {
                                    ShowBlockerMove("Place Monster");
                                }
                            }
                        }
                    }
                }
            }
        }

        // quick helper so the blocker agent's last move is visible
        private void ShowBlockerMove(string moveText)
        {
            if (lbl_DisplayMove == null)
            {
                return;
            }

            lbl_DisplayMove.Text = "Blocker chose to- " + moveText;
        }

        private void lbl_DisplayMove_Click(object sender, EventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (HandleMoveKey(e.KeyCode))
            {
                e.Handled = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // this makes arrow keys (and dash) work even if a button has focus
            bool handled = HandleMoveKey(keyData);
            if (handled)
            {
                return true;
            }

            // stop Enter from triggering main buttons; allow mouse only
            if (keyData == Keys.Enter || keyData == Keys.Return)
            {
                var ctrl = this.ActiveControl;
                if (ctrl == btnStart || ctrl == btnPause || ctrl == btnBackToMenu)
                {
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool HandleMoveKey(Keys key)
        {
            if (!state.InGame || state.Maze == null)
            {
                return false;
            }

            if (state.IsPaused)
            {
                return false;
            }

            if (state.IsCarving)
            {
                return false; // ignore input until maze is ready
            }

            if (isResettingMaze)
            {
                return false; // ignore input while maze is rebuilding
            }

            if (isBlocker)
            {
                if (state.CurrentBlockerAction == BlockerActionType.UseWindPush)
                {
                    if (!TryGetDirectionFromKey(key, out int pushDx, out int pushDy))
                    {
                        return false;
                    }
                    DoWindPush(pushDx, pushDy, true);
                    return true;
                }
                return false;
            }

            if (key == Keys.Enter || key == Keys.Return)
            {
                return TryDash();
            }

            int dx = 0;
            int dy = 0;

            if (key == Keys.D || key == Keys.Right)
            {
                dx = 1;
            }
            else if (key == Keys.A || key == Keys.Left)
            {
                dx = -1;
            }
            else if (key == Keys.W || key == Keys.Up)
            {
                dy = -1;
            }
            else if (key == Keys.S || key == Keys.Down)
            {
                dy = 1;
            }
            else
            {
                return false;
            }

            if (engine.TryMoveRunner(dx, dy))
            {
                lastMoveDx = dx;
                lastMoveDy = dy;
                UpdateResourceLabels();
            }

            // still remember intent so dash can use it even if we hit a wall
            lastMoveDx = dx;
            lastMoveDy = dy;
            return true;
        }

        // map keys to a push direction
        private bool TryGetDirectionFromKey(Keys key, out int dx, out int dy)
        {
            dx = 0;
            dy = 0;
            if (key == Keys.W || key == Keys.Up)
            {
                dy = -1;
                return true;
            }
            if (key == Keys.S || key == Keys.Down)
            {
                dy = 1;
                return true;
            }
            if (key == Keys.A || key == Keys.Left)
            {
                dx = -1;
                return true;
            }
            if (key == Keys.D || key == Keys.Right)
            {
                dx = 1;
                return true;
            }
            return false;
        }

        private bool DoWindPush(int dx, int dy, bool fromHuman)
        {
            if (state.BlockerService == null || state.Maze == null)
            {
                return false;
            }
            if (!state.InGame || state.IsPaused)
            {
                return false;
            }

            List<Point> path;
            WindResult windResult = state.BlockerService.TryWindPush(
                state.Maze,
                new Point(state.PlayerX, state.PlayerY),
                dx,
                dy,
                DateTime.Now,
                out path);

            if (windResult == WindResult.Cooldown)
            {
                if (fromHuman && CanShowMessage("wind_cooldown")) ShowGameplayMessage("Wind is recharging.", "Wind", MessageBoxButtons.OK, MessageBoxIcon.Information);
                state.CurrentBlockerAction = BlockerActionType.None;
                return false;
            }
            if (windResult == WindResult.NotAdjacent)
            {
                if (fromHuman && CanShowMessage("wind_direction")) ShowGameplayMessage("Press a single direction (WASD or arrows).", "Wind", MessageBoxButtons.OK, MessageBoxIcon.Information);
                state.CurrentBlockerAction = BlockerActionType.None;
                return false;
            }
            if (windResult == WindResult.Blocked)
            {
                if (fromHuman && CanShowMessage("wind_blocked")) ShowGameplayMessage("Blocked by a wall.", "Wind", MessageBoxButtons.OK, MessageBoxIcon.Information);
                state.CurrentBlockerAction = BlockerActionType.None;
                return false;
            }
            if (windResult == WindResult.NoCharges)
            {
                if (fromHuman && CanShowMessage("wind_nocharges")) ShowGameplayMessage("No wind pushes left this level.", "Wind", MessageBoxButtons.OK, MessageBoxIcon.Information);
                state.CurrentBlockerAction = BlockerActionType.None;
                return false;
            }

            AnimateWindPush(path);
            state.CurrentBlockerAction = BlockerActionType.None;
            UpdateResourceLabels();
            return true;
        }

        private void AnimateWindPush(List<Point> path)
        {
            if (path == null || path.Count == 0)
            {
                return;
            }

            for (int i = 0; i < path.Count; i++)
            {
                var step = path[i];
                state.PlayerX = step.X;
                state.PlayerY = step.Y;
                engine.SetRunnerPosition(step);
                if (!state.InGame)
                {
                    return; // engine ended the run (monster or exit)
                }
                UpdateResourceLabels();
                pbMaze.Invalidate();

                Application.DoEvents(); // keep UI responsive during the shove
                Thread.Sleep(25);       // small pause so it looks like a push, not a teleport
            }
        }

        private bool TryDash()
        {
            // dash only for runner
            if (isBlocker || state.Maze == null)
            {
                return false;
            }
            if (lastMoveDx == 0 && lastMoveDy == 0)
            {
                return false; // no direction yet
            }
            if (state.CurrentScore < DashCostCoins)
            {
                // tell player why dash failed
                if (CanShowMessage("dash_cost"))
                {
                    ShowGameplayMessage("Dash costs " + DashCostCoins + " coins.", "Dash", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return false; // not enough points to dash
            }

            int x = state.PlayerX;
            int y = state.PlayerY;
            Point target = new Point(x, y);
            bool killedMonster = false;

            bool startNextIsWall = !state.Maze.IsPath(state.PlayerX + lastMoveDx, state.PlayerY + lastMoveDy);
            int maxSteps = DashDistanceTiles; // dash distance now fixed

            for (int i = 0; i < maxSteps; i++)
            {
                int nx = x + lastMoveDx;
                int ny = y + lastMoveDy;
                if (nx < 0 || ny < 0 || nx >= state.Maze.width || ny >= state.Maze.height)
                {
                    break;
                }

                x = nx;
                y = ny;

                if (state.Maze.IsPath(x, y))
                {
                    target = new Point(x, y);
                    if (state.Monsters != null)
                    {
                        for (int m = state.Monsters.Count - 1; m >= 0; m--)
                        {
                            if (state.Monsters[m].X == x && state.Monsters[m].Y == y)
                            {
                                state.Monsters.RemoveAt(m); // dash kills monster on any touched path cell
                                killedMonster = true;
                            }
                        }
                    }
                    // if we're on a clear path and didn't come from a wall, allow up to 5 tiles total
                    if (!startNextIsWall && i < maxSteps - 1)
                    {
                        // keep stepping while the path stays open
                        continue;
                    }
                    break;
                }

                if (!startNextIsWall)
                {
                    break; // first move was on path; stop if you hit a wall
                }
            }

            if (target == new Point(state.PlayerX, state.PlayerY))
            {
                return false; // nowhere to dash
            }

            state.CurrentScore = Math.Max(0, state.CurrentScore - DashCostCoins); // pay dash cost
            state.PlayerX = target.X;
            state.PlayerY = target.Y;
            engine.SetRunnerPosition(target); // engine handles tokens/exit/monster checks
            // clear any monster on landing cell
            if (state.Monsters != null)
            {
                for (int m = state.Monsters.Count - 1; m >= 0; m--)
                {
                    if (state.Monsters[m].X == target.X && state.Monsters[m].Y == target.Y)
                    {
                        state.Monsters.RemoveAt(m);
                        killedMonster = true;
                    }
                }
            }
            if (killedMonster)
            {
                ShowGameplayMessage("Monster defeated by dash!", "Monster", MessageBoxButtons.OK, MessageBoxIcon.Information);
                state.MonsterActive = state.Monsters != null && state.Monsters.Count > 0;
                if (!state.MonsterActive)
                {
                    state.MonsterX = -1;
                    state.MonsterY = -1;
                }
            }
            UpdateResourceLabels();
            return true;
        }

        // paint uses renderer so this stays small
        private void PbMaze_Paint(object sender, PaintEventArgs e)
        {
            renderer.Draw(e, pbMaze);
        }

        // Summary: when monster catches the runner, rebuild a solvable maze but keep the timer/lives state.
        private void ResetAfterMonsterCatch()
        {
            isResettingMaze = true;
            state.IsPaused = true; // block input while rebuilding
            timer.Stop(); // stop ticks during rebuild

            int level = GameSession.CurrentLevel;

            try
            {
                // keep the countdown exactly where it was
                int timeLimit = state.TimeLimitSeconds;
                int remaining = state.RemainingSeconds;

                int[] dims = GetLevelDimensions(level);
                int h = dims[0];
                int w = dims[1];

                if (w % 2 == 0) w++;
                if (h % 2 == 0) h++;

                Maze maze = new Maze(w, h);
                maze.Level = level;

                int attempts = 0;
                do
                {
                    maze.Generate(FallbackBraidProbability, FallbackRoomDensity);
                    attempts++;
                }
                while (attempts < ResetRebuildLimit && !maze.IsSolvable());

                // swap to new maze properly
                engine.StartGame(maze, isBlocker, timeLimit, state.MazeBaseColor);

                // make sure runner coords match the new maze start
                state.PlayerX = maze.start.X;
                state.PlayerY = maze.start.Y;
                engine.SetRunnerPosition(new Point(state.PlayerX, state.PlayerY));

                // restore the timer values (StartGame may overwrite them)
                state.TimeLimitSeconds = timeLimit;
                state.RemainingSeconds = remaining;
                state.StartTime = DateTime.UtcNow - TimeSpan.FromSeconds(timeLimit - remaining);
                state.TimerAccumulator = 0.0;
                state.CountdownActive = true; // keep countdown running after reset

                // instant reset, not carving
                state.IsCarving = false;
                state.StartResetAfterCarve = false;

                // clear temporary effects
                state.TempWalls.Clear();
                state.WallCooldownUntil = DateTime.MinValue;
                state.WindCooldownUntil = DateTime.MinValue;
                state.MonsterCooldownUntil = DateTime.MinValue;

                // monster resets
                state.Monsters.Clear();
                state.MonsterActive = false;
                state.MonsterPrevX = -1;
                state.MonsterPrevY = -1;
                state.MonsterStepCounter = 0;
                state.MonsterX = -1;
                state.MonsterY = -1;
                UpdateResourceLabels();

                // reconfigure blocker stuff + runner AI if needed
                if (isBlocker)
                {
                    if (state.BlockerHelper == null) state.BlockerHelper = new BlockerHeuristicController();
                    state.BlockerHelper.AttachMaze(maze);

                    if (state.BlockerService == null) state.BlockerService = new BlockerAbilityService(state);
                    state.BlockerService.ConfigureForLevel(level, state.BlockerHelper);
                    UpdateResourceLabels();

                    if (state.RunnerAgent == null) state.RunnerAgent = new RunnerQAgent();
                    state.RunnerAgent.AttachMaze(maze, new Point(state.PlayerX, state.PlayerY));
                    state.RunnerAgent.ConfigureForLevel(level);
                }
                else
                {
                    // player is runner: keep blocker AI active after each reset
                    if (state.BlockerHelper == null) state.BlockerHelper = new BlockerHeuristicController();
                    state.BlockerHelper.AttachMaze(maze);

                    if (state.BlockerService == null) state.BlockerService = new BlockerAbilityService(state);
                    state.BlockerService.ConfigureForLevel(level, state.BlockerHelper);
                }

                pbMaze.Invalidate();
            }
            finally
            {
                state.BlockerAgentStepCounter = 0; // ensure blocker AI keeps ticking after resets
                state.IsPaused = false;
                isResettingMaze = false;
                state.InGame = true; // keep tick loop active after rebuild
                if (!this.IsDisposed)
                {
                    timer.Start(); // always restart timer after rebuild
                }
            }
        }

        // stub handlers 
        private void panelMaze_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblWallsLeft_Click(object sender, EventArgs e)
        {

        }
        
        private void pbMaze_Click(object sender, EventArgs e)
        {

        }

        private void lblUser_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

       

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (!state.InGame)
            {
                return;
            }

            if (!state.IsPaused)
            {
                state.IsPaused = true;
                timer.Stop();
                pauseStarted = DateTime.UtcNow;
                if (btnPause != null)
                {
                    btnPause.Text = "Resume";
                }
                if (!Text.EndsWith(" (Paused)"))
                {
                    Text = Text + " (Paused)";
                }
            }
            else
            {
                state.IsPaused = false;
                if (pauseStarted != DateTime.MinValue && state.StartTime != DateTime.MinValue)
                {
                    var pausedSpan = DateTime.UtcNow - pauseStarted;
                    state.StartTime = state.StartTime.Add(pausedSpan);
                }
                pauseStarted = DateTime.MinValue;
                timer.Start();
                if (btnPause != null)
                {
                    btnPause.Text = "Pause";
                }
                if (Text.EndsWith(" (Paused)"))
                {
                    Text = Text.Substring(0, Text.Length - " (Paused)".Length);
                }
            }
        }

        private void btnPlaceWall_Click(object sender, EventArgs e)
        {
            if (btnPlaceWall != null && !btnPlaceWall.Visible)
            {
                return;
            }
            if (!isBlocker)
            {
                return;
            }

            state.CurrentBlockerAction = BlockerActionType.PlaceWall;
            // action will reset after the next maze click
        }

        private void btnWind_Click(object sender, EventArgs e)
        {
            if (btnWind != null && !btnWind.Visible)
            {
                return;
            }
            if (!isBlocker)
            {
                return;
            }

            state.CurrentBlockerAction = BlockerActionType.UseWindPush;
            // action will reset after the next maze click
        }

        private void btnMonster_Click(object sender, EventArgs e)
        {
            if (btnMonster != null && !btnMonster.Visible)
            {
                return;
            }
            if (!isBlocker || state.IsPaused || !state.InGame || state.Maze == null)
            {
                return;
            }

            if (state.BlockerService == null)
            {
                return;
            }

            SpawnResult result = state.BlockerService.TrySpawnMonster(DateTime.Now, false);
            if (result == SpawnResult.NoPath)
            {
                ShowGameplayMessage("Monster could not spawn (maze might still be carving or limit reached).", "Monster", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            state.MonsterActive = state.Monsters.Count > 0;
            UpdateResourceLabels();
            pbMaze.Invalidate();
        }

        private void btnBackToMenu_Click(object sender, EventArgs e)
        {
            timer.Stop();
            if (state.InGame)
            {
                engine.EndGame(GameEndReason.FormClosed, 0, state.CurrentScore);
            }
            this.Close();
        }

        private void ChallengeMazeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!state.InGame)
            {
                if (timer != null)
                {
                    timer.Stop();
                }
                return;
            }

            if (timer != null)
            {
                timer.Stop();
            }
            engine.EndGame(GameEndReason.FormClosed, 0, 0);
            state.InGame = false; // stop duplicate EndGame calls
        }

        private void pbMaze_MouseClick(object sender, MouseEventArgs e)
        {
            if (!state.InGame || state.Maze == null || state.IsPaused || !isBlocker)
            {
                return;
            }
            if (isResettingMaze)
            {
                return; // ignore clicks while rebuilding
            }

            int cellW = Math.Max(1, pbMaze.Width / state.Maze.width);
            int cellH = Math.Max(1, pbMaze.Height / state.Maze.height);

            int cellX = e.X / cellW;
            int cellY = e.Y / cellH;

            if (cellX < 0 || cellY < 0 || cellX >= state.Maze.width || cellY >= state.Maze.height)
            {
                return;
            }

            Point runnerCell = new Point(state.PlayerX, state.PlayerY);
            Point clickedCell = new Point(cellX, cellY);
            DateTime now = DateTime.Now;

            if (state.CurrentBlockerAction == BlockerActionType.PlaceWall)
            {
                if (state.BlockerService == null)
                {
                    return;
                }

                PlaceWallResult placeResult = state.BlockerService.TryPlaceWall(state.Maze, runnerCell, clickedCell, now);
                if (placeResult == PlaceWallResult.NoWalls)
                {
                    if (CanShowMessage("walls_nowalls")) ShowGameplayMessage("No walls left for this level.", "Walls", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (placeResult == PlaceWallResult.Cooldown)
                {
                    if (CanShowMessage("walls_cooldown")) ShowGameplayMessage("Wall is cooling down, wait a moment.", "Walls", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (placeResult == PlaceWallResult.NotAPath)
                {
                    // throttle duplicate popups if player double-clicks
                    if ((DateTime.Now - lastWallMessage).TotalMilliseconds > 400)
                    {
                        ShowGameplayMessage("Wall must go on a path cell.", "Walls", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        lastWallMessage = DateTime.Now;
                    }
                    return;
                }

                UpdateResourceLabels();
                pbMaze.Invalidate();
                state.CurrentBlockerAction = BlockerActionType.None; // reset after one wall
            }
            else if (state.CurrentBlockerAction == BlockerActionType.UseWindPush)
            {
                if (CanShowMessage("wind_prompt")) ShowGameplayMessage("Use WASD or arrow keys to pick a push direction.", "Wind", MessageBoxButtons.OK, MessageBoxIcon.Information);
                state.CurrentBlockerAction = BlockerActionType.None;
            }
        }

        private void ReattachControllersAfterMazeChange(int level)
        {
            if (state.Maze == null)
            {
                return;
            }

            // keep engine and state lined up
            engine.SetRunnerPosition(new Point(state.PlayerX, state.PlayerY));

            if (state.BlockerHelper == null) state.BlockerHelper = new BlockerHeuristicController();
            state.BlockerHelper.AttachMaze(state.Maze);

            if (state.BlockerService == null) state.BlockerService = new BlockerAbilityService(state);
            state.BlockerService.ConfigureForLevel(level, state.BlockerHelper);

            if (state.RunnerAgent != null)
            {
                Point pos = new Point(state.PlayerX, state.PlayerY);
                state.RunnerAgent.AttachMaze(state.Maze, pos);
                state.RunnerAgent.StartNewEpisode(pos);
            }

            state.Monsters.Clear();
            state.MonsterActive = false;
            state.MonsterPrevX = -1;
            state.MonsterPrevY = -1;
            state.MonsterStepCounter = 0;
            UpdateResourceLabels();
        }
        private void UpdateCountdownLabel()
        {
            if (lblCountdown == null || lblCountdown.IsDisposed)
            {
                return;
            }

            if (state.RemainingSeconds < 0)
            {
                state.RemainingSeconds = 0;
            }

            lblCountdown.Visible = true;
            lblCountdown.Text = "Time left: " + state.RemainingSeconds + "s";
            lblCountdown.Refresh(); // repaint just the label so it keeps up during heavy draws
        }

        private void HandleRunnerCaught()
        {
            if (handlingCatch)
            {
                return; // avoid double popups if multiple catches happen quickly
            }
            handlingCatch = true;
            try
            {
                timer.Stop(); // pause countdown while dialog is open
                state.IsPaused = true;
                runnerLives = Math.Max(0, runnerLives - 1); // always drop a life so the label stays honest
                UpdateLivesLabel();
                if (runnerLives <= 0)
                {
                    timer.Stop();
                    state.IsPaused = true;
                    double elapsed = state.TimeLimitSeconds - state.RemainingSeconds;
                    if (isBlocker)
                    {
                        UpdateStatusLabel("Success");
                        var savedWin = engine.RunnerCaught;
                        engine.RunnerCaught = null; // avoid recursion
                        engine.EndGame(GameEndReason.RunnerLivesDepleted, elapsed, state.CurrentScore);
                        engine.RunnerCaught = savedWin;
                        Close();
                        return;
                    }

                    UpdateStatusLabel("Failed");
                    ShowGameplayMessage("Game over. No lives left.", "Lives", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    var saved = engine.RunnerCaught;
                    engine.RunnerCaught = null; // avoid recursion
                    engine.EndGame(GameEndReason.RunnerLivesDepleted, elapsed, state.CurrentScore);
                    engine.RunnerCaught = saved;
                    Close();
                    return;
                }

                string livesText = "Lives left: " + runnerLives + ". Maze will reset.";
                string catchMsg = isBlocker
                    ? "Runner caught by the monster. " + livesText
                    : "Caught by the monster! " + livesText;
                ShowGameplayMessage(catchMsg, "Caught", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ResetAfterMonsterCatch();
                UpdateCountdownLabel(); // keep time label in sync after pause
                return;
            }
            finally
            {
                handlingCatch = false; // never leave the flag stuck
            }
        }

        private void HandleMonsterSteps()
        {
            if (state.Monsters == null)
            {
                state.Monsters = new System.Collections.Generic.List<MonsterController>();
            }

            // remove expired monsters
            DateTime now = DateTime.Now;
            for (int i = state.Monsters.Count - 1; i >= 0; i--)
            {
                if (now >= state.Monsters[i].RemoveTime)
                {
                    state.Monsters.RemoveAt(i);
                }
            }

            state.MonsterActive = state.Monsters.Count > 0;
            if (!state.MonsterActive)
            {
                state.MonsterX = -1;
                state.MonsterY = -1;
                UpdateResourceLabels();
                return;
            }

            state.MonsterStepCounter++;
            int level = GameSession.CurrentLevel;
            // faster per level: L1 move every 2 ticks, L2+ every tick; L3+ take 2 steps, L5 takes 3
            int stepInterval = (level <= 1) ? 2 : 1;
            int monsterSteps = 1;
            if (level >= 3) monsterSteps = 2;
            if (level >= 5) monsterSteps = 3;

            if (state.MonsterStepCounter < stepInterval)
            {
                return;
            }
            state.MonsterStepCounter = 0;

            for (int i = 0; i < state.Monsters.Count; i++)
            {
                var controller = state.Monsters[i];
                for (int s = 0; s < monsterSteps; s++)
                {
                    var result = controller.Step(state.Maze, new Point(state.PlayerX, state.PlayerY));
                    if (result == MonsterStepResult.CaughtRunner)
                    {
                        HandleRunnerCaught();
                        return;
                    }
                }
            }

            if (state.Monsters.Count > 0)
            {
                state.MonsterX = state.Monsters[0].X;
                state.MonsterY = state.Monsters[0].Y;
                state.MonsterPrevX = state.Monsters[0].PrevX;
                state.MonsterPrevY = state.Monsters[0].PrevY;
            }
            UpdateResourceLabels();
            pbMaze.Invalidate();
        }

        private List<Point> GetMonsterCells()
        {
            List<Point> cells = new List<Point>();
            if (state.Monsters != null)
            {
                for (int i = 0; i < state.Monsters.Count; i++)
                {
                    if (state.Monsters[i].Active)
                    {
                        cells.Add(new Point(state.Monsters[i].X, state.Monsters[i].Y));
                    }
                }
            }
            return cells;
        }

        private void UpdateResourceLabels()
        {
            if (lbl_WallsLeft != null)
            {
                lbl_WallsLeft.Text = "Walls left: " + state.WallsLeft;
            }
            if (lbl_PushesLeft != null)
            {
                lbl_PushesLeft.Text = "Pushes left: " + state.WindCharges;
            }
            if (lbl_MonstersLeft != null)
            {
                int left = Math.Max(0, state.MonsterCharges);
                lbl_MonstersLeft.Text = "Monsters left: " + left;
            }
            if (lbl_Score != null)
            {
                lbl_Score.Text = "Coins: " + state.CurrentScore;
            }
        }

        private void UpdateLivesLabel()
        {
            if (lbl_lives_left == null)
            {
                return;
            }

            lbl_lives_left.Visible = true;
            lbl_lives_left.Text = "Lives: " + runnerLives;
        }

        private void ApplyMazeColour()
        {
            string choice = GameSession.CurrentMazeColour;
            if (string.IsNullOrWhiteSpace(choice))
            {
                choice = "Green";
            }

            string lower = choice.ToLower();
            if (lower == "black")
            {
                state.MazeBaseColor = Color.Black;
            }
            else if (lower == "blue")
            {
                state.MazeBaseColor = Color.DarkBlue;
            }
            else if (lower == "red")
            {
                state.MazeBaseColor = Color.DarkRed;
            }
            else if (lower == "orange")
            {
                state.MazeBaseColor = Color.DarkOrange;
            }
            else if (lower == "purple")
            {
                state.MazeBaseColor = Color.Purple;
            }
            else if (lower == "yellow")
            {
                state.MazeBaseColor = Color.Goldenrod;
            }
            else if (lower == "brown")
            {
                state.MazeBaseColor = Color.SaddleBrown;
            }
            else if (lower == "peach")
            {
                state.MazeBaseColor = Color.PeachPuff;
            }
            else if (lower == "grey" || lower == "gray")
            {
                state.MazeBaseColor = Color.DimGray;
            }
            else
            {
                state.MazeBaseColor = Color.DarkGreen;
            }
        }

        private void SetupModeLabel()
        {
            if (lblMode == null)
            {
                return;
            }

            string goalText = isBlocker ? "Goal: stop the runner" : "Goal: reach the exit";
            modeLabelBase = goalText;
            lblMode.Text = modeLabelBase;
        }

        private void UpdateStatusLabel(string status)
        {
            if (lblMode == null || string.IsNullOrWhiteSpace(modeLabelBase))
            {
                return;
            }
            lblMode.Text = modeLabelBase + " | Status: " + status;
        }

        private void lblMode_Click(object sender, EventArgs e)
        {

        }

        private void lblCountdown_Click(object sender, EventArgs e)
        {

        }

        private void lbl_MonstersLeft_Click(object sender, EventArgs e)
        {

        }

        private void lbl_PushesLeft_Click(object sender, EventArgs e)
        {

        }

        private void lbl_Score_Click(object sender, EventArgs e)
        {

        }
    }
}
