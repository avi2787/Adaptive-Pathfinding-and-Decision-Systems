using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Windows.Forms;

namespace NEA_ai_pathfinding
{
    public partial class RegisterOrLoginForm : Form
    {
        public RegisterOrLoginForm()
        {
            InitializeComponent();
        }

        private void login_btn_Click(object sender, EventArgs e)
        {
            string username = usern.Text.Trim();
            string password = password_input.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Invalid username or password. Try Again!");
                return;
            }

            SqlDatabase db = new SqlDatabase();
            bool authorised = db.LoginUser(username, password);

            if (authorised)
            {
                GameSession.CurrentUsername = username;

                GameSession.ResetLevelProgress();
                var scoreRows = db.GetScoresForUser(username);
                foreach (var row in scoreRows)
                {
                    GameSession.ApplyScoreRow(row.Role, row.Level, row.Seconds, row.Score);
                }

                roles game = new roles();
                this.Hide();
                game.ShowDialog();
                this.Close();
            }
            else
            {
                if (db.LastCallFailed)
                {
                    MessageBox.Show("Database unavailable. Continuing in guest mode.");
                    GameSession.CurrentUsername = "";
                    GameSession.ResetLevelProgress();
                    roles game = new roles();
                    this.Hide();
                    game.ShowDialog();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Login failed! Have you registered?");
                }
            }
        }

        private void register_btn_Click(object sender, EventArgs e)
        {
            string username = usern.Text.Trim();
            string password = password_input.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Invalid username or password. Try Again!");
                return;


            }

            SqlDatabase db = new SqlDatabase();
            bool created = db.RegisterUser(username, password);

            if (created)
            {
                MessageBox.Show("Account created, you can now log in");
            }
            else
            {
                if (db.LastCallFailed)
                {
                    MessageBox.Show("Database unavailable. Continuing in guest mode.");
                }
                else
                {
                    MessageBox.Show("That username already exists");
                }
            }

        }


        private void Intro_Load(object sender, EventArgs e)
        {

        }

        private void password_input_TextChanged(object sender, EventArgs e)
        {

        }

        private void password_input_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void usern_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void btn_BackToHome_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
