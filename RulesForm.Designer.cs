namespace NEA_ai_pathfinding
{
    partial class RulesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RulesForm));
            okButton = new Button();
            panel_Controls = new Panel();
            lbl_controls = new Label();
            panel_Controls.SuspendLayout();
            SuspendLayout();
            // 
            // okButton
            // 
            okButton.Font = new Font("Segoe Print", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            okButton.Location = new Point(266, 513);
            okButton.Name = "okButton";
            okButton.Size = new Size(239, 55);
            okButton.TabIndex = 2;
            okButton.Text = "Begin Challenge";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // panel_Controls
            // 
            panel_Controls.AutoScroll = true;
            panel_Controls.BorderStyle = BorderStyle.FixedSingle;
            panel_Controls.Controls.Add(lbl_controls);
            panel_Controls.Location = new Point(2, -1);
            panel_Controls.Name = "panel_Controls";
            panel_Controls.Size = new Size(797, 487);
            panel_Controls.TabIndex = 3;
            // 
            // lbl_controls
            // 
            lbl_controls.AutoSize = true;
            lbl_controls.Location = new Point(6, 9);
            lbl_controls.MaximumSize = new Size(750, 0);
            lbl_controls.Name = "lbl_controls";
            lbl_controls.Size = new Size(749, 980);
            lbl_controls.TabIndex = 0;
            lbl_controls.Text = resources.GetString("lbl_controls.Text");
            lbl_controls.Click += lbl_controls_Click;
            // 
            // RulesForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(803, 591);
            Controls.Add(panel_Controls);
            Controls.Add(okButton);
            Name = "RulesForm";
            Text = "Controls and Objectives";
            Load += RulesForm_Load;
            panel_Controls.ResumeLayout(false);
            panel_Controls.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button okButton;
        private Panel panel_Controls;
        private Label lbl_controls;
    }
}
