using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEA_ai_pathfinding
{

    // blocker menu: launch levels + show blocker progress; maze controls live in the form itself

    public partial class blockersmenu : UserControl
    {
        private string _username;

        public blockersmenu()
        {
            InitializeComponent();
            _username = GameSession.CurrentUsername;
        }

        private void level1label_Click(object sender, EventArgs e)
        {

        }

        private void blocker_title_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void level1_btn_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void blockersmenu_Load(object sender, EventArgs e)
        {
            if (comboMazeColor != null)
            {
                comboMazeColor.SelectedItem = GameSession.CurrentMazeColour;
            }
            UpdateLevelStatusLabels();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            StartLevel(1);
            GameSession.CurrentLevel = 1;
        }

        private void StartLevel(int levelNumber)
        {
            // stop players from skipping ahead
            if (levelNumber > 1)
            {
                int prevIndex = levelNumber - 2;
                if (!GameSession.BlockerLevelCompleted[prevIndex])
                {
                    MessageBox.Show("You need to clear Level " + (levelNumber - 1) + " before Level " + levelNumber + ".");
                    return;
                }
            }

            GameSession.CurrentLevel = levelNumber;
            ChallengeMazeForm game_playground = new ChallengeMazeForm();
            game_playground.ShowDialog();
            UpdateLevelStatusLabels();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            StartLevel(2);
            GameSession.CurrentLevel = 2;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StartLevel(3);
            GameSession.CurrentLevel = 3;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartLevel(4);
            GameSession.CurrentLevel = 4;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StartLevel(5);
            GameSession.CurrentLevel = 5;
        }

        private void explainBlockerButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "As the Blocker, your grade is based on how long it takes to stop the Runner!\n" +
                "" +
                "Less time taken to stop Runner = better letter (A is best, D is worst).\n" + 
                "Where D is just blocking till time runs out",
                "How grading works",
                MessageBoxButtons.OK);
        }

        private void UpdateLevelStatusLabels()
        {
            Label[] labels =
            {
                label1,  // level 1
                label12, // level 2
                label10, // level 3
                label11, // level 4
                label9   // level 5
            };

            Button[] buttons =
            {
                button8, // level 1
                button5, // level 2
                button4, // level 3
                button1, // level 4
                button3  // level 5
            };

            for (int i = 0; i < labels.Length; i++)
            {
                string rating = GameSession.BlockerLevelRatings[i];
                bool completed = GameSession.BlockerLevelCompleted[i];
                double seconds = GameSession.BlockerLevelTimes[i];
                int score = GameSession.BlockerLevelScores[i];
                bool heldOffRunner = GameSession.BlockerTimeoutWins[i];

                if (i == 0)
                {
                    buttons[i].Enabled = true;
                }
                else
                {
                    buttons[i].Enabled = GameSession.BlockerLevelCompleted[i - 1];
                }

                if (!completed || string.IsNullOrWhiteSpace(rating))
                {
                    if (!string.IsNullOrWhiteSpace(rating) && rating.ToLower() == "failed")
                    {
                        labels[i].Text = "Failed";
                        labels[i].ForeColor = Color.Red;
                        blockerToolTip.SetToolTip(labels[i], "Runner escaped. Try again.");
                    }
                    else
                    {
                        labels[i].Text = "Not played";
                        labels[i].ForeColor = Color.Black;
                        blockerToolTip.SetToolTip(labels[i], "You have not played this level yet.");
                    }
                }
                else
                {
                    if (heldOffRunner)
                    {
                        labels[i].Text = "Completed: " + rating + " (timeout)";
                        labels[i].ForeColor = Color.Green;
                        blockerToolTip.SetToolTip(labels[i], "You held the runner off until time ran out.");
                    }
                    else if (rating.ToLower() == "failed")
                    {
                        labels[i].Text = "Failed";
                        labels[i].ForeColor = Color.Red;
                        blockerToolTip.SetToolTip(labels[i], "Runner escaped. Try again.");
                    }
                    else
                    {
                        labels[i].Text = "Completed: " + rating;
                        labels[i].ForeColor = Color.Green;
                        string info = "Time: " + seconds.ToString("0.0") + "s, "
                                      + "Score: " + score
                                      + ". Grade " + rating + " based on how long you held them off.";

                        blockerToolTip.SetToolTip(labels[i], info);
                    }
                }
            }
        }

        private void comboMazeColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboMazeColor == null)
            {
                return;
            }

            string choice = comboMazeColor.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(choice))
            {
                return;
            }

            GameSession.CurrentMazeColour = choice;
        }
    }
}
