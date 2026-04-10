using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NEA_ai_pathfinding
{
    public partial class LabMazeForm : Form
    {
        // lab sandbox for generation + solving demos
        private Maze maze;
        private ChallengeMazeState state = new ChallengeMazeState();
        private MazeRenderer renderer;
        private System.Windows.Forms.Timer labTimer = new System.Windows.Forms.Timer();
        private ILabMazeGenerator gen;
        private ILabMazeSolver solver;
        private bool generating;
        private bool solving;
        private bool[,] exploredMask;
        private bool[,] pathMask;
        private bool manualEdit;
        private bool setStartMode;
        private bool setExitMode;
        private Point startCell = new Point(1, 1);
        private Point exitCell = new Point(1, 1);
        private Stopwatch genWatch = new Stopwatch();
        private Stopwatch solveWatch = new Stopwatch();
        private readonly int _rngSeed = Environment.TickCount;
        private readonly Random _rng;
        private Point agentPos = new Point(-1, -1);
        private bool agentRunning;
        private int tickCounter = 0;
        private bool turnBiasTestMode = false; // allow room/braid sliders to work in lab
        private LabQLearningAgent labAgent = new LabQLearningAgent();
        // note: challenge mode pre-train not used because mazes/tokens/monsters change too much

        public LabMazeForm()
        {
            InitializeComponent();
            _rng = new Random(_rngSeed);
            state.PlayerX = -1; // hide runner dot in lab
            state.PlayerY = -1;
            state.MazeBaseColor = Color.DarkGreen;
            renderer = new MazeRenderer(state);
            renderer.SetLabOverlays(null, null, false, true);
            labTimer.Interval = 80;
            labTimer.Tick += LabTimer_Tick;

        }

        private void LabMazeForm_Load(object sender, EventArgs e)
        {
            
            cmbGenAlgo.SelectedIndex = 0;

            
            cmbSolveAlgo.SelectedIndex = 0;

            lblStatus.Text = "Status: idle";
            // hide older episode controls to keep UI clean
            if (lblEpisodes != null) lblEpisodes.Visible = false;
            if (numEpisodes != null) numEpisodes.Visible = false;
            if (numLiveEpisodes != null)
            {
                numLiveEpisodes.Minimum = 1;
                numLiveEpisodes.Maximum = 5000;
                numLiveEpisodes.Value = 20;
                numLiveEpisodes.Visible = true;
            }
            if (lblLiveEpisodes != null)
            {
                lblLiveEpisodes.Text = "Live episodes:";
                lblLiveEpisodes.Visible = true;
            }
            if (btnStopAgent != null) btnStopAgent.Enabled = false;


        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            StopAll();
            solver = null;

            if (cmbGenAlgo != null && (cmbGenAlgo.SelectedItem == null || cmbGenAlgo.SelectedItem.ToString() == "Choose-"))
            {
                MessageBox.Show("Must Choose Generating Algorithm");
                return;
            }

            int w = MakeOdd((int)nudWidth.Value);
            int h = MakeOdd((int)nudHeight.Value);

            maze = new Maze(w, h);
            maze.Level = 1;
            maze.tokens.Clear(); // lab has no tokens
            state.Maze = maze;
            state.PlayerX = -1;
            state.PlayerY = -1;
            agentPos = new Point(-1, -1);

            startCell = new Point(1, 1);
            exitCell = new Point(maze.width - 2, maze.height - 2);

            gen = CreateGenerator(cmbGenAlgo.SelectedItem as string);
            LabGenSettings settings = new LabGenSettings
            {
                Width = w,
                Height = h,
                // UI is 0..100 so convert to 0..1 for generators
                BraidPerc = (double)nudBraidInput.Value / 100.0,
                RoomPerc = (double)nudRoomDensity.Value / 100.0,
                TurnBias = (int)nudTurnBias.Value,
                StepsPerTick = (int)nudStepsPerTick.Value
            };

            gen.Initialise(maze, _rng, settings);
            generating = true;
            solving = false;
            exploredMask = null;
            pathMask = null;
            renderer.SetLabOverlays(null, null, true, true);
            genWatch.Restart();
            labTimer.Interval = (int)nudDelay.Value;
            labTimer.Start();
            tickCounter = 0;
            lblStatus.Text = "Status: generating...";
            pbLabMaze.Invalidate();
        }

        private void btnSolve_Click(object sender, EventArgs e)
        {
            if (maze == null || generating)
            {
                MessageBox.Show("Maze must be generated first!", "Nothing to solve");
                return;
            }

            if (cmbSolveAlgo != null && (cmbSolveAlgo.SelectedItem == null || cmbSolveAlgo.SelectedItem.ToString() == "Choose-"))
            {
                MessageBox.Show("Must Choose Pathfinding Algorithm");
                return;
            }

            if (!maze.IsPath(startCell.X, startCell.Y))
            {
                // guard if start was on a wall
                maze.SetStart(new Point(1, 1));
                startCell = maze.start;
            }
            if (!maze.IsPath(exitCell.X, exitCell.Y))
            {
                // guard if exit was on a wall
                maze.SetExit(new Point(maze.width - 2, maze.height - 2));
                exitCell = maze.exit;
            }

            // Manual edits can break the maze, so block Solve if no path exists
            if (!maze.IsSolvable())
            {
                MessageBox.Show("This maze is not solvable (no path from Start to Exit). Please edit the maze to open a path.");
                return;
            }

            exploredMask = new bool[maze.width, maze.height];
            pathMask = new bool[maze.width, maze.height];

            solver = CreateSolver(cmbSolveAlgo.SelectedItem as string);
            solver.Initialise(maze, startCell, exitCell, exploredMask, pathMask);

            solving = true;
            genWatch.Reset();
            solveWatch.Restart();
            labTimer.Interval = (int)nudDelay.Value;
            labTimer.Start();
            lblStatus.Text = "Status: solving...";
            renderer.SetLabOverlays(exploredMask, pathMask, false, true);
        }

        private void btnClearSolve_Click(object sender, EventArgs e)
        {
            if (solving == true || generating || maze == null)
            {
                MessageBox.Show("Maze must be generated and solved before clearing!", "Nothing to clear");
                return;
            }
            solving = false;
            if (maze != null && exploredMask != null)
            {
                exploredMask = new bool[maze.width, maze.height];
                pathMask = new bool[maze.width, maze.height];
            }
            renderer.SetLabOverlays(exploredMask, pathMask, false, true);
            pbLabMaze.Invalidate();
            lblSolveTime.Text = "Solve time: --";
            lblExplored.Text = "Explored: --";
            lblPathLen.Text = "Path len: --";
            lblStatus.Text = "Status: cleared overlays";
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (maze != null)
            {
                StopAll();
                maze = null;
                state.Maze = null;
                renderer.SetLabOverlays(null, null, false, true);
                agentPos = new Point(-1, -1);
                lblGenTime.Text = "Gen time: --";
                lblSolveTime.Text = "Solve time: --";
                lblExplored.Text = "Explored: --";
                lblPathLen.Text = "Path len: --";
                lblStatus.Text = "Status: reset";
                pbLabMaze.Invalidate();
            }
            else
            {
                MessageBox.Show("Must generate a maze first by choosing an algorithm and clicking 'Generate'' ", "Nothing to reset");
            }

        }

        private void chkManualEdit_CheckedChanged(object sender, EventArgs e)
        {
            manualEdit = chkManualEdit.Checked;
            if (manualEdit)
            {
                StopAll();
                lblStatus.Text = "Status: manual edit";
            }
        }

        private void btnSetStart_Click(object sender, EventArgs e)
        {
            if (manualEdit)
            {
                setStartMode = true;
                setExitMode = false;
                lblStatus.Text = "Status: click a path cell for the start cell";
            }
            else
            {
                MessageBox.Show("Manual edit must be enabled to set start and exit, and edit the maze by clicking.", "Manual edit disabled");
            }
        }

        private void btnSetExit_Click(object sender, EventArgs e)
        {
            if (manualEdit)
            {
                setStartMode = false;
                setExitMode = true;
                lblStatus.Text = "Status: click a path cell for the exit cell";
            }
            else
            {
                MessageBox.Show("Manual edit must be enabled to set start and exit, and edit the maze by clicking.", "Manual edit disabled");

            }

        }

        private void pbLabMaze_Paint(object sender, PaintEventArgs e)
        {
            renderer.Draw(e, pbLabMaze);
        }

        private void pbLabMaze_MouseClick(object sender, MouseEventArgs e)
        {
            if (maze == null)
            {
                return;
            }

            int cellW = Math.Max(1, pbLabMaze.Width / maze.width);
            int cellH = Math.Max(1, pbLabMaze.Height / maze.height);
            int x = e.X / cellW;
            int y = e.Y / cellH;

            if (x < 0 || y < 0 || x >= maze.width || y >= maze.height)
            {
                return;
            }

            if (manualEdit)
            {
                // placing start/exit on a path
                if (setStartMode)
                {
                    if (maze.IsPath(x, y))
                    {
                        maze.SetStart(new Point(x, y));
                        startCell = new Point(x, y);
                        setStartMode = false;
                        ClearSolveOverlay();
                        lblStatus.Text = "Status: start moved";
                    }
                    pbLabMaze.Invalidate();
                    return;
                }

                if (setExitMode)
                {
                    if (maze.IsPath(x, y))
                    {
                        maze.SetExit(new Point(x, y));
                        exitCell = new Point(x, y);
                        setExitMode = false;
                        ClearSolveOverlay();
                        lblStatus.Text = "Status: exit moved";
                    }
                    pbLabMaze.Invalidate();
                    return;
                }

                if (x == startCell.X && y == startCell.Y) return;
                if (x == exitCell.X && y == exitCell.Y) return;

                if (x > 0 && y > 0 && x < maze.width - 1 && y < maze.height - 1)
                {
                    maze.grid[x, y] = maze.grid[x, y] == 0 ? 1 : 0;
                    ClearSolveOverlay();
                    pbLabMaze.Invalidate();
                    lblStatus.Text = "Status: edited cell";
                }
            }
        }

        private void LabTimer_Tick(object sender, EventArgs e)
        {
            labTimer.Interval = (int)nudDelay.Value;
            if (generating && gen != null)
            {
                tickCounter++;
                bool more = true;
                int steps = Math.Max(1, (int)nudStepsPerTick.Value);
                for (int i = 0; i < steps; i++)
                {
                    more = gen.Step();
                    if (!more)
                    {
                        break;
                    }
                }

                if (!more)
                {
                    // stop carving once no frontier remains
                    generating = false;
                    genWatch.Stop();
                    lblGenTime.Text = "Gen time: " + genWatch.ElapsedMilliseconds + " ms";
                    lblStatus.Text = "Status: generation done";
                    renderer.SetLabOverlays(exploredMask, pathMask, false, true);
                    maze.SetStart(startCell);
                    maze.SetExit(exitCell);
                    // Safety check: generated mazes must be playable
                    if (!maze.IsSolvable())
                    {
                        StopAll();
                        state.Maze = null;
                        renderer.SetLabOverlays(null, null, false, true);
                        pbLabMaze.Invalidate();
                        MessageBox.Show("Could not generate a solvable maze. Please try again.");
                        return;
                    }
                    pbLabMaze.Invalidate(); // final redraw so Rooms/Braid are visible
                }

                pbLabMaze.Invalidate();
                return;
            }

            if (solving && solver != null)
            {
                bool more = true;
                int steps = Math.Max(1, (int)nudStepsPerTick.Value);
                for (int i = 0; i < steps; i++)
                {
                    more = solver.Step();
                    if (!more)
                    {
                        break;
                    }
                }

                if (!more)
                {
                    // reached exit or no nodes left
                    solving = false;
                    solveWatch.Stop();
                    lblSolveTime.Text = "Solve time: " + solveWatch.ElapsedMilliseconds + " ms";
                    lblExplored.Text = "Explored: " + solver.VisitedCount;
                    lblPathLen.Text = solver.FoundPath ? "Path len: " + solver.PathLength : "Path len: none";
                    lblStatus.Text = solver.FoundPath ? "Status: path found" : "Status: no path";
                }

                pbLabMaze.Invalidate();
            }
            else
            {
                labTimer.Stop();
            }
        }

        private async void btnRunAgent_Click(object sender, EventArgs e)
        {
            if (maze == null || generating)
            {
                MessageBox.Show("Generate a maze first.", "Nowhere for agent to run");
                return;
            }
            if (agentRunning)
            {
                MessageBox.Show("Agent already running.", "Cannot run twice");
                return;
            }

            // Only run the agent if the user enabled it
            if (chkBox_AgentActive == null || !chkBox_AgentActive.Checked)
            {
                MessageBox.Show("Tick 'Agent Active' to enable the agent.", "Agent disabled");
                return;
            }

            double alpha = (double)num_LearningRate.Value;
            double gamma = (double)numDiscountFactor.Value;
            double epsilon = (double)numExplorationRate.Value;
            double decay = (double)numDecay.Value;
            int liveEpisodes = (int)numLiveEpisodes.Value;
            int maxSteps = (int)numMaxSteps.Value;
            int delayMs = (int)numStepDelay.Value;
            double bias = (double)numBias.Value;

            if (liveEpisodes <= 0 || maxSteps <= 0)
            {
                MessageBox.Show("Episodes and max steps must be positive.");
                return;
            }
            if (alpha == 0)
            {
                MessageBox.Show("Set learning rate above 0 to let the agent learn.", "Learning off");
                return;
            }

            agentRunning = true;
            btnRunAgent.Enabled = false;
            btnStopAgent.Enabled = true;
            lblAgentStatus.Text = "Agent running...";
            labAgent.StopRequested = false;
            await labAgent.RunEpisodesAsync(
                maze: maze,
                start: startCell,
                exit: exitCell,
                alpha: alpha,
                gamma: gamma,
                epsilon: epsilon,
                decay: decay,
                episodes: liveEpisodes,
                maxSteps: maxSteps,
                delayMs: delayMs,
                bias: bias,
                updateStatus: UpdateAgentStatus,
                redrawUi: RedrawAgentAsync);
            agentRunning = false;
            btnRunAgent.Enabled = true;
            btnStopAgent.Enabled = false;
        }

        private void btnStopAgent_Click(object sender, EventArgs e)
        {
            labAgent.StopRequested = true;
        }

        private ILabMazeGenerator CreateGenerator(string name)
        {
            // ComboBox items in Designer are human-friendly ("Prims", "Depth First Search"),
            // so normalise them here to avoid silently falling back to Prim.
            if (string.IsNullOrWhiteSpace(name))
            {
                return new LabPrimGenerator();
            }

            string trimmed = name.Trim();
            switch (trimmed)
            {
                case "Prims":
                    return new LabPrimGenerator();
                case "DFS":
                case "Depth First Search":
                    return new LabDfsGenerator();
                case "Kruskal":
                    return new LabKruskalGenerator();
                case "Division":
                    return new LabDivisionGenerator();
                case "Binary Tree":
                    return new LabBinaryTreeGenerator();
                default:
                    return new LabPrimGenerator();
            }
        }

        private ILabMazeSolver CreateSolver(string name)
        {
            switch (name)
            {
                case "DFS":
                    return new LabDfsSolver();
                case "Greedy":
                    return new LabGreedySolver();
                case "A*":
                    return new LabAStarSolver();
                default:
                    return new LabBfsSolver();
            }
        }

        private int MakeOdd(int v)
        {
            if (v % 2 == 0)
            {
                v += 1;
            }
            return v;
        }

        private void StopAll()
        {
            generating = false;
            solving = false;
            labTimer.Stop();
            genWatch.Reset();
            solveWatch.Reset();
            labAgent.StopRequested = true;
            agentRunning = false;
        }

        private void ClearSolveOverlay()
        {
            solving = false;
            if (maze != null)
            {
                exploredMask = new bool[maze.width, maze.height];
                pathMask = new bool[maze.width, maze.height];
            }
            renderer.SetLabOverlays(exploredMask, pathMask, false, true);
            pbLabMaze.Invalidate();
        }

        private void nudStepsPerTick_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudDelay_ValueChanged(object sender, EventArgs e)
        {
            labTimer.Interval = (int)nudDelay.Value;
        }

        private void nudWidth_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudTurnBias_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudRoomDensity_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void braid_ValueChanged(object sender, EventArgs e)
        {

        }

        private void UpdateAgentStatus(Point pos, string text)
        {
            // keep agent marker and label in sync while training (text built in LabQLearningAgent)
            agentPos = pos;
            state.PlayerX = pos.X;
            state.PlayerY = pos.Y;
            lblAgentStatus.Text = text;
        }

        private Task RedrawAgentAsync()
        {
            pbLabMaze.Invalidate();
            return Task.CompletedTask;
        }

        private void chkBox_AgentActive_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cmbSolveAlgo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lblSolver_Click(object sender, EventArgs e)
        {

        }

        private void lbl_GenerationAlg_Click(object sender, EventArgs e)
        {

        }



        private void lblSize_Click(object sender, EventArgs e)
        {

        }

        private void btn_backHome_Click(object sender, EventArgs e)
        {
            // simple back button from designer wiring
            this.Close();
        }

        // no special owner handling needed; closing this form is enough

        private void cmbGenAlgo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Btn_ExplainingParams(object sender, EventArgs e)
        {

            string msg =
        "Quick guide\n\n" +

        "Generation:\n" +

        
        "• DFS (Backtracker): long winding corridors, lots of dead ends\n" +
        "• Prim: bushier maze, lots of short branches, more evenly spread\n" +
        "• Binary Tree: very structured, diagonal bias, predictable patterns\n" +
        "• Kruskal: more uniform networks, lots of small loops/variety depending on shuffle\n" +
        "• Division: rectangular “rooms” and straight walls, very blocky look\n\n" +
        "• Turn bias: higher = less straighter corridors, more turns\n" +
        "• Width/Height: maze size (bigger = harder + slower)\n" +
        "• Braid: adds loops (less dead-ends)\n" +
        "• Room density: more open areas\n\n" +



        "Solving:\n" +
        "• Pathfinding algorithm: how Solve finds a route\n" +
        "• Animation delay: slows the step-by-step playback\n\n" +

        "RL agent (Q-learning):\n" +
        "• Active?: turns the agent on\n" +
        "• Learning rate: how fast it updates/learns \n" +
        "• Discount: how much it cares about the future\n" +
        "• Exploration: how random it is\n" +
        "• Episodes: how many training runs\n" +
        "• Max steps: stops it getting stuck/ how many moves it can make per episode\n" +
        "• Step delay: pause (ms) between each agent move on screen\n" +
        "• Decay: shrinks exploration each episode (lower decay = keeps exploring longer)";

            MessageBox.Show(msg, "Explaining Parameters", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lblAgentStatus_Click(object sender, EventArgs e)
        {
            // label only shows live agent text
        }

        private void nudHeight_ValueChanged(object sender, EventArgs e)
        {

        }

        private void pbLabMaze_Click(object sender, EventArgs e)
        {

        }

        private void lbl_LearningRate_Click(object sender, EventArgs e)
        {

        }
    }
}
