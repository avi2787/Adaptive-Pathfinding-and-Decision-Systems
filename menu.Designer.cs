namespace NEA_ai_pathfinding
{
    partial class menu
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
            placeholder_panel = new Panel();
            changeRoleButton = new Button();
            btn_Exit = new Button();
            btn_backHome = new Button();
            SuspendLayout();
            // 
            // placeholder_panel
            // 
            placeholder_panel.Location = new Point(-2, 5);
            placeholder_panel.Margin = new Padding(3, 4, 3, 4);
            placeholder_panel.Name = "placeholder_panel";
            placeholder_panel.Size = new Size(513, 574);
            placeholder_panel.TabIndex = 0;
            placeholder_panel.Paint += panel1_Paint;
            // 
            // changeRoleButton
            // 
            changeRoleButton.Location = new Point(23, 597);
            changeRoleButton.Margin = new Padding(3, 4, 3, 4);
            changeRoleButton.Name = "changeRoleButton";
            changeRoleButton.Size = new Size(130, 39);
            changeRoleButton.TabIndex = 1;
            changeRoleButton.Text = "Change Role";
            changeRoleButton.UseVisualStyleBackColor = true;
            changeRoleButton.Click += changeRoleButton_Click;
            // 
            // btn_Exit
            // 
            btn_Exit.Location = new Point(337, 597);
            btn_Exit.Margin = new Padding(3, 4, 3, 4);
            btn_Exit.Name = "btn_Exit";
            btn_Exit.Size = new Size(144, 39);
            btn_Exit.TabIndex = 2;
            btn_Exit.Text = "Close Program";
            btn_Exit.UseVisualStyleBackColor = true;
            btn_Exit.Click += btn_Exit_Click;
            // 
            // btn_backHome
            // 
            btn_backHome.Location = new Point(179, 597);
            btn_backHome.Margin = new Padding(3, 4, 3, 4);
            btn_backHome.Name = "btn_backHome";
            btn_backHome.Size = new Size(134, 39);
            btn_backHome.TabIndex = 3;
            btn_backHome.Text = "Back to Home";
            btn_backHome.UseVisualStyleBackColor = true;
            btn_backHome.Click += btn_backHome_Click;
            // 
            // menu
            // 
            AutoScaleDimensions = new SizeF(10F, 26F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(514, 660);
            Controls.Add(btn_backHome);
            Controls.Add(changeRoleButton);
            Controls.Add(placeholder_panel);
            Controls.Add(btn_Exit);
            Font = new Font("Segoe Print", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Margin = new Padding(3, 4, 3, 4);
            Name = "menu";
            Text = "Menu";
            Load += menu_Load;
            ResumeLayout(false);
        }

        #endregion

        private Panel placeholder_panel;
        private Button changeRoleButton;
        private Button btn_Exit;
        private Button btn_backHome;
    }
}
