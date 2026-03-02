namespace BrickBreaker.WinFormsClient.WinUI
{
    partial class LauncherForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            // Control instantiation.
            components = new System.ComponentModel.Container();
            layout = new System.Windows.Forms.TableLayoutPanel();
            panelHeader = new System.Windows.Forms.Panel();
            headerLayout = new System.Windows.Forms.TableLayoutPanel();
            btnSettings = new System.Windows.Forms.Button();
            groupAuth = new System.Windows.Forms.GroupBox();
            authLayout = new System.Windows.Forms.TableLayoutPanel();
            lblUsername = new System.Windows.Forms.Label();
            txtUsername = new System.Windows.Forms.TextBox();
            lblPassword = new System.Windows.Forms.Label();
            txtPassword = new System.Windows.Forms.TextBox();
            btnLogin = new System.Windows.Forms.Button();
            btnRegister = new System.Windows.Forms.Button();
            btnLogout = new System.Windows.Forms.Button();
            lblCurrentUser = new System.Windows.Forms.Label();
            lblLastScore = new System.Windows.Forms.Label();
            lblStatus = new System.Windows.Forms.Label();
            groupLeaderboard = new System.Windows.Forms.GroupBox();
            leaderboardLayout = new System.Windows.Forms.TableLayoutPanel();
            pnlLeaderboard = new LauncherForm.LeaderboardPanel();
            panelGameHost = new System.Windows.Forms.Panel();

            // Suspend layout updates while configuring controls.
            layout.SuspendLayout();
            panelHeader.SuspendLayout();
            headerLayout.SuspendLayout();
            groupAuth.SuspendLayout();
            authLayout.SuspendLayout();
            groupLeaderboard.SuspendLayout();
            leaderboardLayout.SuspendLayout();
            SuspendLayout();

            // Root two-column layout: game area (fluid) | sidebar (fixed 420px).
            // Rows 0 and 4 are percent fillers that vertically centre the sidebar content.
            layout.BackColor = System.Drawing.Color.FromArgb(8, 6, 20);
            layout.ColumnCount = 2;
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 420F));
            layout.Controls.Add(panelGameHost, 0, 0);
            layout.Controls.Add(panelHeader, 1, 1);
            layout.Controls.Add(groupAuth, 1, 2);
            layout.Controls.Add(groupLeaderboard, 1, 3);
            layout.Dock = System.Windows.Forms.DockStyle.Fill;
            layout.Location = new System.Drawing.Point(0, 0);
            layout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            layout.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
            layout.Name = "layout";
            layout.RowCount = 5;
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 240F));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 330F));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            layout.Size = new System.Drawing.Size(1400, 850);
            layout.TabIndex = 0;
            layout.SetRowSpan(panelGameHost, 5);

            // Header strip — background matches the canvas, houses the settings button.
            panelHeader.BackColor = System.Drawing.Color.FromArgb(8, 6, 20);
            panelHeader.Controls.Add(headerLayout);
            panelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            panelHeader.Location = new System.Drawing.Point(772, 3);
            panelHeader.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new System.Windows.Forms.Padding(12, 12, 12, 0);
            panelHeader.Size = new System.Drawing.Size(348, 84);
            panelHeader.TabIndex = 0;

            // Single-row layout that right-aligns the settings button.
            headerLayout.ColumnCount = 2;
            headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            headerLayout.Controls.Add(btnSettings, 1, 0);
            headerLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            headerLayout.Location = new System.Drawing.Point(12, 12);
            headerLayout.Name = "headerLayout";
            headerLayout.RowCount = 1;
            headerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            headerLayout.Size = new System.Drawing.Size(328, 60);
            headerLayout.TabIndex = 0;

            // Borderless gear button — blends with the background, subtle hover highlight.
            btnSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            btnSettings.BackColor = System.Drawing.Color.Transparent;
            btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(40, 255, 255, 255);
            btnSettings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(70, 255, 255, 255);
            btnSettings.ForeColor = System.Drawing.Color.White;
            btnSettings.Location = new System.Drawing.Point(242, 24);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new System.Drawing.Size(83, 33);
            btnSettings.TabIndex = 2;
            btnSettings.Text = "⚙";
            btnSettings.Font = new System.Drawing.Font("Consolas", 28F);
            btnSettings.UseVisualStyleBackColor = false;

            // "Player login" group — credentials, auth buttons, and session info.
            groupAuth.BackColor = System.Drawing.Color.FromArgb(22, 16, 44);
            groupAuth.Controls.Add(authLayout);
            groupAuth.ForeColor = System.Drawing.Color.White;
            groupAuth.Dock = System.Windows.Forms.DockStyle.Fill;
            groupAuth.Location = new System.Drawing.Point(772, 93);
            groupAuth.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupAuth.Name = "groupAuth";
            groupAuth.Padding = new System.Windows.Forms.Padding(8);
            groupAuth.Size = new System.Drawing.Size(348, 269);
            groupAuth.TabIndex = 1;
            groupAuth.TabStop = false;
            groupAuth.Text = "Player login";

            // Five-row grid: username, password, login/register, sign out, player info.
            authLayout.ColumnCount = 2;
            authLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            authLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            authLayout.Controls.Add(lblUsername, 0, 0);
            authLayout.Controls.Add(txtUsername, 1, 0);
            authLayout.Controls.Add(lblPassword, 0, 1);
            authLayout.Controls.Add(txtPassword, 1, 1);
            authLayout.Controls.Add(btnLogin, 0, 2);
            authLayout.Controls.Add(btnRegister, 1, 2);
            authLayout.Controls.Add(btnLogout, 0, 3);
            authLayout.Controls.Add(lblCurrentUser, 0, 4);
            authLayout.Controls.Add(lblLastScore, 1, 4);
            authLayout.BackColor = System.Drawing.Color.FromArgb(22, 16, 44);
            authLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            authLayout.Location = new System.Drawing.Point(8, 24);
            authLayout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            authLayout.Name = "authLayout";
            authLayout.RowCount = 5;
            authLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            authLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            authLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            authLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            authLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            authLayout.SetColumnSpan(btnLogout, 2);
            authLayout.Size = new System.Drawing.Size(366, 237);
            authLayout.TabIndex = 0;

            // Username label and input field.
            lblUsername.AutoSize = false;
            lblUsername.Dock = System.Windows.Forms.DockStyle.Fill;
            lblUsername.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new System.Drawing.Size(63, 15);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Username";
            lblUsername.ForeColor = System.Drawing.Color.White;
            lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            txtUsername.Dock = System.Windows.Forms.DockStyle.Fill;
            txtUsername.BackColor = System.Drawing.Color.FromArgb(15, 12, 32);
            txtUsername.ForeColor = System.Drawing.Color.White;
            txtUsername.Location = new System.Drawing.Point(132, 3);
            txtUsername.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new System.Drawing.Size(230, 23);
            txtUsername.TabIndex = 1;

            // Password label and masked input field.
            lblPassword.AutoSize = false;
            lblPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(60, 15);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Password";
            lblPassword.ForeColor = System.Drawing.Color.White;
            lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            txtPassword.BackColor = System.Drawing.Color.FromArgb(15, 12, 32);
            txtPassword.ForeColor = System.Drawing.Color.White;
            txtPassword.Location = new System.Drawing.Point(132, 39);
            txtPassword.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '•';
            txtPassword.Size = new System.Drawing.Size(230, 23);
            txtPassword.TabIndex = 3;

            // Login and Register buttons sit side by side.
            btnLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            btnLogin.BackColor = System.Drawing.Color.FromArgb(58, 41, 102);
            btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnLogin.ForeColor = System.Drawing.Color.White;
            btnLogin.Location = new System.Drawing.Point(4, 75);
            btnLogin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new System.Drawing.Size(120, 34);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = false;

            btnRegister.Dock = System.Windows.Forms.DockStyle.Fill;
            btnRegister.BackColor = System.Drawing.Color.FromArgb(58, 41, 102);
            btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRegister.ForeColor = System.Drawing.Color.White;
            btnRegister.Location = new System.Drawing.Point(132, 75);
            btnRegister.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new System.Drawing.Size(230, 34);
            btnRegister.TabIndex = 5;
            btnRegister.Text = "Register";
            btnRegister.UseVisualStyleBackColor = false;

            // Sign Out spans both columns; colour is toggled by ApplyState based on login state.
            btnLogout.Dock = System.Windows.Forms.DockStyle.Fill;
            btnLogout.BackColor = System.Drawing.Color.FromArgb(28, 22, 50);
            btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnLogout.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            btnLogout.Location = new System.Drawing.Point(187, 3);
            btnLogout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new System.Drawing.Size(175, 34);
            btnLogout.TabIndex = 1;
            btnLogout.Text = "Sign Out";
            btnLogout.UseVisualStyleBackColor = false;

            // Session info displayed after a successful login.
            lblCurrentUser.AutoSize = true;
            lblCurrentUser.ForeColor = System.Drawing.Color.White;
            lblCurrentUser.Location = new System.Drawing.Point(4, 40);
            lblCurrentUser.Margin = new System.Windows.Forms.Padding(4, 16, 4, 0);
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new System.Drawing.Size(120, 15);
            lblCurrentUser.TabIndex = 2;
            lblCurrentUser.Text = "Player: -";

            lblLastScore.AutoSize = true;
            lblLastScore.ForeColor = System.Drawing.Color.White;
            lblLastScore.Location = new System.Drawing.Point(187, 40);
            lblLastScore.Margin = new System.Windows.Forms.Padding(4, 16, 4, 0);
            lblLastScore.Name = "lblLastScore";
            lblLastScore.Size = new System.Drawing.Size(96, 15);
            lblLastScore.TabIndex = 3;
            lblLastScore.Text = "Best score: -";

            // Status line shown at the bottom of the leaderboard section.
            lblStatus.AutoSize = true;
            lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            lblStatus.ForeColor = System.Drawing.Color.Gray;
            lblStatus.Location = new System.Drawing.Point(4, 80);
            lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(358, 118);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Connect to the API to get started.";

            // "Leaderboard" group — owner-drawn table above a status line.
            groupLeaderboard.BackColor = System.Drawing.Color.FromArgb(22, 16, 44);
            groupLeaderboard.Controls.Add(leaderboardLayout);
            groupLeaderboard.ForeColor = System.Drawing.Color.White;
            groupLeaderboard.Dock = System.Windows.Forms.DockStyle.Fill;
            groupLeaderboard.Location = new System.Drawing.Point(772, 549);
            groupLeaderboard.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupLeaderboard.Name = "groupLeaderboard";
            groupLeaderboard.Padding = new System.Windows.Forms.Padding(8);
            groupLeaderboard.Size = new System.Drawing.Size(348, 545);
            groupLeaderboard.TabIndex = 3;
            groupLeaderboard.TabStop = false;
            groupLeaderboard.Text = "Leaderboard";

            // Leaderboard panel fills available height; status line pinned to the bottom.
            leaderboardLayout.ColumnCount = 1;
            leaderboardLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            leaderboardLayout.Controls.Add(pnlLeaderboard, 0, 0);
            leaderboardLayout.Controls.Add(lblStatus, 0, 1);
            leaderboardLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            leaderboardLayout.Location = new System.Drawing.Point(8, 24);
            leaderboardLayout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            leaderboardLayout.Name = "leaderboardLayout";
            leaderboardLayout.RowCount = 2;
            leaderboardLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            leaderboardLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            leaderboardLayout.Size = new System.Drawing.Size(710, 513);
            leaderboardLayout.TabIndex = 0;

            // Double-buffered owner-drawn panel for the custom leaderboard table.
            pnlLeaderboard.BackColor = System.Drawing.Color.FromArgb(15, 12, 32);
            pnlLeaderboard.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlLeaderboard.ForeColor = System.Drawing.Color.White;
            pnlLeaderboard.Location = new System.Drawing.Point(4, 41);
            pnlLeaderboard.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pnlLeaderboard.Name = "pnlLeaderboard";
            pnlLeaderboard.Size = new System.Drawing.Size(702, 469);
            pnlLeaderboard.TabIndex = 1;

            // Host panel for the embedded game view — spans all rows of the left column.
            panelGameHost.BackColor = System.Drawing.Color.FromArgb(8, 6, 20);
            panelGameHost.Dock = System.Windows.Forms.DockStyle.Fill;
            panelGameHost.Location = new System.Drawing.Point(4, 3);
            panelGameHost.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panelGameHost.Name = "panelGameHost";
            panelGameHost.Padding = new System.Windows.Forms.Padding(12);
            panelGameHost.Size = new System.Drawing.Size(760, 675);
            panelGameHost.TabIndex = 4;

            // Form settings.
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(8, 6, 20);
            ForeColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(1400, 850);
            Controls.Add(layout);
            MinimumSize = new System.Drawing.Size(1000, 680);
            Name = "LauncherForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "BrickBreaker – WinForms Client";

            // Resume deferred layout now that all controls are configured.
            layout.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            headerLayout.ResumeLayout(false);
            headerLayout.PerformLayout();
            groupAuth.ResumeLayout(false);
            authLayout.ResumeLayout(false);
            authLayout.PerformLayout();
            groupLeaderboard.ResumeLayout(false);
            leaderboardLayout.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.TableLayoutPanel headerLayout;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.GroupBox groupAuth;
        private System.Windows.Forms.TableLayoutPanel authLayout;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Label lblCurrentUser;
        private System.Windows.Forms.Label lblLastScore;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox groupLeaderboard;
        private System.Windows.Forms.TableLayoutPanel leaderboardLayout;
        private LauncherForm.LeaderboardPanel pnlLeaderboard;
        private System.Windows.Forms.Panel panelGameHost;
    }
}
