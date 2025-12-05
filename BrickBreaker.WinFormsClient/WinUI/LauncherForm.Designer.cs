namespace BrickBreaker.WinFormsClient.WinUI
{
    partial class LauncherForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
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
            components = new System.ComponentModel.Container();
            layout = new System.Windows.Forms.TableLayoutPanel();
            groupApi = new System.Windows.Forms.GroupBox();
            apiLayout = new System.Windows.Forms.TableLayoutPanel();
            lblApiTitle = new System.Windows.Forms.Label();
            txtApiUrl = new System.Windows.Forms.TextBox();
            btnApplyApi = new System.Windows.Forms.Button();
            lblApiHint = new System.Windows.Forms.Label();
            groupAuth = new System.Windows.Forms.GroupBox();
            authLayout = new System.Windows.Forms.TableLayoutPanel();
            lblUsername = new System.Windows.Forms.Label();
            txtUsername = new System.Windows.Forms.TextBox();
            lblPassword = new System.Windows.Forms.Label();
            txtPassword = new System.Windows.Forms.TextBox();
            btnLogin = new System.Windows.Forms.Button();
            btnRegister = new System.Windows.Forms.Button();
            btnQuickPlay = new System.Windows.Forms.Button();
            groupActions = new System.Windows.Forms.GroupBox();
            actionsLayout = new System.Windows.Forms.TableLayoutPanel();
            btnStartGame = new System.Windows.Forms.Button();
            btnLogout = new System.Windows.Forms.Button();
            lblCurrentUser = new System.Windows.Forms.Label();
            lblLastScore = new System.Windows.Forms.Label();
            lblStatus = new System.Windows.Forms.Label();
            btnRefreshLeaderboard = new System.Windows.Forms.Button();
            groupLeaderboard = new System.Windows.Forms.GroupBox();
            leaderboardLayout = new System.Windows.Forms.TableLayoutPanel();
            panelLeaderboardHeader = new System.Windows.Forms.Panel();
            lblLeaderboardTitle = new System.Windows.Forms.Label();
            listLeaderboard = new System.Windows.Forms.ListView();
            columnRank = new System.Windows.Forms.ColumnHeader();
            columnPlayer = new System.Windows.Forms.ColumnHeader();
            columnScore = new System.Windows.Forms.ColumnHeader();
            columnWhen = new System.Windows.Forms.ColumnHeader();
            layout.SuspendLayout();
            groupApi.SuspendLayout();
            apiLayout.SuspendLayout();
            groupAuth.SuspendLayout();
            authLayout.SuspendLayout();
            groupActions.SuspendLayout();
            actionsLayout.SuspendLayout();
            groupLeaderboard.SuspendLayout();
            leaderboardLayout.SuspendLayout();
            panelLeaderboardHeader.SuspendLayout();
            SuspendLayout();
            // 
            // layout
            // 
            layout.ColumnCount = 2;
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            layout.Controls.Add(groupApi, 0, 0);
            layout.Controls.Add(groupAuth, 0, 1);
            layout.Controls.Add(groupActions, 0, 2);
            layout.Controls.Add(groupLeaderboard, 1, 1);
            layout.Dock = System.Windows.Forms.DockStyle.Fill;
            layout.Location = new System.Drawing.Point(0, 0);
            layout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            layout.Name = "layout";
            layout.RowCount = 3;
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            layout.Size = new System.Drawing.Size(1124, 681);
            layout.TabIndex = 0;
            layout.SetColumnSpan(groupApi, 2);
            layout.SetRowSpan(groupLeaderboard, 2);
            // 
            // groupApi
            // 
            groupApi.Controls.Add(apiLayout);
            groupApi.Dock = System.Windows.Forms.DockStyle.Fill;
            groupApi.Location = new System.Drawing.Point(4, 3);
            groupApi.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupApi.Name = "groupApi";
            groupApi.Padding = new System.Windows.Forms.Padding(8);
            groupApi.Size = new System.Drawing.Size(1116, 124);
            groupApi.TabIndex = 0;
            groupApi.TabStop = false;
            groupApi.Text = "Backend API";
            // 
            // apiLayout
            // 
            apiLayout.ColumnCount = 2;
            apiLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            apiLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            apiLayout.Controls.Add(lblApiTitle, 0, 0);
            apiLayout.Controls.Add(txtApiUrl, 0, 1);
            apiLayout.Controls.Add(btnApplyApi, 1, 1);
            apiLayout.Controls.Add(lblApiHint, 0, 2);
            apiLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            apiLayout.Location = new System.Drawing.Point(8, 24);
            apiLayout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            apiLayout.Name = "apiLayout";
            apiLayout.RowCount = 3;
            apiLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            apiLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            apiLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            apiLayout.Size = new System.Drawing.Size(1100, 92);
            apiLayout.TabIndex = 0;
            apiLayout.SetColumnSpan(lblApiTitle, 2);
            apiLayout.SetColumnSpan(lblApiHint, 2);
            // 
            // lblApiTitle
            // 
            lblApiTitle.AutoSize = true;
            lblApiTitle.Location = new System.Drawing.Point(4, 0);
            lblApiTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 8);
            lblApiTitle.Name = "lblApiTitle";
            lblApiTitle.Size = new System.Drawing.Size(112, 15);
            lblApiTitle.TabIndex = 0;
            lblApiTitle.Text = "API base URL (HTTP)";
            // 
            // txtApiUrl
            // 
            txtApiUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            txtApiUrl.Location = new System.Drawing.Point(4, 23);
            txtApiUrl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtApiUrl.Name = "txtApiUrl";
            txtApiUrl.Size = new System.Drawing.Size(952, 23);
            txtApiUrl.TabIndex = 1;
            // 
            // btnApplyApi
            // 
            btnApplyApi.Dock = System.Windows.Forms.DockStyle.Fill;
            btnApplyApi.Location = new System.Drawing.Point(964, 23);
            btnApplyApi.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnApplyApi.Name = "btnApplyApi";
            btnApplyApi.Size = new System.Drawing.Size(132, 23);
            btnApplyApi.TabIndex = 2;
            btnApplyApi.Text = "Apply";
            btnApplyApi.UseVisualStyleBackColor = true;
            // 
            // lblApiHint
            // 
            lblApiHint.AutoSize = true;
            lblApiHint.ForeColor = System.Drawing.SystemColors.GrayText;
            lblApiHint.Location = new System.Drawing.Point(4, 49);
            lblApiHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblApiHint.Name = "lblApiHint";
            lblApiHint.Size = new System.Drawing.Size(212, 15);
            lblApiHint.TabIndex = 3;
            lblApiHint.Text = "Example: http://localhost:5080 for local";
            // 
            // groupAuth
            // 
            groupAuth.Controls.Add(authLayout);
            groupAuth.Dock = System.Windows.Forms.DockStyle.Fill;
            groupAuth.Location = new System.Drawing.Point(4, 133);
            groupAuth.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupAuth.Name = "groupAuth";
            groupAuth.Padding = new System.Windows.Forms.Padding(8);
            groupAuth.Size = new System.Drawing.Size(382, 269);
            groupAuth.TabIndex = 1;
            groupAuth.TabStop = false;
            groupAuth.Text = "Player login";
            // 
            // authLayout
            // 
            authLayout.ColumnCount = 2;
            authLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            authLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            authLayout.Controls.Add(lblUsername, 0, 0);
            authLayout.Controls.Add(txtUsername, 1, 0);
            authLayout.Controls.Add(lblPassword, 0, 1);
            authLayout.Controls.Add(txtPassword, 1, 1);
            authLayout.Controls.Add(btnLogin, 0, 2);
            authLayout.Controls.Add(btnRegister, 1, 2);
            authLayout.Controls.Add(btnQuickPlay, 0, 3);
            authLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            authLayout.Location = new System.Drawing.Point(8, 24);
            authLayout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            authLayout.Name = "authLayout";
            authLayout.RowCount = 4;
            authLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            authLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            authLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            authLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            authLayout.Size = new System.Drawing.Size(366, 237);
            authLayout.TabIndex = 0;
            authLayout.SetColumnSpan(btnQuickPlay, 2);
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new System.Drawing.Point(4, 0);
            lblUsername.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new System.Drawing.Size(63, 15);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Username";
            lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtUsername
            // 
            txtUsername.Dock = System.Windows.Forms.DockStyle.Fill;
            txtUsername.Location = new System.Drawing.Point(132, 3);
            txtUsername.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new System.Drawing.Size(230, 23);
            txtUsername.TabIndex = 1;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new System.Drawing.Point(4, 36);
            lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(60, 15);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Password";
            lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPassword
            // 
            txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            txtPassword.Location = new System.Drawing.Point(132, 39);
            txtPassword.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '•';
            txtPassword.Size = new System.Drawing.Size(230, 23);
            txtPassword.TabIndex = 3;
            // 
            // btnLogin
            // 
            btnLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            btnLogin.Location = new System.Drawing.Point(4, 75);
            btnLogin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new System.Drawing.Size(120, 34);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = true;
            // 
            // btnRegister
            // 
            btnRegister.Dock = System.Windows.Forms.DockStyle.Fill;
            btnRegister.Location = new System.Drawing.Point(132, 75);
            btnRegister.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new System.Drawing.Size(230, 34);
            btnRegister.TabIndex = 5;
            btnRegister.Text = "Register";
            btnRegister.UseVisualStyleBackColor = true;
            // 
            // btnQuickPlay
            // 
            btnQuickPlay.Dock = System.Windows.Forms.DockStyle.Fill;
            btnQuickPlay.Location = new System.Drawing.Point(4, 115);
            btnQuickPlay.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnQuickPlay.Name = "btnQuickPlay";
            btnQuickPlay.Size = new System.Drawing.Size(358, 34);
            btnQuickPlay.TabIndex = 6;
            btnQuickPlay.Text = "Quick Play (no login)";
            btnQuickPlay.UseVisualStyleBackColor = true;
            // 
            // groupActions
            // 
            groupActions.Controls.Add(actionsLayout);
            groupActions.Dock = System.Windows.Forms.DockStyle.Fill;
            groupActions.Location = new System.Drawing.Point(4, 408);
            groupActions.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupActions.Name = "groupActions";
            groupActions.Padding = new System.Windows.Forms.Padding(8);
            groupActions.Size = new System.Drawing.Size(382, 270);
            groupActions.TabIndex = 2;
            groupActions.TabStop = false;
            groupActions.Text = "Actions";
            // 
            // actionsLayout
            // 
            actionsLayout.ColumnCount = 2;
            actionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            actionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            actionsLayout.Controls.Add(btnStartGame, 0, 0);
            actionsLayout.Controls.Add(btnLogout, 1, 0);
            actionsLayout.Controls.Add(lblCurrentUser, 0, 1);
            actionsLayout.Controls.Add(lblLastScore, 1, 1);
            actionsLayout.Controls.Add(lblStatus, 0, 2);
            actionsLayout.Controls.Add(btnRefreshLeaderboard, 0, 3);
            actionsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            actionsLayout.Location = new System.Drawing.Point(8, 24);
            actionsLayout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            actionsLayout.Name = "actionsLayout";
            actionsLayout.RowCount = 4;
            actionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            actionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            actionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            actionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            actionsLayout.Size = new System.Drawing.Size(366, 238);
            actionsLayout.TabIndex = 0;
            actionsLayout.SetColumnSpan(lblStatus, 2);
            actionsLayout.SetColumnSpan(btnRefreshLeaderboard, 2);
            // 
            // btnStartGame
            // 
            btnStartGame.Dock = System.Windows.Forms.DockStyle.Fill;
            btnStartGame.Enabled = false;
            btnStartGame.Location = new System.Drawing.Point(4, 3);
            btnStartGame.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Size = new System.Drawing.Size(175, 34);
            btnStartGame.TabIndex = 0;
            btnStartGame.Text = "Start Game";
            btnStartGame.UseVisualStyleBackColor = true;
            // 
            // btnLogout
            // 
            btnLogout.Dock = System.Windows.Forms.DockStyle.Fill;
            btnLogout.Enabled = false;
            btnLogout.Location = new System.Drawing.Point(187, 3);
            btnLogout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new System.Drawing.Size(175, 34);
            btnLogout.TabIndex = 1;
            btnLogout.Text = "Sign Out";
            btnLogout.UseVisualStyleBackColor = true;
            // 
            // lblCurrentUser
            // 
            lblCurrentUser.AutoSize = true;
            lblCurrentUser.Location = new System.Drawing.Point(4, 40);
            lblCurrentUser.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new System.Drawing.Size(120, 15);
            lblCurrentUser.TabIndex = 2;
            lblCurrentUser.Text = "Player: (not logged in)";
            // 
            // lblLastScore
            // 
            lblLastScore.AutoSize = true;
            lblLastScore.Location = new System.Drawing.Point(187, 40);
            lblLastScore.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblLastScore.Name = "lblLastScore";
            lblLastScore.Size = new System.Drawing.Size(96, 15);
            lblLastScore.TabIndex = 3;
            lblLastScore.Text = "Last score: none";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            lblStatus.ForeColor = System.Drawing.SystemColors.GrayText;
            lblStatus.Location = new System.Drawing.Point(4, 80);
            lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(358, 118);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Connect to the API to get started.";
            // 
            // btnRefreshLeaderboard
            // 
            btnRefreshLeaderboard.Dock = System.Windows.Forms.DockStyle.Right;
            btnRefreshLeaderboard.Location = new System.Drawing.Point(239, 201);
            btnRefreshLeaderboard.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnRefreshLeaderboard.Name = "btnRefreshLeaderboard";
            btnRefreshLeaderboard.Size = new System.Drawing.Size(123, 34);
            btnRefreshLeaderboard.TabIndex = 5;
            btnRefreshLeaderboard.Text = "Refresh Leaderboard";
            btnRefreshLeaderboard.UseVisualStyleBackColor = true;
            // 
            // groupLeaderboard
            // 
            groupLeaderboard.Controls.Add(leaderboardLayout);
            groupLeaderboard.Dock = System.Windows.Forms.DockStyle.Fill;
            groupLeaderboard.Location = new System.Drawing.Point(394, 133);
            groupLeaderboard.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupLeaderboard.Name = "groupLeaderboard";
            groupLeaderboard.Padding = new System.Windows.Forms.Padding(8);
            groupLeaderboard.Size = new System.Drawing.Size(726, 545);
            groupLeaderboard.TabIndex = 3;
            groupLeaderboard.TabStop = false;
            groupLeaderboard.Text = "Leaderboard";
            // 
            // leaderboardLayout
            // 
            leaderboardLayout.ColumnCount = 1;
            leaderboardLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            leaderboardLayout.Controls.Add(panelLeaderboardHeader, 0, 0);
            leaderboardLayout.Controls.Add(listLeaderboard, 0, 1);
            leaderboardLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            leaderboardLayout.Location = new System.Drawing.Point(8, 24);
            leaderboardLayout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            leaderboardLayout.Name = "leaderboardLayout";
            leaderboardLayout.RowCount = 2;
            leaderboardLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            leaderboardLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            leaderboardLayout.Size = new System.Drawing.Size(710, 513);
            leaderboardLayout.TabIndex = 0;
            // 
            // panelLeaderboardHeader
            // 
            panelLeaderboardHeader.Controls.Add(lblLeaderboardTitle);
            panelLeaderboardHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            panelLeaderboardHeader.Location = new System.Drawing.Point(4, 3);
            panelLeaderboardHeader.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panelLeaderboardHeader.Name = "panelLeaderboardHeader";
            panelLeaderboardHeader.Size = new System.Drawing.Size(702, 32);
            panelLeaderboardHeader.TabIndex = 0;
            // 
            // lblLeaderboardTitle
            // 
            lblLeaderboardTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            lblLeaderboardTitle.Location = new System.Drawing.Point(0, 0);
            lblLeaderboardTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblLeaderboardTitle.Name = "lblLeaderboardTitle";
            lblLeaderboardTitle.Size = new System.Drawing.Size(702, 32);
            lblLeaderboardTitle.TabIndex = 0;
            lblLeaderboardTitle.Text = "Top 10 scores";
            lblLeaderboardTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // listLeaderboard
            // 
            listLeaderboard.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnRank, columnPlayer, columnScore, columnWhen });
            listLeaderboard.Dock = System.Windows.Forms.DockStyle.Fill;
            listLeaderboard.FullRowSelect = true;
            listLeaderboard.GridLines = true;
            listLeaderboard.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listLeaderboard.HideSelection = false;
            listLeaderboard.Location = new System.Drawing.Point(4, 41);
            listLeaderboard.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listLeaderboard.MultiSelect = false;
            listLeaderboard.Name = "listLeaderboard";
            listLeaderboard.Size = new System.Drawing.Size(702, 469);
            listLeaderboard.TabIndex = 1;
            listLeaderboard.UseCompatibleStateImageBehavior = false;
            listLeaderboard.View = System.Windows.Forms.View.Details;
            // 
            // columnRank
            // 
            columnRank.Text = "#";
            columnRank.Width = 40;
            // 
            // columnPlayer
            // 
            columnPlayer.Text = "Player";
            columnPlayer.Width = 200;
            // 
            // columnScore
            // 
            columnScore.Text = "Score";
            columnScore.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnScore.Width = 100;
            // 
            // columnWhen
            // 
            columnWhen.Text = "Achieved";
            columnWhen.Width = 200;
            // 
            // LauncherForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1124, 681);
            Controls.Add(layout);
            MinimumSize = new System.Drawing.Size(960, 640);
            Name = "LauncherForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "BrickBreaker – WinForms Client";
            layout.ResumeLayout(false);
            groupApi.ResumeLayout(false);
            apiLayout.ResumeLayout(false);
            apiLayout.PerformLayout();
            groupAuth.ResumeLayout(false);
            authLayout.ResumeLayout(false);
            authLayout.PerformLayout();
            groupActions.ResumeLayout(false);
            actionsLayout.ResumeLayout(false);
            actionsLayout.PerformLayout();
            groupLeaderboard.ResumeLayout(false);
            leaderboardLayout.ResumeLayout(false);
            panelLeaderboardHeader.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.GroupBox groupApi;
        private System.Windows.Forms.TableLayoutPanel apiLayout;
        private System.Windows.Forms.Label lblApiTitle;
        private System.Windows.Forms.TextBox txtApiUrl;
        private System.Windows.Forms.Button btnApplyApi;
        private System.Windows.Forms.Label lblApiHint;
        private System.Windows.Forms.GroupBox groupAuth;
        private System.Windows.Forms.TableLayoutPanel authLayout;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnQuickPlay;
        private System.Windows.Forms.GroupBox groupActions;
        private System.Windows.Forms.TableLayoutPanel actionsLayout;
        private System.Windows.Forms.Button btnStartGame;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Label lblCurrentUser;
        private System.Windows.Forms.Label lblLastScore;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnRefreshLeaderboard;
        private System.Windows.Forms.GroupBox groupLeaderboard;
        private System.Windows.Forms.TableLayoutPanel leaderboardLayout;
        private System.Windows.Forms.ListView listLeaderboard;
        private System.Windows.Forms.ColumnHeader columnRank;
        private System.Windows.Forms.ColumnHeader columnPlayer;
        private System.Windows.Forms.ColumnHeader columnScore;
        private System.Windows.Forms.ColumnHeader columnWhen;
        private System.Windows.Forms.Panel panelLeaderboardHeader;
        private System.Windows.Forms.Label lblLeaderboardTitle;
    }
}
