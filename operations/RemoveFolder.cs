using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kopi
{
    class RemoveFolder
    {
        public RemoveFolder(Copyer.Context a_context, string a_destinationPath)
        {
            m_context = a_context;
            DestinationPath = a_destinationPath;
        }

        public void Execute()
        {
            if (!m_context.DryRun && !m_context.NeverDelete)
            {
                Directory.Delete(DestinationPath, true);
            }
        }

        private Copyer.Context m_context;
        public string DestinationPath { get; set; }
    }
}