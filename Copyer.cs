using Microsoft.VisualBasic.FileIO;
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
				CompareFolders(mapping, mapping.Source, mapping.Destination);
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

		private void CompareFolders(Mapping a_mapping, string a_src, string a_dst)
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
					HashSet<string> uniqueFiles = new HashSet<string>();
					foreach(string file in srcFiles)
					{
						uniqueFiles.Add(Path.GetFileName(file));
					}
					foreach(string file in dstFiles)
					{
						uniqueFiles.Add(Path.GetFileName(file));
					}
					foreach(string file in uniqueFiles)
					{
						CompareFiles(a_mapping, Path.Combine(a_src, file), Path.Combine(a_dst, file));
					}

					// recurse all folders in a_src and a_dst
					string[] srcFolders = Directory.GetDirectories(a_src);
					string[] dstFolders = Directory.GetDirectories(a_dst);
                    HashSet<string> uniqueFolders = new HashSet<string>();
					foreach (string folder in srcFolders)
					{
                        uniqueFolders.Add(Path.GetFileName(folder));
					}
					foreach (string folder in dstFolders)
					{
						uniqueFolders.Add(Path.GetFileName(folder));
					}
					foreach (string folder in uniqueFolders)
					{
						CompareFolders(a_mapping, Path.Combine(a_src, folder), Path.Combine(a_dst, folder));
					}
				}
				else
				{
					Log("Copying to \"" + a_dst + "\".");
					CopyFolder(a_src, a_dst);
				}
			}
			else if(dstExists && !a_mapping.NeverDelete)
			{
				Log("Removing \"" + a_dst + "\".");
				RemoveFolder(a_dst, !a_mapping.NeverBackup);
			}
		}

		private void CompareFiles(Mapping a_mapping, string a_src, string a_dst)
		{
			if (m_stop)
			{
				return;
			}

			FileInfo srcFileInfo = new FileInfo(a_src);
			FileInfo dstFileInfo = new FileInfo(a_dst);
			if (srcFileInfo.Exists)
            {
                if (!IgnoreFile(a_src))
                {
                    if (dstFileInfo.Exists)
                    {
                        // compare size and timestamp, and if different, copy from src to dst
                        DateTime srcDT;
                        try
                        {
                            srcDT = srcFileInfo.LastWriteTime;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            srcFileInfo.LastWriteTime = DateTime.Now;
                            srcDT = srcFileInfo.LastWriteTime;
                        }
                        DateTime dstDT;
                        try
                        {
                            dstDT = dstFileInfo.LastWriteTime;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            dstFileInfo.LastWriteTime = DateTime.Now;
                            dstDT = dstFileInfo.LastWriteTime;
                        }

                        if ((!a_mapping.IgnoreTimestamp && srcDT != dstDT) || srcFileInfo.Length != dstFileInfo.Length)
                        {
                            Log("Updating \"" + a_dst + "\".");
                            RemoveFile(a_dst, !a_mapping.NeverBackup);
                            CopyFile(a_src, a_dst);
                        }
                    }
                    else
                    {
                        Log("Copying to \"" + a_dst + "\".");
                        CopyFile(a_src, a_dst);
                    }
                }
			}
			else if(dstFileInfo.Exists && !a_mapping.NeverDelete)
			{
				Log("Removing \"" + a_dst + "\".");
				RemoveFile(a_dst, !a_mapping.NeverBackup);
			}
		}

		// assumes a_folder exists
		private void RemoveFolder(string a_folder, bool a_backup)
		{
            if (m_stop || m_dryRun)
            {
                return;
            }

            if (a_backup)
            {
                // move folder to recycle bin
                FileSystem.DeleteDirectory(a_folder, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            else
            {
                Directory.Delete(a_folder);
            }
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

                string a_srcFilePath = Path.Combine(a_src, Path.GetFileName(file));
                if (!IgnoreFile(a_srcFilePath))
                {
                    CopyFile(a_srcFilePath, Path.Combine(a_dst, Path.GetFileName(file)));
                }
			}

			// Copy each folder into its new directory.
			string[] srcFolders = Directory.GetDirectories(a_src);
			foreach (string folder in srcFolders)
			{
				CopyFolder(Path.Combine(a_src, Path.GetFileName(folder)), Path.Combine(a_dst, Path.GetFileName(folder)));
			}
		}

		// assumes a_file exists
		private void RemoveFile(string a_file, bool a_backup)
		{
            if (m_stop || m_dryRun)
            {
                return;
            }

            if (a_backup)
            {
                // move file to recycle bin
                FileSystem.DeleteFile(a_file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            else
            {
                File.Delete(a_file);
            }
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

        private bool IgnoreFile(string a_src)
        {
            return Path.GetFileName(a_src).Equals("desktop.ini", StringComparison.CurrentCultureIgnoreCase);
        }
	}
}
