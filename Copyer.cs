
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Kopi
{
    class Copyer
    {
        public delegate void LogDelegate(string a_str);
        public delegate void EventDelegate();
        public delegate void ProgressDelegate(int a_progressPercent);

        public Copyer(LogDelegate a_logDelegate, EventDelegate a_stoppedDelegate, ProgressDelegate a_progressDelegate)
        {
            if (a_logDelegate != null)
            {
                m_logDelegate = a_logDelegate;
            }
            if (a_stoppedDelegate != null)
            {
                m_stoppedDelegate = a_stoppedDelegate;
            }
            if (a_progressDelegate != null)
            {
                m_progressDelegate = a_progressDelegate;
            }
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
                m_updates = new Updates(!mapping.NeverBackup);
                CompareFolders(mapping, mapping.Source, mapping.Destination);
                if (!m_stop)
                {
                    // calculate disk space needed
                    long spaceNeeded = m_updates.GetBytesRequired();
                    DriveInfo driveInfo = new DriveInfo(mapping.Destination.Substring(0, 1));
                    if (spaceNeeded >= driveInfo.AvailableFreeSpace)
                    {
                        string message = "Skipping mapping from " + mapping.Source + " to " + mapping.Destination + " because it would exceed available disk space by " + (spaceNeeded - driveInfo.AvailableFreeSpace) + " bytes.";
                        Log(message);
                        MessageBox.Show(message);
                    }
                    else if(!m_dryRun)
                    {
                        // copy everything
                        m_updates.Execute(m_progressDelegate);
                    }
                }
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
        private ProgressDelegate m_progressDelegate = delegate(int a_progressPercent) { };
        private Settings m_settings;
        private bool m_dryRun;
        private bool m_stop;
        private Updates m_updates;


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
                    m_updates.AddFolder(a_dst);
                    CopyFolder(a_src, a_dst);
                }
            }
            else if(dstExists && !a_mapping.NeverDelete)
            {
                Log("Removing \"" + a_dst + "\".");
                m_updates.RemoveFolder(a_dst);
                RemoveFolder(a_dst);
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
                        if ((!a_mapping.IgnoreTimestamp && srcFileInfo.LastWriteTime != dstFileInfo.LastWriteTime) || srcFileInfo.Length != dstFileInfo.Length)
                        {
                            Log("Updating \"" + a_dst + "\".");
                            m_updates.Modify(a_src, a_dst);
                        }
                    }
                    else
                    {
                        Log("Copying to \"" + a_dst + "\".");
                        m_updates.Copy(a_src, a_dst);
                    }
                }
            }
            else if(dstFileInfo.Exists && !a_mapping.NeverDelete)
            {
                Log("Removing \"" + a_dst + "\".");
                m_updates.DeleteDestination(a_dst);
            }
        }

        // assumes a_folder exists
        private void RemoveFolder(string a_folder)
        {
            if (m_stop)
            {
                return;
            }

            // Record each file being deleted
            string[] dstFiles = Directory.GetFiles(a_folder);
            foreach (string file in dstFiles)
            {
                if (m_stop)
                {
                    return;
                }
                m_updates.DeleteDestination(file);
            }

            // Recurse into folders being deleted.
            string[] dstFolders = Directory.GetDirectories(a_folder);
            foreach (string folder in dstFolders)
            {
                RemoveFolder(folder);
            }
        }

        private void CopyFolder(string a_src, string a_dst)
        {
            if (m_stop)
            {
                return;
            }

            // Copy each file into its new directory.
            string[] srcFiles = Directory.GetFiles(a_src);
            foreach (string file in srcFiles)
            {
                if (m_stop)
                {
                    return;
                }

                if (!IgnoreFile(file))
                {
                    m_updates.Copy(file, Path.Combine(a_dst, Path.GetFileName(file)));
                }
            }

            // Copy each folder into its new directory.
            string[] srcFolders = Directory.GetDirectories(a_src);
            foreach (string folder in srcFolders)
            {
                CopyFolder(folder, Path.Combine(a_dst, Path.GetFileName(folder)));
            }
        }

        private bool IgnoreFile(string a_src)
        {
            return Path.GetFileName(a_src).Equals("desktop.ini", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
