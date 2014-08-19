using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kopi
{
    class AddFolder
    {
        public AddFolder(string a_path)
        {
            DestinationPath = a_path;
        }

        public void Execute()
        {
            Directory.CreateDirectory(DestinationPath);
        }

        public string DestinationPath { get; set; }
    }
}
