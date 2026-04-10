using System;
using System.Windows.Forms;

namespace NEA_ai_pathfinding
{
    // Home screen that lets the player pick Lab or Challenge mode
    public partial class HomeForm : Form
    {
        public HomeForm()
        {
            InitializeComponent();
            ShowCurrentUser();
        }

        private void ShowCurrentUser()
        {
            // simple label so guests know if they are logged in
            if (string.IsNullOrWhiteSpace(GameSession.CurrentUsername))
            {
                lblUser.Text = "User: Guest (Login from Challenge Mode)";
            }
            else
            {
                lblUser.Text = "User: " + GameSession.CurrentUsername;
            }
        }

        private void btnLab_Click(object sender, System.EventArgs e)
        {
            GameSession.SetModeLab();
            GameSession.CurrentRole = "Runner"; // default for sandbox
            GameSession.CurrentLevel = 1;

            this.Hide();
            using (LabMazeForm lab = new LabMazeForm())
            {
                lab.ShowDialog(this);
            }
            if (!this.IsDisposed)
            {
                this.Show();
                ShowCurrentUser();
            }
        }

        private void btnChallenge_Click(object sender, System.EventArgs e)
        {
            GameSession.SetModeChallenge();

            bool needsLogin = string.IsNullOrWhiteSpace(GameSession.CurrentUsername);
            this.Hide();
            if (needsLogin)
            {
                DialogResult pick = MessageBox.Show(
                    "Login is needed to save grades and scores. Continue to login?",
                    "Challenge Mode",
                    MessageBoxButtons.YesNo);

                if (pick == DialogResult.Yes)
                {
                    using (RegisterOrLoginForm log = new RegisterOrLoginForm())
                    {
                        log.ShowDialog(this);
                    }
                    ShowCurrentUser();
                    this.Show();
                    return;
                }
                else
                {
                    MessageBox.Show("Continuing as guest. Progress will not save.");
                    GameSession.ResetLevelProgress();
                }
            }

            GameSession.CurrentLevel = 1;
            using (roles rolePicker = new roles())
            {
                rolePicker.ShowDialog(this);
            }

            if (!this.IsDisposed)
            {
                this.Show();
                ShowCurrentUser();
            }
        }

        private void btnLeaderboard_Click(object sender, System.EventArgs e)
        {
            this.Hide();
            using (LeaderboardForm board = new LeaderboardForm())
            {
                board.ShowDialog(this);
            }
            if (!this.IsDisposed)
            {
                this.Show();
            }
        }

        private void btnExit_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void HomeForm_Load(object sender, EventArgs e)
        {

        }

        private void lblUser_Click(object sender, EventArgs e)
        {

        }

        private void lblSubtitle_Click(object sender, EventArgs e)
        {

        }
    }
}
