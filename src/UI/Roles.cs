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
    public partial class roles : Form
    {

        public string global_username;
        public roles()
        {
            InitializeComponent();
            global_username = GameSession.CurrentUsername;
            roles_title.Text = "Choose your role " + global_username + "!";
        }

        private void roles_Load(object sender, EventArgs e)
        {

        }

        public void runner_check_box_CheckedChanged(object sender, EventArgs e)
        {
            blocker_check_box.Checked = false; // only one role at a time
            MessageBox.Show("Get to the end before the timer runs out or the your lives run out!");

            string role = "Runner";
            GameSession.CurrentRole = role;
            using (RulesForm rules = new RulesForm())
            {
                rules.ShowDialog(this);
            }
            menu game_menu = new menu();
            this.Hide();
            game_menu.ShowDialog();
            this.Close();

        }

        private void roles_title_Click(object sender, EventArgs e)
        {

        }

        private void blocker_check_box_CheckedChanged(object sender, EventArgs e)
        {
            runner_check_box.Checked = false; // only one role at a time
            MessageBox.Show("Keep the runner away from the exit with abilities till the timer runs out or they lose all lives!");
            string role = "Blocker";
            GameSession.CurrentRole = role;
            using (RulesForm rules = new RulesForm())
            {
                rules.ShowDialog(this);
            }
            menu game_menu = new menu();
            this.Hide();
            game_menu.ShowDialog();
            this.Close();

        }

        private void btn_backHome_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void roles_explaination_label_Click(object sender, EventArgs e)
        {

        }
    }
}
