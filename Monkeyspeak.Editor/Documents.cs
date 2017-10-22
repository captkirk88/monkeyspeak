using System.Collections.Generic;
using Eto.Forms;

namespace Monkeyspeak.Editor
{
    public struct Document
    {
        private RichTextArea editBox;
        private TabPage page;

        public Document(TabPage page, RichTextArea editBox)
        {
            this.page = page;
            this.editBox = editBox;
        }
    }
}