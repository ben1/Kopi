namespace Kopi
{
	partial class Form1
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
			this.buttonBackup = new System.Windows.Forms.Button();
			this.textBoxLog = new System.Windows.Forms.TextBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.checkBoxDryRun = new System.Windows.Forms.CheckBox();
			this.folderGrid = new System.Windows.Forms.DataGridView();
			this.buttonLoad = new System.Windows.Forms.Button();
			this.buttonSave = new System.Windows.Forms.Button();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			((System.ComponentModel.ISupportInitialize)(this.folderGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonBackup
			// 
			this.buttonBackup.Location = new System.Drawing.Point(315, 3);
			this.buttonBackup.Name = "buttonBackup";
			this.buttonBackup.Size = new System.Drawing.Size(75, 23);
			this.buttonBackup.TabIndex = 3;
			this.buttonBackup.Text = "Backup";
			this.buttonBackup.UseVisualStyleBackColor = true;
			this.buttonBackup.Click += new System.EventHandler(this.buttonBackup_Click);
			// 
			// textBoxLog
			// 
			this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxLog.Location = new System.Drawing.Point(1, 212);
			this.textBoxLog.Multiline = true;
			this.textBoxLog.Name = "textBoxLog";
			this.textBoxLog.ReadOnly = true;
			this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxLog.Size = new System.Drawing.Size(481, 339);
			this.textBoxLog.TabIndex = 6;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(396, 3);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// checkBoxDryRun
			// 
			this.checkBoxDryRun.AutoSize = true;
			this.checkBoxDryRun.Location = new System.Drawing.Point(244, 7);
			this.checkBoxDryRun.Name = "checkBoxDryRun";
			this.checkBoxDryRun.Size = new System.Drawing.Size(65, 17);
			this.checkBoxDryRun.TabIndex = 2;
			this.checkBoxDryRun.Text = "Dry Run";
			this.checkBoxDryRun.UseVisualStyleBackColor = true;
			// 
			// folderGrid
			// 
			this.folderGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.folderGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.folderGrid.Location = new System.Drawing.Point(8, 51);
			this.folderGrid.Name = "folderGrid";
			this.folderGrid.Size = new System.Drawing.Size(466, 155);
			this.folderGrid.TabIndex = 5;
			// 
			// buttonLoad
			// 
			this.buttonLoad.Location = new System.Drawing.Point(12, 3);
			this.buttonLoad.Name = "buttonLoad";
			this.buttonLoad.Size = new System.Drawing.Size(95, 23);
			this.buttonLoad.TabIndex = 0;
			this.buttonLoad.Text = "Load Settings...";
			this.buttonLoad.UseVisualStyleBackColor = true;
			this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
			// 
			// buttonSave
			// 
			this.buttonSave.Location = new System.Drawing.Point(111, 3);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(92, 23);
			this.buttonSave.TabIndex = 1;
			this.buttonSave.Text = "Save Settings...";
			this.buttonSave.UseVisualStyleBackColor = true;
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(14, 30);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(455, 15);
			this.progressBar.TabIndex = 7;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(482, 553);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.buttonSave);
			this.Controls.Add(this.buttonLoad);
			this.Controls.Add(this.folderGrid);
			this.Controls.Add(this.checkBoxDryRun);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.textBoxLog);
			this.Controls.Add(this.buttonBackup);
			this.Name = "Form1";
			this.Text = "Kopi";
			((System.ComponentModel.ISupportInitialize)(this.folderGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonBackup;
		private System.Windows.Forms.TextBox textBoxLog;
		private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkBoxDryRun;
		private System.Windows.Forms.DataGridView folderGrid;
		private System.Windows.Forms.Button buttonLoad;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.ProgressBar progressBar;
	}
}

