using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kopi
{
    class Deletion
    {
        public Deletion(Copyer.Context a_context, string a_destinationPath)
        {
            m_context = a_context;
            FileInfo fileInfo = new FileInfo(a_destinationPath);
            Name = fileInfo.Name;
            Path = a_destinationPath;
            Size = fileInfo.Length;
            LastModifiedTime = fileInfo.LastWriteTimeUtc;
        }

        public void Execute()
        {
            m_context.LogDelegate("Deleting file " + (m_context.NeverBackup ? "" : "(backed up) ") + Path);
            if (!m_context.DryRun)
            {
                if (m_context.NeverBackup)
                {
                    File.Delete(Path);
                }
                else
                {
                    // move file to recycle bin
                    FileSystem.DeleteFile(Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            }
        }

        public void Move(string a_moveToPath)
        {
            m_context.LogDelegate("Moving deleted file " + Path + " to new file location " + a_moveToPath);
            if (!m_context.DryRun)
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(a_moveToPath));
                File.Move(Path, a_moveToPath);
            }
        }

        private Copyer.Context m_context;
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime LastModifiedTime { get; set; }
    }
}
