using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NEA_ai_pathfinding
{
    public partial class runnersmenu : UserControl
    {
        private string _username;

        public runnersmenu()
        {
            InitializeComponent();
            _username = GameSession.CurrentUsername;

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void level1_btn_Click(object sender, EventArgs e)
        {
            StartLevel(1);
            GameSession.CurrentLevel = 1;
        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void runnersmenu_Load(object sender, EventArgs e)
        {
            if (comboMazeColor != null)
            {
                comboMazeColor.SelectedItem = GameSession.CurrentMazeColour;
            }
            UpdateLevelStatusLabels();
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void explainRunnerButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("As the Runner:\n"+
                "Your grade for each level is based on two things:\n" +
                "- How many token points you collect (1, 3 or 5 each)\n" +
                "- How quickly you reach the exit\n\n" +
                "Behind the scenes the game looks at roughly how many points " +
                "you earn per minute.\n" +
                "More points, collected faster, gives you a better grade (A is best, D is worst).",
                "How grading works",
                MessageBoxButtons.OK);
        }

        private void StartLevel(int levelNumber)
        {
            // block access until the previous level is cleared
            if (levelNumber > 1)
            {
                int prevIndex = levelNumber - 2;
                if (!GameSession.RunnerLevelCompleted[prevIndex])
                {
                    MessageBox.Show("You need to clear Level " + (levelNumber - 1) + " before you can play Level " + levelNumber + ".");
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

        private void UpdateLevelStatusLabels()
        {
            Label[] labels =
            {
                label13, // level 1
                label12, // level 2
                label10, // level 3
                label11, // level 4
                label9   // level 5
            };

            Button[] buttons =
            {
                level1_btn, // level 1
                button5,    // level 2
                button4,    // level 3
                button1,    // level 4
                button3    // level 5
            };

            for (int i = 0; i < labels.Length; i++)
            {
                string rating = GameSession.RunnerLevelRatings[i];
                bool completed = GameSession.RunnerLevelCompleted[i];
                double seconds = GameSession.RunnerLevelTimes[i];
                int score = GameSession.RunnerLevelScores[i];
                bool timedOutFound = GameSession.RunnerTimedOutFound[i];

                // lock later levels until the previous one is done
                if (i == 0)
                {
                    buttons[i].Enabled = true;
                }
                else
                {
                    buttons[i].Enabled = GameSession.RunnerLevelCompleted[i - 1];
                }

                if (timedOutFound && !completed)
                {
                    labels[i].Text = "Out of time";
                    labels[i].ForeColor = Color.Red;
                    runnerToolTip.SetToolTip(labels[i], "You ran out of time on this level. Try again.");
                }
                else if (!completed || string.IsNullOrWhiteSpace(rating))
                {
                    if (!string.IsNullOrWhiteSpace(rating) && rating.ToLower() == "failed")
                    {
                        labels[i].Text = "Failed";
                        labels[i].ForeColor = Color.Red;
                        runnerToolTip.SetToolTip(labels[i], "Attempted but not cleared yet.");
                    }
                    else
                    {
                        labels[i].Text = "Not played";
                        labels[i].ForeColor = Color.Black;
                        runnerToolTip.SetToolTip(labels[i], "You have not played this level yet.");
                    }
                }
                else
                {
                    labels[i].Text = "Completed: " + rating;
                    labels[i].ForeColor = Color.Green;

                    string info = "Time: " + seconds.ToString("0.0") + "s, "
                                  + "Score: " + score
                                  + " (tokens). Grade " + rating + " based on points per minute.";

                    runnerToolTip.SetToolTip(labels[i], info);
                }
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

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
