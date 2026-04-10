using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEA_ai_pathfinding
{
    public partial class menu : Form
    {
        public menu()
        {
            InitializeComponent();
            string role = GameSession.CurrentRole;

            runnersmenu runners_user_control = new runnersmenu();
            blockersmenu blockers_user_control = new blockersmenu();

            if (string.Equals(role, "Blocker", StringComparison.OrdinalIgnoreCase))
            {
                placeholder_panel.Controls.Add(blockers_user_control);
            }
            else if (string.Equals(role, "Runner", StringComparison.OrdinalIgnoreCase))
            {
                placeholder_panel.Controls.Add(runners_user_control);
            }
            else
            {
                MessageBox.Show("No role was selected. Please go back and choose Runner or Blocker.");
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void menu_Load(object sender, EventArgs e)
        {

        }

        private void changeRoleButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Changed your mind? Your data will be saved in either role if you are logged in!",
                "Change Role",
                MessageBoxButtons.OK);

            this.Hide();
            roles roleForm = new roles();
            roleForm.ShowDialog();
            this.Close();
        }

        private void btn_backHome_Click(object sender, EventArgs e)
        {
            //this.Hide();
            //HomeForm startscreen = new HomeForm();
            //startscreen.ShowDialog(this);
            //this.Close();
            Close();
            Close();
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            //Close();
            //Close();
            Application.Exit();
        }
    }
}
