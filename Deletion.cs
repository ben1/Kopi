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
        public Deletion(string a_destinationPath)
        {
            FileInfo fileInfo = new FileInfo(a_destinationPath);
            Name = fileInfo.Name;
            Path = a_destinationPath;
            Size = fileInfo.Length;
            LastModifiedTime = fileInfo.LastWriteTimeUtc;
        }

        public void Execute(bool a_backup)
        {
            if (a_backup)
            {
                // move file to recycle bin
                FileSystem.DeleteFile(Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            else
            {
                File.Delete(Path);
            }
        }

        public void Move(string a_moveToPath)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(a_moveToPath));
            File.Move(Path, a_moveToPath);
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime LastModifiedTime { get; set; }
    }
}
