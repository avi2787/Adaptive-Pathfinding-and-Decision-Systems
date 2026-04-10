namespace NEA_ai_pathfinding
{
    partial class HomeForm
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
            lblTitle = new Label();
            lblSubtitle = new Label();
            btnLab = new Button();
            btnChallenge = new Button();
            btnLeaderboard = new Button();
            btnExit = new Button();
            lblUser = new Label();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.Top;
            lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.Location = new Point(82, 22);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(725, 46);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Lost? Maze Simulator with Ai Pathfinding";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Anchor = AnchorStyles.Top;
            lblSubtitle.Font = new Font("Segoe UI", 11F);
            lblSubtitle.ForeColor = Color.Blue;
            lblSubtitle.Location = new Point(166, 92);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(560, 25);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Pick a mode to start testing or play the full challenge.";
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            lblSubtitle.Click += lblSubtitle_Click;
            // 
            // btnLab
            // 
            btnLab.Anchor = AnchorStyles.Top;
            btnLab.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnLab.Location = new Point(305, 178);
            btnLab.Name = "btnLab";
            btnLab.Size = new Size(274, 77);
            btnLab.TabIndex = 2;
            btnLab.Text = "Lab Mode";
            btnLab.UseVisualStyleBackColor = true;
            btnLab.Click += btnLab_Click;
            // 
            // btnChallenge
            // 
            btnChallenge.Anchor = AnchorStyles.Top;
            btnChallenge.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnChallenge.Location = new Point(305, 261);
            btnChallenge.Name = "btnChallenge";
            btnChallenge.Size = new Size(274, 80);
            btnChallenge.TabIndex = 3;
            btnChallenge.Text = "Challenge Mode";
            btnChallenge.UseVisualStyleBackColor = true;
            btnChallenge.Click += btnChallenge_Click;
            // 
            // btnLeaderboard
            // 
            btnLeaderboard.Anchor = AnchorStyles.Top;
            btnLeaderboard.Font = new Font("Segoe UI", 11F);
            btnLeaderboard.Location = new Point(318, 357);
            btnLeaderboard.Name = "btnLeaderboard";
            btnLeaderboard.Size = new Size(246, 74);
            btnLeaderboard.TabIndex = 4;
            btnLeaderboard.Text = "Leaderboard";
            btnLeaderboard.UseVisualStyleBackColor = true;
            btnLeaderboard.Click += btnLeaderboard_Click;
            // 
            // btnExit
            // 
            btnExit.Anchor = AnchorStyles.Top;
            btnExit.Font = new Font("Segoe UI", 11F);
            btnExit.Location = new Point(318, 437);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(246, 69);
            btnExit.TabIndex = 5;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // lblUser
            // 
            lblUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUser.Font = new Font("Segoe UI", 9F);
            lblUser.ForeColor = Color.FromArgb(192, 0, 0);
            lblUser.Location = new Point(272, 117);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(335, 58);
            lblUser.TabIndex = 6;
            lblUser.Text = "        \r\nUser: Guest (Login from Challenge Mode)\r\n";
            lblUser.TextAlign = ContentAlignment.MiddleCenter;
            lblUser.Click += lblUser_Click;
            // 
            // HomeForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(892, 518);
            Controls.Add(lblUser);
            Controls.Add(btnExit);
            Controls.Add(btnLeaderboard);
            Controls.Add(btnChallenge);
            Controls.Add(btnLab);
            Controls.Add(lblSubtitle);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "HomeForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Home";
            Load += HomeForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Button btnLab;
        private System.Windows.Forms.Button btnChallenge;
        private System.Windows.Forms.Button btnLeaderboard;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblUser;
    }
}
