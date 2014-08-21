using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kopi
{
    class AddFolder
    {
        public AddFolder(Copyer.Context a_context, string a_destinationPath)
        {
            m_context = a_context;
            DestinationPath = a_destinationPath;
        }

        public void Execute()
        {
            if (!m_context.DryRun)
            {
                Directory.CreateDirectory(DestinationPath);
            }
        }

        private Copyer.Context m_context;
        public string DestinationPath { get; set; }
    }
}
