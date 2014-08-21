
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
        public class Context
        {
            public LogDelegate LogDelegate { get; set; }
            public ProgressDelegate ProgressDelegate { get; set; }
            public bool DryRun { get; set; }
            public bool NeverDelete { get; set; }
            public bool NeverBackup { get; set; }
        }

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
            foreach (Updates updates in m_updates)
            {
                updates.Stop();
            }
        }

        private void BackupThread()
        {
            Dictionary<string,long> spaceNeeded = new Dictionary<string,long>();
            foreach (Mapping mapping in m_settings.Mappings)
            {
                Context context = new Context();
                context.LogDelegate = m_logDelegate;
                context.ProgressDelegate = m_progressDelegate;
                context.DryRun = m_dryRun;
                context.NeverBackup = mapping.NeverBackup;
                context.NeverDelete = mapping.NeverDelete;

                m_updates.Add(new Updates(context));
                CompareFolders(mapping, mapping.Source, mapping.Destination);
                long bytesRequired = 0;
                spaceNeeded.TryGetValue(mapping.Destination.Substring(0, 1).ToLower(), out bytesRequired);
                bytesRequired += m_updates.Last().GetBytesRequired();
                spaceNeeded[mapping.Destination.Substring(0, 1).ToLower()] = bytesRequired;
                if (m_stop)
                {
                    break;
                }
            }
            if(!m_stop)
            {
                foreach(KeyValuePair<string,long> kvp in spaceNeeded)
                {
                    DriveInfo driveInfo = new DriveInfo(kvp.Key);
                    if (kvp.Value >= driveInfo.AvailableFreeSpace)
                    {
                        string message = "Not synchronizing because it would exceed available disk space on drive letter " + kvp.Key + " by " + (kvp.Value - driveInfo.AvailableFreeSpace) + " bytes.";
                        Log(message);
                        MessageBox.Show(message);
                        spaceNeeded.Remove(kvp.Key);
                        m_stop = true;
                        break;
                    }
                }
                foreach (Updates updates in m_updates)
                {
                    if (m_stop)
                    {
                        break;
                    }
                    updates.Execute();
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
        private List<Updates> m_updates = new List<Updates>();


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
                    m_updates.Last().AddFolder(a_dst);
                    CopyFolder(a_src, a_dst);
                }
            }
            else if(dstExists)
            {
                m_updates.Last().RemoveFolder(a_dst);
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
                            m_updates.Last().Modify(a_src, a_dst);
                        }
                    }
                    else
                    {
                        m_updates.Last().Copy(a_src, a_dst);
                    }
                }
            }
            else if(dstFileInfo.Exists)
            {
                m_updates.Last().DeleteDestination(a_dst);
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
                m_updates.Last().DeleteDestination(file);
            }

            // Recurse into folders being deleted.
            string[] dstFolders = Directory.GetDirectories(a_folder);
            foreach (string folder in dstFolders)
            {
                // we don't need to call m_updates.RemoveFolder here because the parent removes recursively
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
                    m_updates.Last().Copy(file, Path.Combine(a_dst, Path.GetFileName(file)));
                }
            }

            // Copy each folder into its new directory.
            string[] srcFolders = Directory.GetDirectories(a_src);
            foreach (string folder in srcFolders)
            {
                m_updates.Last().AddFolder(Path.Combine(a_dst, Path.GetFileName(folder)));
                CopyFolder(folder, Path.Combine(a_dst, Path.GetFileName(folder)));
            }
        }

        private bool IgnoreFile(string a_src)
        {
            return Path.GetFileName(a_src).Equals("desktop.ini", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
