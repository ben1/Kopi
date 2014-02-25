using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kopi
{
	class Copyer
	{
        public delegate void LogDelegate(string a_str);
        public delegate void EventDelegate();
        public Copyer(LogDelegate a_log, EventDelegate a_stopped)
        {
            m_logDelegate = a_log;
            m_stoppedDelegate = a_stopped;
        }

		public bool Start(Settings a_settings, bool a_dryRun)
		{
			bool start = true;
			foreach (Mapping mapping in a_settings.Mappings)
			{
				if (!Directory.Exists(mapping.Source))
				{
					start = false;
					Log("Source folder \"" + mapping.Source + "\" does not exist.");
				}
				if (!Directory.Exists(mapping.Destination))
				{
					start = false;
					Log("Destination folder \"" + mapping.Destination + "\" does not exist.");
				}
				if (mapping.Source == mapping.Destination)
				{
					start = false;
					Log("Source and destination are the same.");
				}
				if (mapping.Destination == "C:\\")
				{
					start = false;
					Log("Destination is C:\\ which is dangerous.");
				}
			}
			if (start)
			{
				Log("Starting backup.");
				DateTime now = DateTime.Now;
				m_startTimeStamp = (now.Year % 100).ToString("D2") + now.Month.ToString("D2") + now.Day.ToString("D2") + now.Hour.ToString("D2") + now.Minute.ToString("D2") + now.Second.ToString("D2");
				m_settings = a_settings;
				m_dryRun = a_dryRun;
				m_stop = false;
				Thread thread = new Thread(new ThreadStart(BackupThread));
				thread.Start();
				return true;
			}
			else
			{
				Log("Failed to start backup.");
			}
			return false;
		}

		public void Stop()
		{
			m_stop = true;
		}

		private void BackupThread()
		{
			foreach (Mapping mapping in m_settings.Mappings)
			{
				CompareFolders(mapping.Source, mapping.Destination);
			}
			if (m_stop)
			{
				Log("Backup cancelled.");
			}
			else
			{
				Log("Backup complete.");
			}
			m_stoppedDelegate();
		}

        private const string BACKUP_FOLDER = "$SBV$";
        private LogDelegate m_logDelegate = delegate(string a_str) { };
        private EventDelegate m_stoppedDelegate = delegate() { };
		private Settings m_settings;
        private bool m_dryRun;
		private bool m_stop;
		private string m_startTimeStamp;

		private void Log(string a_str)
		{
			m_logDelegate(a_str);
		}

		private void CompareFolders(string a_src, string a_dst)
		{
			if (m_stop)
			{
				return;
			}

			bool srcExists = Directory.Exists(a_src);
			bool dstExists = Directory.Exists(a_dst);

			if(srcExists)
			{
				if(dstExists)
				{
					// recurse all files in a_src and a_dst
					string[] srcFiles = Directory.GetFiles(a_src);
					string[] dstFiles = Directory.GetFiles(a_dst);
					List<string> uniqueFiles = new List<string>(srcFiles.Length);
					foreach(string file in srcFiles)
					{
						uniqueFiles.Add(Path.GetFileName(file));
					}
					foreach(string file in dstFiles)
					{
						string f = Path.GetFileName(file);
						if(!uniqueFiles.Contains(f))
						{
							uniqueFiles.Add(f);
						}
					}
					foreach(string file in uniqueFiles)
					{
						CompareFiles(Path.Combine(a_src, file), Path.Combine(a_dst, file));
					}

					// recurse all folders in a_src and a_dst
					string[] srcFolders = Directory.GetDirectories(a_src);
					string[] dstFolders = Directory.GetDirectories(a_dst);
					List<string> uniqueFolders = new List<string>(srcFolders.Length);
					foreach (string folder in srcFolders)
					{
						// ignore backup folder in source folders, otherwise we do weird things to the destination backup folder
						string f = Path.GetFileName(folder);
						if (f != BACKUP_FOLDER)  
						{
							uniqueFolders.Add(f);
						}
					}
					foreach (string folder in dstFolders)
					{
						// ignore backup folder in destination folders of course, otherwise we'd delete it (because it shouldn't exist normally in the source)
						string f = Path.GetFileName(folder);
						if(f != BACKUP_FOLDER && !uniqueFolders.Contains(f))
						{
							uniqueFolders.Add(f);
						}
					}
					foreach (string folder in uniqueFolders)
					{
						CompareFolders(Path.Combine(a_src, folder), Path.Combine(a_dst, folder));
					}
				}
				else
				{
					Log("Copying to \"" + a_dst + "\".");
					CopyFolder(a_src, a_dst);
				}
			}
			else if(dstExists)
			{
				Log("Removing \"" + a_dst + "\".");
				BackupFolder(a_dst);
			}
		}

		private void CompareFiles(string a_src, string a_dst)
		{
			if (m_stop)
			{
				return;
			}

			FileInfo srcFileInfo = new FileInfo(a_src);
			FileInfo dstFileInfo = new FileInfo(a_dst);
			if (srcFileInfo.Exists)
			{
				if (dstFileInfo.Exists)
				{
					// compare size and timestamp, and if different, copy from src to dst
					DateTime srcDT;
					try
					{
						srcDT = srcFileInfo.LastWriteTime;
					}
					catch(ArgumentOutOfRangeException)
					{
						srcFileInfo.LastWriteTime = DateTime.Now;
						srcDT = srcFileInfo.LastWriteTime;
					}
					DateTime dstDT;
					try
					{
						dstDT = dstFileInfo.LastWriteTime;
					}
					catch(ArgumentOutOfRangeException)
					{
						dstFileInfo.LastWriteTime = DateTime.Now;
						dstDT = dstFileInfo.LastWriteTime;
					}

					// This will copy back the dst file times to the src, in the case where the src has had theirs reset by Box.com or something.
					//if (srcFileInfo.Length == dstFileInfo.Length && srcFileInfo.LastWriteTime != dstFileInfo.LastWriteTime)
					//{
					//    File.SetCreationTime(a_src, dstFileInfo.CreationTime);
					//    File.SetLastWriteTime(a_src, dstFileInfo.LastWriteTime);
					//}
					//else
					if (srcDT != dstDT || srcFileInfo.Length != dstFileInfo.Length)
					{
						Log("Updating \"" + a_dst + "\".");
						BackupFile(a_dst);
						CopyFile(a_src, a_dst);
					}
				}
				else
				{
					Log("Copying to \"" + a_dst + "\".");
					CopyFile(a_src, a_dst);
				}
			}
			else if(dstFileInfo.Exists)
			{
				Log("Removing \"" + a_dst + "\".");
				BackupFile(a_dst);
			}
		}

		// assumes a_folder exists
		private void BackupFolder(string a_folder)
		{
            if (m_stop || m_dryRun)
            {
                return;
            }

			// create backup location and make it hidden
			string backup = Path.Combine(Path.GetDirectoryName(a_folder), BACKUP_FOLDER);
			if (!Directory.Exists(backup))
			{
				Directory.CreateDirectory(backup);
				DirectoryInfo di = new DirectoryInfo(backup);
				if((di.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
				{
					di.Attributes = di.Attributes | FileAttributes.Hidden;
				}
			}

			// move folder to backup location
			Directory.Move(a_folder, Path.Combine(backup, m_startTimeStamp + "." + Path.GetFileName(a_folder)));
		}

		private void CopyFolder(string a_src, string a_dst)
		{
			if (m_stop || m_dryRun)
			{
				return;
			}

			// Check if the target directory exists, if not, create it.
			if(!Directory.Exists(a_dst))
			{
				Directory.CreateDirectory(a_dst);
			}

			// Copy each file into its new directory.
			string[] srcFiles = Directory.GetFiles(a_src);
			foreach(string file in srcFiles)
			{
				if (m_stop)
				{
					return;
				}
				CopyFile(Path.Combine(a_src, Path.GetFileName(file)), Path.Combine(a_dst, Path.GetFileName(file)));
			}

			// Copy each folder into its new directory.
			string[] srcFolders = Directory.GetDirectories(a_src);
			foreach (string folder in srcFolders)
			{
				CopyFolder(Path.Combine(a_src, Path.GetFileName(folder)), Path.Combine(a_dst, Path.GetFileName(folder)));
			}
		}

		// assumes a_file exists
		private void BackupFile(string a_file)
		{
            if (m_stop || m_dryRun)
            {
                return;
            }

			// create backup location and make it hidden
			string backup = Path.Combine(Path.GetDirectoryName(a_file), BACKUP_FOLDER);
			if (!Directory.Exists(backup))
			{
				Directory.CreateDirectory(backup);
				DirectoryInfo di = new DirectoryInfo(backup);
				if((di.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
				{
					di.Attributes = di.Attributes | FileAttributes.Hidden;
				}
			}

			// move file to backup location
			File.Move(a_file, Path.Combine(backup, m_startTimeStamp + "." + Path.GetFileName(a_file)));
		}

		// assumes a_src exists, and a_dst does not exist
		private void CopyFile(string a_src, string a_dst)
		{
            if (m_stop || m_dryRun)
            {
                return;
            }

            File.Copy(a_src, a_dst, false);
		}
	}
}
