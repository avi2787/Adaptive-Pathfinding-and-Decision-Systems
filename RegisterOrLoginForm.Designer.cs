namespace NEA_ai_pathfinding
{
    partial class RegisterOrLoginForm
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
            mySqlCommand1 = new MySql.Data.MySqlClient.MySqlCommand();
            label1 = new Label();
            label2 = new Label();
            usern = new TextBox();
            login_btn = new Button();
            register_btn = new Button();
            password_input = new TextBox();
            label3 = new Label();
            label4 = new Label();
            btn_BackToHome = new Button();
            SuspendLayout();
            // 
            // mySqlCommand1
            // 
            mySqlCommand1.CacheAge = 0;
            mySqlCommand1.Connection = null;
            mySqlCommand1.EnableCaching = false;
            mySqlCommand1.Transaction = null;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe Print", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(275, 119);
            label1.Name = "label1";
            label1.Size = new Size(152, 44);
            label1.TabIndex = 0;
            label1.Text = "Username:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe Print", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(275, 189);
            label2.Name = "label2";
            label2.Size = new Size(146, 44);
            label2.TabIndex = 2;
            label2.Text = "Password:";
            label2.Click += label2_Click;
            // 
            // usern
            // 
            usern.Location = new Point(433, 132);
            usern.Name = "usern";
            usern.Size = new Size(125, 27);
            usern.TabIndex = 3;
            usern.TextChanged += usern_TextChanged;
            // 
            // login_btn
            // 
            login_btn.Font = new Font("Segoe Print", 13.2000008F, FontStyle.Bold, GraphicsUnit.Point, 0);
            login_btn.Location = new Point(494, 281);
            login_btn.Name = "login_btn";
            login_btn.Size = new Size(171, 75);
            login_btn.TabIndex = 5;
            login_btn.Text = "Login";
            login_btn.UseVisualStyleBackColor = true;
            login_btn.Click += login_btn_Click;
            // 
            // register_btn
            // 
            register_btn.Font = new Font("Segoe Print", 13.2000008F, FontStyle.Bold, GraphicsUnit.Point, 0);
            register_btn.Location = new Point(166, 281);
            register_btn.Name = "register_btn";
            register_btn.Size = new Size(172, 75);
            register_btn.TabIndex = 6;
            register_btn.Text = "Register";
            register_btn.UseVisualStyleBackColor = true;
            register_btn.Click += register_btn_Click;
            // 
            // password_input
            // 
            password_input.Location = new Point(433, 202);
            password_input.Name = "password_input";
            password_input.PasswordChar = '*';
            password_input.Size = new Size(125, 27);
            password_input.TabIndex = 8;
            password_input.UseSystemPasswordChar = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(338, 20);
            label3.Name = "label3";
            label3.Size = new Size(0, 20);
            label3.TabIndex = 9;
            label3.Click += label3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe Print", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(46, 20);
            label4.Name = "label4";
            label4.Size = new Size(782, 44);
            label4.TabIndex = 10;
            label4.Text = "Register if you haven't already. Login if you have registered!";
            label4.Click += label4_Click;
            // 
            // btn_BackToHome
            // 
            btn_BackToHome.Font = new Font("Segoe Print", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_BackToHome.Location = new Point(699, 392);
            btn_BackToHome.Name = "btn_BackToHome";
            btn_BackToHome.Size = new Size(102, 43);
            btn_BackToHome.TabIndex = 11;
            btn_BackToHome.Text = "Back";
            btn_BackToHome.UseVisualStyleBackColor = true;
            btn_BackToHome.Click += btn_BackToHome_Click;
            // 
            // RegisterOrLoginForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(866, 450);
            Controls.Add(btn_BackToHome);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(password_input);
            Controls.Add(register_btn);
            Controls.Add(login_btn);
            Controls.Add(usern);
            Controls.Add(label2);
            Controls.Add(label1);
            Cursor = Cursors.PanNW;
            Name = "RegisterOrLoginForm";
            Text = "Lost? Log in or Register";
            Load += Intro_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MySql.Data.MySqlClient.MySqlCommand mySqlCommand1;
        private Label label1;
        private Label label2;
        private TextBox usern;
        private Button login_btn;
        private Button register_btn;
        private TextBox password_input;
        private Label label3;
        private Label label4;
        private Button btn_BackToHome;
    }
}