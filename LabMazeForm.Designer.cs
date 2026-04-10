namespace NEA_ai_pathfinding
{
    partial class LabMazeForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pbLabMaze = new PictureBox();
            cmbGenAlgo = new ComboBox();
            cmbSolveAlgo = new ComboBox();
            nudWidth = new NumericUpDown();
            nudHeight = new NumericUpDown();
            nudDelay = new NumericUpDown();
            nudRoomDensity = new NumericUpDown();
            nudTurnBias = new NumericUpDown();
            nudStepsPerTick = new NumericUpDown();
            btnGenerate = new Button();
            btnSolve = new Button();
            btnClearSolve = new Button();
            btnReset = new Button();
            chkManualEdit = new CheckBox();
            btnSetStart = new Button();
            btnSetExit = new Button();
            lblGenTime = new Label();
            lblSolveTime = new Label();
            lblExplored = new Label();
            lblPathLen = new Label();
            lblStatus = new Label();
            lblDelay = new Label();
            lblSize = new Label();
            lblGenParams = new Label();
            lblSolver = new Label();
            lbl_GenerationAlg = new Label();
            lbl_nudturnbias = new Label();
            lbl_nudstepspertick = new Label();
            nudBraidInput = new NumericUpDown();
            lbl_nudbraid = new Label();
            lbl_nudRoomDensity = new Label();
            lbl_nudWidth = new Label();
            lbl_nudHeight = new Label();
            lbl_AgentParameters = new Label();
            chkBox_AgentActive = new CheckBox();
            lbl_LearningRate = new Label();
            num_LearningRate = new NumericUpDown();
            lbl_DiscountFactor = new Label();
            numDiscountFactor = new NumericUpDown();
            lbl_ExplorationRate = new Label();
            numExplorationRate = new NumericUpDown();
            lblDecay = new Label();
            numDecay = new NumericUpDown();
            lblEpisodes = new Label();
            lblLiveEpisodes = new Label();
            numLiveEpisodes = new NumericUpDown();
            numEpisodes = new NumericUpDown();
            lblMaxSteps = new Label();
            numMaxSteps = new NumericUpDown();
            lblStepDelay = new Label();
            numStepDelay = new NumericUpDown();
            lblAgentStatus = new Label();
            btnRunAgent = new Button();
            btnStopAgent = new Button();
            lblBias = new Label();
            numBias = new NumericUpDown();
            btn_backHome = new Button();
            btn_ExplainingParams = new Button();
            ((System.ComponentModel.ISupportInitialize)pbLabMaze).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRoomDensity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTurnBias).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStepsPerTick).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBraidInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)num_LearningRate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDiscountFactor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numExplorationRate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDecay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numLiveEpisodes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numEpisodes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaxSteps).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numStepDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numBias).BeginInit();
            SuspendLayout();
            // 
            // pbLabMaze
            // 
            pbLabMaze.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pbLabMaze.BackColor = Color.Black;
            pbLabMaze.BorderStyle = BorderStyle.FixedSingle;
            pbLabMaze.Location = new Point(1, 9);
            pbLabMaze.Name = "pbLabMaze";
            pbLabMaze.Size = new Size(756, 691);
            pbLabMaze.TabIndex = 0;
            pbLabMaze.TabStop = false;
            pbLabMaze.Click += pbLabMaze_Click;
            pbLabMaze.Paint += pbLabMaze_Paint;
            pbLabMaze.MouseClick += pbLabMaze_MouseClick;
            // 
            // cmbGenAlgo
            // 
            cmbGenAlgo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cmbGenAlgo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGenAlgo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            cmbGenAlgo.FormattingEnabled = true;
            cmbGenAlgo.ImeMode = ImeMode.NoControl;
            cmbGenAlgo.Items.AddRange(new object[] { "Choose-", "Prims", "Depth First Search", "Kruskal", "Division", "Binary Tree" });
            cmbGenAlgo.Location = new Point(847, 47);
            cmbGenAlgo.Name = "cmbGenAlgo";
            cmbGenAlgo.Size = new Size(202, 31);
            cmbGenAlgo.TabIndex = 1;
            cmbGenAlgo.SelectedIndexChanged += cmbGenAlgo_SelectedIndexChanged;
            // 
            // cmbSolveAlgo
            // 
            cmbSolveAlgo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cmbSolveAlgo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSolveAlgo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            cmbSolveAlgo.FormattingEnabled = true;
            cmbSolveAlgo.Items.AddRange(new object[] { "Choose-", "BFS", "DFS", "Greedy", "A*" });
            cmbSolveAlgo.Location = new Point(838, 421);
            cmbSolveAlgo.Name = "cmbSolveAlgo";
            cmbSolveAlgo.Size = new Size(202, 31);
            cmbSolveAlgo.TabIndex = 8;
            cmbSolveAlgo.SelectedIndexChanged += cmbSolveAlgo_SelectedIndexChanged;
            // 
            // nudWidth
            // 
            nudWidth.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            nudWidth.Increment = new decimal(new int[] { 2, 0, 0, 0 });
            nudWidth.Location = new Point(878, 110);
            nudWidth.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            nudWidth.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            nudWidth.Name = "nudWidth";
            nudWidth.Size = new Size(79, 27);
            nudWidth.TabIndex = 2;
            nudWidth.Value = new decimal(new int[] { 25, 0, 0, 0 });
            nudWidth.ValueChanged += nudWidth_ValueChanged;
            // 
            // nudHeight
            // 
            nudHeight.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            nudHeight.Increment = new decimal(new int[] { 2, 0, 0, 0 });
            nudHeight.Location = new Point(1061, 110);
            nudHeight.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            nudHeight.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            nudHeight.Name = "nudHeight";
            nudHeight.Size = new Size(79, 27);
            nudHeight.TabIndex = 3;
            nudHeight.Value = new decimal(new int[] { 25, 0, 0, 0 });
            nudHeight.ValueChanged += nudHeight_ValueChanged;
            // 
            // nudDelay
            // 
            nudDelay.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            nudDelay.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            nudDelay.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            nudDelay.Location = new Point(1159, 570);
            nudDelay.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            nudDelay.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            nudDelay.Name = "nudDelay";
            nudDelay.Size = new Size(101, 32);
            nudDelay.TabIndex = 12;
            nudDelay.Value = new decimal(new int[] { 100, 0, 0, 0 });
            nudDelay.ValueChanged += nudDelay_ValueChanged;
            // 
            // nudRoomDensity
            // 
            nudRoomDensity.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            nudRoomDensity.Location = new Point(1103, 178);
            nudRoomDensity.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            nudRoomDensity.Name = "nudRoomDensity";
            nudRoomDensity.Size = new Size(79, 27);
            nudRoomDensity.TabIndex = 5;
            nudRoomDensity.Value = new decimal(new int[] { 8, 0, 0, 0 });
            nudRoomDensity.ValueChanged += nudRoomDensity_ValueChanged;
            // 
            // nudTurnBias
            // 
            nudTurnBias.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            nudTurnBias.Location = new Point(886, 216);
            nudTurnBias.Name = "nudTurnBias";
            nudTurnBias.Size = new Size(79, 27);
            nudTurnBias.TabIndex = 6;
            nudTurnBias.Value = new decimal(new int[] { 80, 0, 0, 0 });
            nudTurnBias.ValueChanged += nudTurnBias_ValueChanged;
            // 
            // nudStepsPerTick
            // 
            nudStepsPerTick.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            nudStepsPerTick.Location = new Point(1103, 216);
            nudStepsPerTick.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            nudStepsPerTick.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudStepsPerTick.Name = "nudStepsPerTick";
            nudStepsPerTick.Size = new Size(79, 27);
            nudStepsPerTick.TabIndex = 7;
            nudStepsPerTick.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudStepsPerTick.ValueChanged += nudStepsPerTick_ValueChanged;
            // 
            // btnGenerate
            // 
            btnGenerate.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnGenerate.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnGenerate.ForeColor = Color.LimeGreen;
            btnGenerate.Location = new Point(838, 265);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(127, 45);
            btnGenerate.TabIndex = 9;
            btnGenerate.Text = "Generate";
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // btnSolve
            // 
            btnSolve.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnSolve.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnSolve.ForeColor = Color.LimeGreen;
            btnSolve.Location = new Point(802, 469);
            btnSolve.Name = "btnSolve";
            btnSolve.Size = new Size(119, 41);
            btnSolve.TabIndex = 10;
            btnSolve.Text = "Solve";
            btnSolve.UseVisualStyleBackColor = true;
            btnSolve.Click += btnSolve_Click;
            // 
            // btnClearSolve
            // 
            btnClearSolve.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnClearSolve.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnClearSolve.ForeColor = Color.Red;
            btnClearSolve.Location = new Point(971, 469);
            btnClearSolve.Name = "btnClearSolve";
            btnClearSolve.Size = new Size(129, 41);
            btnClearSolve.TabIndex = 11;
            btnClearSolve.Text = "Clear solve";
            btnClearSolve.UseVisualStyleBackColor = true;
            btnClearSolve.Click += btnClearSolve_Click;
            // 
            // btnReset
            // 
            btnReset.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnReset.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnReset.ForeColor = Color.Red;
            btnReset.Location = new Point(997, 265);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(134, 45);
            btnReset.TabIndex = 13;
            btnReset.Text = "Reset";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += btnReset_Click;
            // 
            // chkManualEdit
            // 
            chkManualEdit.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            chkManualEdit.AutoSize = true;
            chkManualEdit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            chkManualEdit.ForeColor = Color.RoyalBlue;
            chkManualEdit.Location = new Point(793, 335);
            chkManualEdit.Name = "chkManualEdit";
            chkManualEdit.Size = new Size(128, 27);
            chkManualEdit.TabIndex = 14;
            chkManualEdit.Text = "Manual edit";
            chkManualEdit.UseVisualStyleBackColor = true;
            chkManualEdit.CheckedChanged += chkManualEdit_CheckedChanged;
            // 
            // btnSetStart
            // 
            btnSetStart.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnSetStart.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnSetStart.Location = new Point(971, 328);
            btnSetStart.Name = "btnSetStart";
            btnSetStart.Size = new Size(105, 36);
            btnSetStart.TabIndex = 15;
            btnSetStart.Text = "Set start";
            btnSetStart.UseVisualStyleBackColor = true;
            btnSetStart.Click += btnSetStart_Click;
            // 
            // btnSetExit
            // 
            btnSetExit.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnSetExit.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnSetExit.Location = new Point(1082, 328);
            btnSetExit.Name = "btnSetExit";
            btnSetExit.Size = new Size(100, 36);
            btnSetExit.TabIndex = 16;
            btnSetExit.Text = "Set exit";
            btnSetExit.UseVisualStyleBackColor = true;
            btnSetExit.Click += btnSetExit_Click;
            // 
            // lblGenTime
            // 
            lblGenTime.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblGenTime.AutoSize = true;
            lblGenTime.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblGenTime.Location = new Point(783, 526);
            lblGenTime.Name = "lblGenTime";
            lblGenTime.Size = new Size(164, 23);
            lblGenTime.TabIndex = 17;
            lblGenTime.Text = "Generation time: --";
            // 
            // lblSolveTime
            // 
            lblSolveTime.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblSolveTime.AutoSize = true;
            lblSolveTime.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSolveTime.Location = new Point(783, 557);
            lblSolveTime.Name = "lblSolveTime";
            lblSolveTime.Size = new Size(119, 23);
            lblSolveTime.TabIndex = 18;
            lblSolveTime.Text = "Solve time: --";
            // 
            // lblExplored
            // 
            lblExplored.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblExplored.AutoSize = true;
            lblExplored.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblExplored.Location = new Point(783, 587);
            lblExplored.Name = "lblExplored";
            lblExplored.Size = new Size(105, 23);
            lblExplored.TabIndex = 19;
            lblExplored.Text = "Explored: --";
            // 
            // lblPathLen
            // 
            lblPathLen.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblPathLen.AutoSize = true;
            lblPathLen.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPathLen.Location = new Point(783, 618);
            lblPathLen.Name = "lblPathLen";
            lblPathLen.Size = new Size(99, 23);
            lblPathLen.TabIndex = 20;
            lblPathLen.Text = "Path len: --";
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblStatus.Location = new Point(783, 650);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(77, 23);
            lblStatus.TabIndex = 21;
            lblStatus.Text = "Status: -";
            // 
            // lblDelay
            // 
            lblDelay.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblDelay.AutoSize = true;
            lblDelay.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblDelay.Location = new Point(989, 572);
            lblDelay.Name = "lblDelay";
            lblDelay.Size = new Size(164, 25);
            lblDelay.TabIndex = 22;
            lblDelay.Text = "Animation Delay:";
            // 
            // lblSize
            // 
            lblSize.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblSize.AutoSize = true;
            lblSize.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSize.ForeColor = Color.Red;
            lblSize.Location = new Point(771, 86);
            lblSize.Name = "lblSize";
            lblSize.Size = new Size(52, 25);
            lblSize.TabIndex = 23;
            lblSize.Text = "Size:";
            lblSize.Click += lblSize_Click;
            // 
            // lblGenParams
            // 
            lblGenParams.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblGenParams.AutoSize = true;
            lblGenParams.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblGenParams.ForeColor = Color.Red;
            lblGenParams.Location = new Point(771, 150);
            lblGenParams.Name = "lblGenParams";
            lblGenParams.Size = new Size(117, 25);
            lblGenParams.TabIndex = 24;
            lblGenParams.Text = "Parameters:";
            // 
            // lblSolver
            // 
            lblSolver.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblSolver.AutoSize = true;
            lblSolver.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblSolver.ForeColor = SystemColors.MenuHighlight;
            lblSolver.Location = new Point(783, 376);
            lblSolver.Name = "lblSolver";
            lblSolver.Size = new Size(277, 32);
            lblSolver.TabIndex = 25;
            lblSolver.Text = "Pathfinding Algorithm:";
            lblSolver.Click += lblSolver_Click;
            // 
            // lbl_GenerationAlg
            // 
            lbl_GenerationAlg.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl_GenerationAlg.AutoSize = true;
            lbl_GenerationAlg.Cursor = Cursors.PanNW;
            lbl_GenerationAlg.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lbl_GenerationAlg.ForeColor = SystemColors.MenuHighlight;
            lbl_GenerationAlg.Location = new Point(789, 9);
            lbl_GenerationAlg.Name = "lbl_GenerationAlg";
            lbl_GenerationAlg.Size = new Size(271, 32);
            lbl_GenerationAlg.TabIndex = 27;
            lbl_GenerationAlg.Text = "Generation Algorithm:";
            lbl_GenerationAlg.Click += lbl_GenerationAlg_Click;
            // 
            // lbl_nudturnbias
            // 
            lbl_nudturnbias.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl_nudturnbias.AutoSize = true;
            lbl_nudturnbias.Location = new Point(802, 218);
            lbl_nudturnbias.Name = "lbl_nudturnbias";
            lbl_nudturnbias.Size = new Size(78, 20);
            lbl_nudturnbias.TabIndex = 28;
            lbl_nudturnbias.Text = "Turn Bias:";
            lbl_nudturnbias.Click += label2_Click;
            // 
            // lbl_nudstepspertick
            // 
            lbl_nudstepspertick.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl_nudstepspertick.AutoSize = true;
            lbl_nudstepspertick.Location = new Point(986, 220);
            lbl_nudstepspertick.Name = "lbl_nudstepspertick";
            lbl_nudstepspertick.Size = new Size(111, 20);
            lbl_nudstepspertick.TabIndex = 29;
            lbl_nudstepspertick.Text = "Steps per Tick:";
            // 
            // nudBraidInput
            // 
            nudBraidInput.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            nudBraidInput.Location = new Point(885, 178);
            nudBraidInput.Name = "nudBraidInput";
            nudBraidInput.Size = new Size(80, 27);
            nudBraidInput.TabIndex = 30;
            nudBraidInput.ValueChanged += braid_ValueChanged;
            // 
            // lbl_nudbraid
            // 
            lbl_nudbraid.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl_nudbraid.AutoSize = true;
            lbl_nudbraid.Location = new Point(813, 180);
            lbl_nudbraid.Name = "lbl_nudbraid";
            lbl_nudbraid.Size = new Size(50, 20);
            lbl_nudbraid.TabIndex = 31;
            lbl_nudbraid.Text = "Braid:";
            // 
            // lbl_nudRoomDensity
            // 
            lbl_nudRoomDensity.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl_nudRoomDensity.AutoSize = true;
            lbl_nudRoomDensity.Location = new Point(980, 180);
            lbl_nudRoomDensity.Name = "lbl_nudRoomDensity";
            lbl_nudRoomDensity.Size = new Size(112, 20);
            lbl_nudRoomDensity.TabIndex = 32;
            lbl_nudRoomDensity.Text = "Room Density:";
            // 
            // lbl_nudWidth
            // 
            lbl_nudWidth.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl_nudWidth.AutoSize = true;
            lbl_nudWidth.Location = new Point(813, 112);
            lbl_nudWidth.Name = "lbl_nudWidth";
            lbl_nudWidth.Size = new Size(56, 20);
            lbl_nudWidth.TabIndex = 33;
            lbl_nudWidth.Text = "Width:";
            // 
            // lbl_nudHeight
            // 
            lbl_nudHeight.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl_nudHeight.AutoSize = true;
            lbl_nudHeight.Location = new Point(989, 112);
            lbl_nudHeight.Name = "lbl_nudHeight";
            lbl_nudHeight.Size = new Size(60, 20);
            lbl_nudHeight.TabIndex = 34;
            lbl_nudHeight.Text = "Height:";
            // 
            // lbl_AgentParameters
            // 
            lbl_AgentParameters.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl_AgentParameters.AutoSize = true;
            lbl_AgentParameters.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lbl_AgentParameters.ForeColor = SystemColors.MenuHighlight;
            lbl_AgentParameters.Location = new Point(1138, 9);
            lbl_AgentParameters.Name = "lbl_AgentParameters";
            lbl_AgentParameters.Size = new Size(507, 32);
            lbl_AgentParameters.TabIndex = 35;
            lbl_AgentParameters.Text = "Reinforcement Learning Agent Parameters:";
            // 
            // chkBox_AgentActive
            // 
            chkBox_AgentActive.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            chkBox_AgentActive.AutoSize = true;
            chkBox_AgentActive.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            chkBox_AgentActive.ForeColor = Color.CornflowerBlue;
            chkBox_AgentActive.Location = new Point(1352, 44);
            chkBox_AgentActive.Name = "chkBox_AgentActive";
            chkBox_AgentActive.Size = new Size(103, 32);
            chkBox_AgentActive.TabIndex = 36;
            chkBox_AgentActive.Text = "Active?";
            chkBox_AgentActive.UseVisualStyleBackColor = true;
            chkBox_AgentActive.CheckedChanged += chkBox_AgentActive_CheckedChanged;
            // 
            // lbl_LearningRate
            // 
            lbl_LearningRate.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl_LearningRate.AutoSize = true;
            lbl_LearningRate.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lbl_LearningRate.Location = new Point(1276, 85);
            lbl_LearningRate.Name = "lbl_LearningRate";
            lbl_LearningRate.Size = new Size(148, 28);
            lbl_LearningRate.TabIndex = 37;
            lbl_LearningRate.Text = "Learning Rate:";
            lbl_LearningRate.Click += lbl_LearningRate_Click;
            // 
            // num_LearningRate
            // 
            num_LearningRate.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            num_LearningRate.DecimalPlaces = 2;
            num_LearningRate.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            num_LearningRate.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            num_LearningRate.Location = new Point(1430, 85);
            num_LearningRate.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            num_LearningRate.Name = "num_LearningRate";
            num_LearningRate.Size = new Size(80, 34);
            num_LearningRate.TabIndex = 38;
            // 
            // lbl_DiscountFactor
            // 
            lbl_DiscountFactor.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl_DiscountFactor.AutoSize = true;
            lbl_DiscountFactor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lbl_DiscountFactor.Location = new Point(1289, 127);
            lbl_DiscountFactor.Name = "lbl_DiscountFactor";
            lbl_DiscountFactor.Size = new Size(166, 28);
            lbl_DiscountFactor.TabIndex = 39;
            lbl_DiscountFactor.Text = "Discount Factor:";
            // 
            // numDiscountFactor
            // 
            numDiscountFactor.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            numDiscountFactor.DecimalPlaces = 2;
            numDiscountFactor.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            numDiscountFactor.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numDiscountFactor.Location = new Point(1461, 125);
            numDiscountFactor.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numDiscountFactor.Name = "numDiscountFactor";
            numDiscountFactor.Size = new Size(80, 34);
            numDiscountFactor.TabIndex = 40;
            // 
            // lbl_ExplorationRate
            // 
            lbl_ExplorationRate.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl_ExplorationRate.AutoSize = true;
            lbl_ExplorationRate.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lbl_ExplorationRate.Location = new Point(1289, 167);
            lbl_ExplorationRate.Name = "lbl_ExplorationRate";
            lbl_ExplorationRate.Size = new Size(175, 28);
            lbl_ExplorationRate.TabIndex = 41;
            lbl_ExplorationRate.Text = "Exploration Rate:";
            // 
            // numExplorationRate
            // 
            numExplorationRate.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            numExplorationRate.DecimalPlaces = 2;
            numExplorationRate.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            numExplorationRate.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numExplorationRate.Location = new Point(1470, 165);
            numExplorationRate.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numExplorationRate.Name = "numExplorationRate";
            numExplorationRate.Size = new Size(80, 34);
            numExplorationRate.TabIndex = 42;
            // 
            // lblDecay
            // 
            lblDecay.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblDecay.AutoSize = true;
            lblDecay.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblDecay.Location = new Point(1290, 213);
            lblDecay.Name = "lblDecay";
            lblDecay.Size = new Size(190, 28);
            lblDecay.TabIndex = 43;
            lblDecay.Text = "Exploration Decay:";
            // 
            // numDecay
            // 
            numDecay.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            numDecay.DecimalPlaces = 3;
            numDecay.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            numDecay.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
            numDecay.Location = new Point(1481, 211);
            numDecay.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numDecay.Name = "numDecay";
            numDecay.Size = new Size(80, 34);
            numDecay.TabIndex = 44;
            // 
            // lblEpisodes
            // 
            lblEpisodes.AutoSize = true;
            lblEpisodes.Location = new Point(1249, 191);
            lblEpisodes.Name = "lblEpisodes";
            lblEpisodes.Size = new Size(72, 20);
            lblEpisodes.TabIndex = 45;
            lblEpisodes.Text = "Episodes:";
            // 
            // lblLiveEpisodes
            // 
            lblLiveEpisodes.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblLiveEpisodes.AutoSize = true;
            lblLiveEpisodes.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblLiveEpisodes.Location = new Point(1290, 254);
            lblLiveEpisodes.Name = "lblLiveEpisodes";
            lblLiveEpisodes.Size = new Size(143, 28);
            lblLiveEpisodes.TabIndex = 45;
            lblLiveEpisodes.Text = "Live episodes:";
            // 
            // numLiveEpisodes
            // 
            numLiveEpisodes.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            numLiveEpisodes.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            numLiveEpisodes.Location = new Point(1439, 254);
            numLiveEpisodes.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numLiveEpisodes.Name = "numLiveEpisodes";
            numLiveEpisodes.Size = new Size(71, 34);
            numLiveEpisodes.TabIndex = 56;
            // 
            // numEpisodes
            // 
            numEpisodes.Location = new Point(1330, 189);
            numEpisodes.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            numEpisodes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numEpisodes.Name = "numEpisodes";
            numEpisodes.Size = new Size(80, 27);
            numEpisodes.TabIndex = 46;
            numEpisodes.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // lblMaxSteps
            // 
            lblMaxSteps.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblMaxSteps.AutoSize = true;
            lblMaxSteps.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblMaxSteps.Location = new Point(1290, 300);
            lblMaxSteps.Name = "lblMaxSteps";
            lblMaxSteps.Size = new Size(113, 28);
            lblMaxSteps.TabIndex = 47;
            lblMaxSteps.Text = "Max steps:";
            // 
            // numMaxSteps
            // 
            numMaxSteps.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            numMaxSteps.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            numMaxSteps.Location = new Point(1409, 300);
            numMaxSteps.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numMaxSteps.Name = "numMaxSteps";
            numMaxSteps.Size = new Size(80, 34);
            numMaxSteps.TabIndex = 48;
            // 
            // lblStepDelay
            // 
            lblStepDelay.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblStepDelay.AutoSize = true;
            lblStepDelay.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblStepDelay.Location = new Point(1290, 386);
            lblStepDelay.Name = "lblStepDelay";
            lblStepDelay.Size = new Size(149, 28);
            lblStepDelay.TabIndex = 49;
            lblStepDelay.Text = "Step delay ms:";
            // 
            // numStepDelay
            // 
            numStepDelay.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            numStepDelay.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            numStepDelay.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numStepDelay.Location = new Point(1445, 384);
            numStepDelay.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numStepDelay.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numStepDelay.Name = "numStepDelay";
            numStepDelay.Size = new Size(80, 34);
            numStepDelay.TabIndex = 50;
            numStepDelay.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // lblAgentStatus
            // 
            lblAgentStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblAgentStatus.AutoSize = true;
            lblAgentStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblAgentStatus.Location = new Point(1352, 495);
            lblAgentStatus.MaximumSize = new Size(200, 0);
            lblAgentStatus.Name = "lblAgentStatus";
            lblAgentStatus.Size = new Size(156, 28);
            lblAgentStatus.TabIndex = 51;
            lblAgentStatus.Text = "Agent idle now";
            lblAgentStatus.Click += lblAgentStatus_Click;
            // 
            // btnRunAgent
            // 
            btnRunAgent.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnRunAgent.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnRunAgent.ForeColor = Color.LimeGreen;
            btnRunAgent.Location = new Point(1323, 433);
            btnRunAgent.Name = "btnRunAgent";
            btnRunAgent.Size = new Size(84, 39);
            btnRunAgent.TabIndex = 52;
            btnRunAgent.Text = "Run";
            btnRunAgent.UseVisualStyleBackColor = true;
            btnRunAgent.Click += btnRunAgent_Click;
            // 
            // btnStopAgent
            // 
            btnStopAgent.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnStopAgent.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnStopAgent.ForeColor = Color.Red;
            btnStopAgent.Location = new Point(1429, 433);
            btnStopAgent.Name = "btnStopAgent";
            btnStopAgent.Size = new Size(84, 39);
            btnStopAgent.TabIndex = 53;
            btnStopAgent.Text = "Stop";
            btnStopAgent.UseVisualStyleBackColor = true;
            btnStopAgent.Click += btnStopAgent_Click;
            // 
            // lblBias
            // 
            lblBias.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblBias.AutoSize = true;
            lblBias.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblBias.Location = new Point(1290, 344);
            lblBias.Name = "lblBias";
            lblBias.Size = new Size(56, 28);
            lblBias.TabIndex = 54;
            lblBias.Text = "Bias:";
            // 
            // numBias
            // 
            numBias.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            numBias.DecimalPlaces = 2;
            numBias.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            numBias.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numBias.Location = new Point(1352, 342);
            numBias.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numBias.Name = "numBias";
            numBias.Size = new Size(80, 34);
            numBias.TabIndex = 55;
            // 
            // btn_backHome
            // 
            btn_backHome.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btn_backHome.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btn_backHome.Location = new Point(1509, 663);
            btn_backHome.Name = "btn_backHome";
            btn_backHome.Size = new Size(119, 34);
            btn_backHome.TabIndex = 57;
            btn_backHome.Text = "Back";
            btn_backHome.UseVisualStyleBackColor = true;
            btn_backHome.Click += btn_backHome_Click;
            // 
            // btn_ExplainingParams
            // 
            btn_ExplainingParams.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btn_ExplainingParams.BackColor = Color.White;
            btn_ExplainingParams.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_ExplainingParams.ForeColor = Color.FromArgb(128, 128, 255);
            btn_ExplainingParams.Location = new Point(1114, 640);
            btn_ExplainingParams.Name = "btn_ExplainingParams";
            btn_ExplainingParams.Size = new Size(249, 60);
            btn_ExplainingParams.TabIndex = 58;
            btn_ExplainingParams.Text = "What does this all mean?";
            btn_ExplainingParams.UseVisualStyleBackColor = false;
            btn_ExplainingParams.Click += Btn_ExplainingParams;
            // 
            // LabMazeForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(1650, 709);
            Controls.Add(btn_ExplainingParams);
            Controls.Add(btn_backHome);
            Controls.Add(numBias);
            Controls.Add(lblBias);
            Controls.Add(btnStopAgent);
            Controls.Add(btnRunAgent);
            Controls.Add(lblAgentStatus);
            Controls.Add(numStepDelay);
            Controls.Add(lblStepDelay);
            Controls.Add(numMaxSteps);
            Controls.Add(lblMaxSteps);
            Controls.Add(numLiveEpisodes);
            Controls.Add(lblLiveEpisodes);
            Controls.Add(numDecay);
            Controls.Add(lblDecay);
            Controls.Add(numExplorationRate);
            Controls.Add(lbl_ExplorationRate);
            Controls.Add(numDiscountFactor);
            Controls.Add(lbl_DiscountFactor);
            Controls.Add(num_LearningRate);
            Controls.Add(lbl_LearningRate);
            Controls.Add(chkBox_AgentActive);
            Controls.Add(lbl_AgentParameters);
            Controls.Add(lbl_nudHeight);
            Controls.Add(lbl_nudWidth);
            Controls.Add(lbl_nudRoomDensity);
            Controls.Add(lbl_nudbraid);
            Controls.Add(nudBraidInput);
            Controls.Add(lbl_nudstepspertick);
            Controls.Add(lbl_nudturnbias);
            Controls.Add(lbl_GenerationAlg);
            Controls.Add(lblSolver);
            Controls.Add(lblGenParams);
            Controls.Add(lblSize);
            Controls.Add(lblDelay);
            Controls.Add(lblStatus);
            Controls.Add(lblPathLen);
            Controls.Add(lblExplored);
            Controls.Add(lblSolveTime);
            Controls.Add(lblGenTime);
            Controls.Add(btnSetExit);
            Controls.Add(btnSetStart);
            Controls.Add(chkManualEdit);
            Controls.Add(btnReset);
            Controls.Add(btnClearSolve);
            Controls.Add(btnSolve);
            Controls.Add(btnGenerate);
            Controls.Add(nudStepsPerTick);
            Controls.Add(nudTurnBias);
            Controls.Add(nudRoomDensity);
            Controls.Add(nudDelay);
            Controls.Add(nudHeight);
            Controls.Add(nudWidth);
            Controls.Add(cmbSolveAlgo);
            Controls.Add(cmbGenAlgo);
            Controls.Add(pbLabMaze);
            Cursor = Cursors.PanNW;
            Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Name = "LabMazeForm";
            Text = "Lab Maze Playground";
            Load += LabMazeForm_Load;
            ((System.ComponentModel.ISupportInitialize)pbLabMaze).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRoomDensity).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTurnBias).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStepsPerTick).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBraidInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)num_LearningRate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDiscountFactor).EndInit();
            ((System.ComponentModel.ISupportInitialize)numExplorationRate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDecay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLiveEpisodes).EndInit();
            ((System.ComponentModel.ISupportInitialize)numEpisodes).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaxSteps).EndInit();
            ((System.ComponentModel.ISupportInitialize)numStepDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numBias).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pbLabMaze;
        private ComboBox cmbGenAlgo;
        private ComboBox cmbSolveAlgo;
        private NumericUpDown nudWidth;
        private NumericUpDown nudHeight;
        private NumericUpDown nudDelay;
        private NumericUpDown nudRoomDensity;
        private NumericUpDown nudTurnBias;
        private NumericUpDown nudStepsPerTick;
        private Button btnGenerate;
        private Button btnSolve;
        private Button btnClearSolve;
        private Button btnReset;
        private CheckBox chkManualEdit;
        private Button btnSetStart;
        private Button btnSetExit;
        private Label lblGenTime;
        private Label lblSolveTime;
        private Label lblExplored;
        private Label lblPathLen;
        private Label lblStatus;
        private Label lblDelay;
        private Label lblSize;
        private Label lblGenParams;
        private Label lblSolver;
        private Label lbl_GenerationAlg;
        private Label lbl_nudturnbias;
        private Label lbl_nudstepspertick;
        private NumericUpDown nudBraidInput;
        private Label lbl_nudbraid;
        private Label lbl_nudRoomDensity;
        private Label lbl_nudWidth;
        private Label lbl_nudHeight;
        private Label lbl_AgentParameters;
        private CheckBox chkBox_AgentActive;
        private Label lbl_LearningRate;
        private NumericUpDown num_LearningRate;
        private Label lbl_DiscountFactor;
        private NumericUpDown numDiscountFactor;
        private Label lbl_ExplorationRate;
        private NumericUpDown numExplorationRate;
        private Label lblDecay;
        private NumericUpDown numDecay;
        private Label lblEpisodes;
        private Label lblLiveEpisodes;
        private NumericUpDown numLiveEpisodes;
        private NumericUpDown numEpisodes;
        private Label lblMaxSteps;
        private NumericUpDown numMaxSteps;
        private Label lblStepDelay;
        private NumericUpDown numStepDelay;
        private Label lblAgentStatus;
        private Button btnRunAgent;
        private Button btnStopAgent;
        private Label lblBias;
        private NumericUpDown numBias;
        private Button btn_backHome;
        private Button btn_ExplainingParams;
    }
}
