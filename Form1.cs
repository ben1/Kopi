using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Kopi
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			// setup grid view
			DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
			column.Name = "Source";
			column.HeaderText = "Source Folder";
			column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			FolderPathCell cell = new FolderPathCell();
			column.CellTemplate = cell;
			folderGrid.Columns.Add(column);
			column = new DataGridViewTextBoxColumn();
			column.Name = "Destination";
			column.HeaderText = "Destination Folder";
			column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			column.CellTemplate = cell;
			folderGrid.Columns.Add(column);
			DataGridViewCheckBoxColumn column2 = new DataGridViewCheckBoxColumn();
			column2.Name = "Enabled";
			column2.HeaderText = "Enabled";
			column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			folderGrid.Columns.Add(column2);
			// set initial GUI state
			buttonCancel.Enabled = false;
			textBoxLog.Text = "Add pairs of source and destination folders by clicking in the cells, or load a pre-saved config.";

			// setup delegates
			Copyer.s_logDelegate = Log;
			Copyer.s_stoppedDelegate = Stopped;
		}

		private void buttonBackup_Click(object sender, EventArgs e)
		{
			// Collect settings from the grid
			Settings settings = new Settings();
			for (int row = 0; row < folderGrid.RowCount - 1; ++row)
			{
				if ((bool)folderGrid[2, row].Value)
				{
					settings.Mappings.Add(new Mapping(folderGrid[0, row].Value.ToString(), folderGrid[1, row].Value.ToString()));
				}
			}

			// Start the copyer thread
			if (Copyer.Start(settings, this.checkBoxDryRun.Checked))
			{
				buttonBackup.Enabled = false;
				buttonCancel.Enabled = true;
			}
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Copyer.Stop();
		}

		private delegate void LogDelegate();

		private void Log(string a_str)
		{
			LogDelegate ld = delegate()
			{
				textBoxLog.AppendText(Environment.NewLine + a_str);
			};
			this.Invoke(ld);
		}

		private delegate void StoppedDelegate();

		private void Stopped()
		{
			StoppedDelegate sd = delegate()
			{
				buttonBackup.Enabled = true;
				buttonCancel.Enabled = false;
			};
			this.Invoke(sd);
		}

		private void buttonLoad_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog ofd = new OpenFileDialog())
			{
				ofd.Filter = "config files (*.ini)|*.ini";
				ofd.RestoreDirectory = true;
				if (ofd.ShowDialog() == DialogResult.OK)
				{
					folderGrid.Rows.Clear(); // clears all rows except for the default new entry row
					string[] config = System.IO.File.ReadAllLines(ofd.FileName);
					foreach (string line in config)
					{
						string[] rowValues = line.Split(new char[] { '|' });
						folderGrid.Rows.Add();
						// index the row before the entry row, so subtract 2 from count rather than the usual 1
						folderGrid[0, folderGrid.Rows.Count - 2].Value = rowValues[0];
						folderGrid[1, folderGrid.Rows.Count - 2].Value = rowValues[1];
						bool enabled = false;
						if (rowValues.Count() > 2)
						{
							enabled = bool.Parse(rowValues[2]);
						}
						folderGrid[2, folderGrid.Rows.Count - 2].Value = enabled;
					}
				}
			}
		}

		private void buttonSave_Click(object sender, EventArgs e)
		{
			// write settings to an ini file
			using (SaveFileDialog sfd = new SaveFileDialog())
			{
				sfd.Filter = "config files (*.ini)|*.ini";
				sfd.RestoreDirectory = true ;
				if(sfd.ShowDialog() == DialogResult.OK)
				{
					string config = "";
					// don't save the default new entry row, so don't iterate over the last row
					for (int row = 0; row < folderGrid.RowCount - 1; ++row)
					{
						config += folderGrid[0, row].Value + "|" + folderGrid[1, row].Value + "|" + folderGrid[2, row].Value + "\r\n";
					}
					System.IO.File.WriteAllText(sfd.FileName, config);
				}
			}
		}
	}

	// Custom cell used as a template for all folder path cells in the grid control
	public class FolderPathCell : DataGridViewTextBoxCell
	{
		protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
		{
			if (base.DataGridView != null)
			{
				base.DataGridView.CurrentCell = base.DataGridView[e.ColumnIndex, e.RowIndex];
				if (e.Button == MouseButtons.Left)
				{
					using (FolderBrowserDialog dlg = new FolderBrowserDialog())
					{
						dlg.SelectedPath = @"c:\";
						dlg.Description = "Select a folder";
						if (dlg.ShowDialog() == DialogResult.OK)
						{
							this.Value = dlg.SelectedPath;
							this.DataGridView.NotifyCurrentCellDirty(true);
						}
					}
				}
				else if (e.Button == MouseButtons.Right)
				{
					base.DataGridView.BeginEdit(false);
				}
			}
		}
	}
}