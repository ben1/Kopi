using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kopi
{
    class RemoveFolder
    {
        public RemoveFolder(string a_path)
        {
            DestinationPath = a_path;
        }

        public void Execute()
        {
            Directory.Delete(DestinationPath);
        }

        public string DestinationPath { get; set; }
    }
}