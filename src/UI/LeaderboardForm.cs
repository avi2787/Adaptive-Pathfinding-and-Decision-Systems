using System;
using System.Globalization;
using System.Windows.Forms;

namespace NEA_ai_pathfinding
{
    public partial class LeaderboardForm : Form
    {
        private readonly SqlDatabase db = new SqlDatabase();

        public LeaderboardForm()
        {
            InitializeComponent();
            LoadScores();
        }


        private void LoadScores()
        {
            // SQL sorts by levels desc, time asc, coins desc (best run per user).
            var rows = db.GetGlobalLeaderboard(50);

            listView1.BeginUpdate();
            listView1.Items.Clear();

            if (rows.Count == 0 && db.LastCallFailed)
            {
                MessageBox.Show("Database unavailable. Continuing in guest mode.");
                listView1.EndUpdate();
                return;
            }

            int rank = 1;
            foreach (var row in rows)
            {
                var item = new ListViewItem(rank.ToString());
                item.SubItems.Add(row.Username);
                item.SubItems.Add(row.TotalTime.ToString("0.0"));
                item.SubItems.Add(row.TotalCoins.ToString());
                item.SubItems.Add(row.LevelsCleared.ToString());
                listView1.Items.Add(item);
                rank++;
            }

            listView1.EndUpdate();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void LeaderboardForm_Load(object sender, EventArgs e)
        {

        }

    }
}
