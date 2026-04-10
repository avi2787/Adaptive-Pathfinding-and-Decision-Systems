namespace NEA_ai_pathfinding
{
    partial class ChallengeMazeForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>

        private void InitializeComponent()
        {
            panelAuth = new Panel();
            lblAuthStatus = new Label();
            panelMaze = new Panel();
            lbl_Score = new Label();
            lbl_DisplayMove = new Label();
            lbl_PushesLeft = new Label();
            lbl_MonstersLeft = new Label();
            lbl_lives_left = new Label();
            btnMonster = new Button();
            btnWind = new Button();
            btnPlaceWall = new Button();
            lbl_WallsLeft = new Label();
            btnPause = new Button();
            btnBackToMenu = new Button();
            btnStart = new Button();
            pbMaze = new PictureBox();
            lblUser = new Label();
            lblCountdown = new Label();
            lblMode = new Label();
            panelAuth.SuspendLayout();
            panelMaze.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbMaze).BeginInit();
            SuspendLayout();
            // 
            // panelAuth
            // 
            panelAuth.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelAuth.Controls.Add(lblAuthStatus);
            panelAuth.Location = new Point(14, 16);
            panelAuth.Margin = new Padding(3, 4, 3, 4);
            panelAuth.Name = "panelAuth";
            panelAuth.Size = new Size(1415, 882);
            panelAuth.TabIndex = 0;
            // 
            // lblAuthStatus
            // 
            lblAuthStatus.AutoSize = true;
            lblAuthStatus.ForeColor = Color.Firebrick;
            lblAuthStatus.Location = new Point(27, 200);
            lblAuthStatus.Name = "lblAuthStatus";
            lblAuthStatus.Size = new Size(0, 20);
            lblAuthStatus.TabIndex = 4;
            // 
            // panelMaze
            // 
            panelMaze.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelMaze.Controls.Add(lbl_Score);
            panelMaze.Controls.Add(lbl_DisplayMove);
            panelMaze.Controls.Add(lbl_PushesLeft);
            panelMaze.Controls.Add(lbl_MonstersLeft);
            panelMaze.Controls.Add(lbl_lives_left);
            panelMaze.Controls.Add(btnMonster);
            panelMaze.Controls.Add(btnWind);
            panelMaze.Controls.Add(btnPlaceWall);
            panelMaze.Controls.Add(lbl_WallsLeft);
            panelMaze.Controls.Add(btnPause);
            panelMaze.Controls.Add(btnBackToMenu);
            panelMaze.Controls.Add(btnStart);
            panelMaze.Controls.Add(pbMaze);
            panelMaze.Controls.Add(lblUser);
            panelMaze.Controls.Add(lblCountdown);
            panelMaze.Controls.Add(lblMode);
            panelMaze.Location = new Point(0, 0);
            panelMaze.Margin = new Padding(3, 4, 3, 4);
            panelMaze.Name = "panelMaze";
            panelMaze.Size = new Size(1438, 910);
            panelMaze.TabIndex = 1;
            panelMaze.Paint += panelMaze_Paint;
            // 
            // lbl_Score
            // 
            lbl_Score.AutoSize = true;
            lbl_Score.Font = new Font("Segoe Print", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl_Score.Location = new Point(302, 16);
            lbl_Score.Name = "lbl_Score";
            lbl_Score.Size = new Size(63, 30);
            lbl_Score.TabIndex = 20;
            lbl_Score.Text = "Coins:";
            lbl_Score.Click += lbl_Score_Click;
            // 
            // lbl_DisplayMove
            // 
            lbl_DisplayMove.AutoSize = true;
            lbl_DisplayMove.Font = new Font("Segoe Print", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl_DisplayMove.Location = new Point(3, 68);
            lbl_DisplayMove.Name = "lbl_DisplayMove";
            lbl_DisplayMove.Size = new Size(173, 30);
            lbl_DisplayMove.TabIndex = 19;
            lbl_DisplayMove.Text = "Blocker chose to- ";
            lbl_DisplayMove.Click += lbl_DisplayMove_Click;
            // 
            // lbl_PushesLeft
            // 
            lbl_PushesLeft.AutoSize = true;
            lbl_PushesLeft.Font = new Font("Segoe Script", 10.2F, FontStyle.Bold);
            lbl_PushesLeft.Location = new Point(879, 62);
            lbl_PushesLeft.Name = "lbl_PushesLeft";
            lbl_PushesLeft.Size = new Size(123, 28);
            lbl_PushesLeft.TabIndex = 18;
            lbl_PushesLeft.Text = "Pushes left: ";
            lbl_PushesLeft.Click += lbl_PushesLeft_Click;
            // 
            // lbl_MonstersLeft
            // 
            lbl_MonstersLeft.AutoSize = true;
            lbl_MonstersLeft.Font = new Font("Segoe Script", 10.2F, FontStyle.Bold);
            lbl_MonstersLeft.Location = new Point(695, 62);
            lbl_MonstersLeft.Name = "lbl_MonstersLeft";
            lbl_MonstersLeft.Size = new Size(145, 28);
            lbl_MonstersLeft.TabIndex = 17;
            lbl_MonstersLeft.Text = "Monsters Left: ";
            lbl_MonstersLeft.Click += lbl_MonstersLeft_Click;
            // 
            // lbl_lives_left
            // 
            lbl_lives_left.AutoSize = true;
            lbl_lives_left.Font = new Font("Segoe Print", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl_lives_left.ForeColor = Color.Red;
            lbl_lives_left.Location = new Point(478, 59);
            lbl_lives_left.Name = "lbl_lives_left";
            lbl_lives_left.Size = new Size(124, 31);
            lbl_lives_left.TabIndex = 16;
            lbl_lives_left.Text = "Lives Left: 3";
            // 
            // btnMonster
            // 
            btnMonster.Font = new Font("Segoe Script", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnMonster.Location = new Point(678, 9);
            btnMonster.Margin = new Padding(3, 4, 3, 4);
            btnMonster.Name = "btnMonster";
            btnMonster.Size = new Size(181, 43);
            btnMonster.TabIndex = 12;
            btnMonster.Text = "Place Monster";
            btnMonster.UseVisualStyleBackColor = true;
            btnMonster.Click += btnMonster_Click;
            // 
            // btnWind
            // 
            btnWind.Font = new Font("Segoe Script", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnWind.Location = new Point(865, 9);
            btnWind.Margin = new Padding(3, 4, 3, 4);
            btnWind.Name = "btnWind";
            btnWind.Size = new Size(137, 43);
            btnWind.TabIndex = 11;
            btnWind.Text = "Wind push";
            btnWind.UseVisualStyleBackColor = true;
            btnWind.Click += btnWind_Click;
            // 
            // btnPlaceWall
            // 
            btnPlaceWall.Font = new Font("Segoe Script", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPlaceWall.Location = new Point(1008, 9);
            btnPlaceWall.Margin = new Padding(3, 4, 3, 4);
            btnPlaceWall.Name = "btnPlaceWall";
            btnPlaceWall.Size = new Size(144, 43);
            btnPlaceWall.TabIndex = 10;
            btnPlaceWall.Text = "Place wall";
            btnPlaceWall.UseVisualStyleBackColor = true;
            btnPlaceWall.Click += btnPlaceWall_Click;
            // 
            // lbl_WallsLeft
            // 
            lbl_WallsLeft.AutoSize = true;
            lbl_WallsLeft.Font = new Font("Segoe Script", 10.2F, FontStyle.Bold);
            lbl_WallsLeft.Location = new Point(1029, 62);
            lbl_WallsLeft.Name = "lbl_WallsLeft";
            lbl_WallsLeft.Size = new Size(109, 28);
            lbl_WallsLeft.TabIndex = 9;
            lbl_WallsLeft.Text = "Walls left: ";
            lbl_WallsLeft.Click += lblWallsLeft_Click;
            // 
            // btnPause
            // 
            btnPause.Font = new Font("Segoe Script", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPause.ForeColor = Color.FromArgb(255, 128, 0);
            btnPause.Location = new Point(542, 11);
            btnPause.Margin = new Padding(3, 4, 3, 4);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(100, 39);
            btnPause.TabIndex = 13;
            btnPause.Text = "Pause";
            btnPause.UseVisualStyleBackColor = true;
            btnPause.Click += btnPause_Click;
            // 
            // btnBackToMenu
            // 
            btnBackToMenu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBackToMenu.Font = new Font("Segoe Script", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnBackToMenu.ForeColor = Color.Red;
            btnBackToMenu.Location = new Point(1251, 55);
            btnBackToMenu.Margin = new Padding(3, 4, 3, 4);
            btnBackToMenu.Name = "btnBackToMenu";
            btnBackToMenu.Size = new Size(155, 41);
            btnBackToMenu.TabIndex = 8;
            btnBackToMenu.Text = "Back to Menu";
            btnBackToMenu.UseVisualStyleBackColor = true;
            btnBackToMenu.Click += btnBackToMenu_Click;
            // 
            // btnStart
            // 
            btnStart.Font = new Font("Segoe Script", 11F, FontStyle.Bold);
            btnStart.ForeColor = Color.Lime;
            btnStart.Location = new Point(443, 11);
            btnStart.Margin = new Padding(3, 4, 3, 4);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(93, 40);
            btnStart.TabIndex = 2;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // pbMaze
            // 
            pbMaze.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pbMaze.BackColor = Color.Black;
            pbMaze.BorderStyle = BorderStyle.FixedSingle;
            pbMaze.Location = new Point(3, 102);
            pbMaze.Margin = new Padding(3, 4, 3, 4);
            pbMaze.Name = "pbMaze";
            pbMaze.Size = new Size(1435, 808);
            pbMaze.TabIndex = 1;
            pbMaze.TabStop = false;
            pbMaze.Click += pbMaze_Click;
            pbMaze.MouseClick += pbMaze_MouseClick;
            // 
            // lblUser
            // 
            lblUser.AutoSize = true;
            lblUser.Font = new Font("Segoe Script", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblUser.Location = new Point(3, 7);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(58, 30);
            lblUser.TabIndex = 0;
            lblUser.Text = "User:";
            lblUser.Click += lblUser_Click;
            // 
            // lblCountdown
            // 
            lblCountdown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblCountdown.Font = new Font("Segoe Script", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblCountdown.Location = new Point(1187, 10);
            lblCountdown.Name = "lblCountdown";
            lblCountdown.Size = new Size(184, 41);
            lblCountdown.TabIndex = 14;
            lblCountdown.Text = "Time left: --";
            lblCountdown.TextAlign = ContentAlignment.MiddleRight;
            lblCountdown.Click += lblCountdown_Click;
            // 
            // lblMode
            // 
            lblMode.AutoSize = true;
            lblMode.Font = new Font("Segoe Script", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblMode.Location = new Point(3, 37);
            lblMode.Name = "lblMode";
            lblMode.Size = new Size(81, 30);
            lblMode.TabIndex = 15;
            lblMode.Text = "Goal: -";
            lblMode.Click += lblMode_Click;
            // 
            // ChallengeMazeForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(1442, 914);
            Controls.Add(panelMaze);
            Controls.Add(panelAuth);
            Margin = new Padding(3, 4, 3, 4);
            Name = "ChallengeMazeForm";
            Text = "Lost? Find your way out!";
            Load += Form1_Load;
            panelAuth.ResumeLayout(false);
            panelAuth.PerformLayout();
            panelMaze.ResumeLayout(false);
            panelMaze.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbMaze).EndInit();
            ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.Panel panelAuth;
        private System.Windows.Forms.Label lblAuthStatus;
        private System.Windows.Forms.Panel panelMaze;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.PictureBox pbMaze;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnBackToMenu;
        private System.Windows.Forms.Label lbl_WallsLeft;
        private System.Windows.Forms.Button btnPlaceWall;
        private System.Windows.Forms.Button btnWind;
        private System.Windows.Forms.Button btnMonster;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Label lblCountdown;
        private System.Windows.Forms.Label lblMode;
        private Label lbl_lives_left;
        private Label lbl_MonstersLeft;
        private Label lbl_PushesLeft;
        private Label lbl_DisplayMove;
        private Label lbl_Score;
    }
}
