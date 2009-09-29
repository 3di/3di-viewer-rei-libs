namespace Dashboard
{
    partial class Dashboard
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.txtStatus = new System.Windows.Forms.Label();
            this.localChat1 = new OpenMetaverse.GUI.LocalChat();
            this.avatarList1 = new OpenMetaverse.GUI.AvatarList();
            this.friendsList1 = new OpenMetaverse.GUI.FriendList();
            this.groupList1 = new OpenMetaverse.GUI.GroupList();
            this.inventoryTree1 = new OpenMetaverse.GUI.InventoryTree();
            this.miniMap1 = new OpenMetaverse.GUI.MiniMap();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.miniMap1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(2, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.localChat1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(627, 419);
            this.splitContainer1.SplitterDistance = 415;
            this.splitContainer1.TabIndex = 4;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.miniMap1);
            this.splitContainer2.Size = new System.Drawing.Size(208, 419);
            this.splitContainer2.SplitterDistance = 209;
            this.splitContainer2.TabIndex = 9;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(208, 209);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.avatarList1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(200, 183);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Nearby";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.friendsList1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(200, 183);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Friends";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupList1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(200, 183);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Groups";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.inventoryTree1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(200, 183);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Inventory";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // txtStatus
            // 
            this.txtStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStatus.AutoEllipsis = true;
            this.txtStatus.Location = new System.Drawing.Point(10, 426);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(611, 13);
            this.txtStatus.TabIndex = 5;
            this.txtStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // localChat1
            // 
            this.localChat1.Client = null;
            this.localChat1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.localChat1.Location = new System.Drawing.Point(0, 0);
            this.localChat1.Name = "localChat1";
            this.localChat1.Size = new System.Drawing.Size(415, 419);
            this.localChat1.TabIndex = 3;
            // 
            // avatarList1
            // 
            this.avatarList1.Client = null;
            this.avatarList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.avatarList1.Location = new System.Drawing.Point(3, 3);
            this.avatarList1.Name = "avatarList1";
            this.avatarList1.Size = new System.Drawing.Size(194, 177);
            this.avatarList1.TabIndex = 2;
            this.avatarList1.UseCompatibleStateImageBehavior = false;
            this.avatarList1.View = System.Windows.Forms.View.Details;
            // 
            // friendsList1
            // 
            this.friendsList1.Client = null;
            this.friendsList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.friendsList1.Location = new System.Drawing.Point(3, 3);
            this.friendsList1.Name = "friendsList1";
            this.friendsList1.Size = new System.Drawing.Size(194, 177);
            this.friendsList1.TabIndex = 5;
            this.friendsList1.UseCompatibleStateImageBehavior = false;
            this.friendsList1.View = System.Windows.Forms.View.Details;
            // 
            // groupList1
            // 
            this.groupList1.Client = null;
            this.groupList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupList1.Location = new System.Drawing.Point(0, 0);
            this.groupList1.Name = "groupList1";
            this.groupList1.Size = new System.Drawing.Size(200, 183);
            this.groupList1.TabIndex = 7;
            this.groupList1.UseCompatibleStateImageBehavior = false;
            this.groupList1.View = System.Windows.Forms.View.Details;
            // 
            // inventoryTree1
            // 
            this.inventoryTree1.Client = null;
            this.inventoryTree1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inventoryTree1.Location = new System.Drawing.Point(0, 0);
            this.inventoryTree1.Name = "inventoryTree1";
            this.inventoryTree1.Size = new System.Drawing.Size(200, 183);
            this.inventoryTree1.TabIndex = 1;
            // 
            // miniMap1
            // 
            this.miniMap1.BackColor = System.Drawing.SystemColors.Control;
            this.miniMap1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.miniMap1.Client = null;
            this.miniMap1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.miniMap1.Location = new System.Drawing.Point(0, 0);
            this.miniMap1.Name = "miniMap1";
            this.miniMap1.Size = new System.Drawing.Size(208, 206);
            this.miniMap1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.miniMap1.TabIndex = 11;
            this.miniMap1.TabStop = false;
            // 
            // Dashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 443);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Dashboard";
            this.Text = "Dashboard";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.miniMap1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private OpenMetaverse.GUI.LocalChat localChat1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private OpenMetaverse.GUI.AvatarList avatarList1;
        private System.Windows.Forms.TabPage tabPage2;
        private OpenMetaverse.GUI.FriendList friendsList1;
        private System.Windows.Forms.TabPage tabPage3;
        private OpenMetaverse.GUI.GroupList groupList1;
        private System.Windows.Forms.TabPage tabPage4;
        private OpenMetaverse.GUI.InventoryTree inventoryTree1;
        private OpenMetaverse.GUI.MiniMap miniMap1;
        private System.Windows.Forms.Label txtStatus;

    }
}

