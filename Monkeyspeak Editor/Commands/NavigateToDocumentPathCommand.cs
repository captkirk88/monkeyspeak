using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Commands
{
    public class NavigateToDocumentPathCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            var doc = Editors.Instance.Selected;
            if (doc != null)
            {
                var docPath = Path.GetDirectoryName(doc.CurrentFilePath);
                if (Directory.Exists(docPath))
                    Process.Start(docPath);
            }
        }
    }
}
