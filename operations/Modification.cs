using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kopi
{
    class Modification
    {
        public Modification(Copyer.Context a_context, string a_sourcePath, string a_destinationPath)
        {
            m_context = a_context;
            SourcePath = a_sourcePath;
            DestinationPath = a_destinationPath;
            FileInfo sourceFileInfo = new FileInfo(a_sourcePath);
            SourceSize = sourceFileInfo.Length;
            FileInfo destinationFileInfo = new FileInfo(a_destinationPath);
            DestinationSize = destinationFileInfo.Length;
        }

        public long GetBytesRequired()
        {
            if(m_context.NeverBackup)
            {
                return SourceSize - DestinationSize;
            }
            return SourceSize;
        }

        public void Execute()
        {
            m_context.LogDelegate("Copying changed file " + (m_context.NeverBackup ? "" : "(old version backed up) ") + SourcePath + " to " + DestinationPath);
            if (!m_context.DryRun)
            {
                if (!m_context.NeverBackup)
                {
                    // move the old file to recycle bin first
                    FileSystem.DeleteFile(DestinationPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                File.Copy(SourcePath, DestinationPath, true);
            }
        }

        private Copyer.Context m_context;
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public long SourceSize { get; set; }
        public long DestinationSize { get; set; }
    }
}
