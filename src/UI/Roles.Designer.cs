namespace NEA_ai_pathfinding
{
    partial class roles
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(roles));
            blocker_check_box = new CheckBox();
            runner_check_box = new CheckBox();
            roles_title = new Label();
            roles_explaination_label = new Label();
            roles_question_label = new Label();
            btn_backHome = new Button();
            SuspendLayout();
            // 
            // blocker_check_box
            // 
            blocker_check_box.AutoSize = true;
            blocker_check_box.Font = new Font("Segoe Print", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            blocker_check_box.Location = new Point(436, 367);
            blocker_check_box.Name = "blocker_check_box";
            blocker_check_box.Size = new Size(149, 53);
            blocker_check_box.TabIndex = 0;
            blocker_check_box.Text = "Blocker";
            blocker_check_box.UseVisualStyleBackColor = true;
            blocker_check_box.CheckedChanged += blocker_check_box_CheckedChanged;
            // 
            // runner_check_box
            // 
            runner_check_box.AutoSize = true;
            runner_check_box.Font = new Font("Segoe Print", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            runner_check_box.Location = new Point(166, 367);
            runner_check_box.Name = "runner_check_box";
            runner_check_box.Size = new Size(148, 53);
            runner_check_box.TabIndex = 1;
            runner_check_box.Text = "Runner";
            runner_check_box.UseVisualStyleBackColor = true;
            runner_check_box.CheckedChanged += runner_check_box_CheckedChanged;
            // 
            // roles_title
            // 
            roles_title.AutoSize = true;
            roles_title.Cursor = Cursors.PanNW;
            roles_title.Font = new Font("Ink Free", 26F);
            roles_title.Location = new Point(212, 26);
            roles_title.Name = "roles_title";
            roles_title.Size = new Size(342, 54);
            roles_title.TabIndex = 2;
            roles_title.Text = "Choose your role! ";
            roles_title.Click += roles_title_Click;
            // 
            // roles_explaination_label
            // 
            roles_explaination_label.Font = new Font("Javanese Text", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            roles_explaination_label.Location = new Point(72, 111);
            roles_explaination_label.Name = "roles_explaination_label";
            roles_explaination_label.Size = new Size(652, 189);
            roles_explaination_label.TabIndex = 3;
            roles_explaination_label.Text = resources.GetString("roles_explaination_label.Text");
            roles_explaination_label.Click += roles_explaination_label_Click;
            // 
            // roles_question_label
            // 
            roles_question_label.AutoSize = true;
            roles_question_label.Font = new Font("Segoe Print", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            roles_question_label.Location = new Point(212, 290);
            roles_question_label.Name = "roles_question_label";
            roles_question_label.Size = new Size(334, 52);
            roles_question_label.TabIndex = 4;
            roles_question_label.Text = "Which one are YOU?";
            // 
            // btn_backHome
            // 
            btn_backHome.Font = new Font("Segoe Print", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_backHome.Location = new Point(693, 404);
            btn_backHome.Name = "btn_backHome";
            btn_backHome.Size = new Size(84, 34);
            btn_backHome.TabIndex = 5;
            btn_backHome.Text = "Back";
            btn_backHome.UseVisualStyleBackColor = true;
            btn_backHome.Click += btn_backHome_Click;
            // 
            // roles
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btn_backHome);
            Controls.Add(roles_question_label);
            Controls.Add(roles_explaination_label);
            Controls.Add(roles_title);
            Controls.Add(runner_check_box);
            Controls.Add(blocker_check_box);
            Name = "roles";
            Text = "Run or Block?";
            Load += roles_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox blocker_check_box;
        private CheckBox runner_check_box;
        private Label roles_title;
        private Label roles_explaination_label;
        private Label roles_question_label;
        private Button btn_backHome;
    }
}
