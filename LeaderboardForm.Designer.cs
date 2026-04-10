namespace NEA_ai_pathfinding
{
    partial class LeaderboardForm
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
            listView1 = new ListView();
            chRank = new ColumnHeader();
            chUser = new ColumnHeader();
            chTime = new ColumnHeader();
            chScore = new ColumnHeader();
            chLevelsCleared = new ColumnHeader();
            SuspendLayout();
            // 
            // listView1
            // 
            listView1.Alignment = ListViewAlignment.Default;
            listView1.Columns.AddRange(new ColumnHeader[] { chRank, chUser, chTime, chScore, chLevelsCleared });
            listView1.Cursor = Cursors.PanNW;
            listView1.Dock = DockStyle.Fill;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Location = new Point(0, 0);
            listView1.Margin = new Padding(3, 4, 3, 4);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new Size(615, 576);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
            // 
            // chRank
            // 
            chRank.Text = "#";
            chRank.Width = 30;
            // 
            // chUser
            // 
            chUser.Text = "User";
            chUser.Width = 120;
            // 
            // chTime
            // 
            chTime.Text = "Total Time (s)";
            chTime.Width = 130;
            // 
            // chScore
            // 
            chScore.Text = "Coins";
            chScore.Width = 100;
            // 
            // chLevelsCleared
            // 
            chLevelsCleared.Text = "Levels Cleared";
            chLevelsCleared.Width = 140;
            // 
            // LeaderboardForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(615, 576);
            Controls.Add(listView1);
            Cursor = Cursors.PanNW;
            Margin = new Padding(3, 4, 3, 4);
            Name = "LeaderboardForm";
            Text = "Leaderboard";
            Load += LeaderboardForm_Load;
            ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader chRank;
        private System.Windows.Forms.ColumnHeader chUser;
        private System.Windows.Forms.ColumnHeader chTime;
        private System.Windows.Forms.ColumnHeader chScore;
        private System.Windows.Forms.ColumnHeader chLevelsCleared;
    }
}
