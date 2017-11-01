using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Eto.Forms;

using Eto.Drawing;

namespace Monkeyspeak.Editor
{
    public sealed class CodeTextBox : RichTextArea
    {
        private int lastcaretPos, endingCaret;
        private static MonkeyspeakEngine engine = new MonkeyspeakEngine();
        private Lexer lexer;

        public CodeTextBox()
        {
            this.AcceptsTab = true;
            this.Enabled = true;
        }

        public void RunCode(int cause = 0)
        {
            engine.LoadFromString(Text).Execute(cause);
        }

        private void ParseTokensForColoring()
        {
            if (Text.Length == 0) return;
            var stream = new MemoryStream();
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                streamWriter.Write(Encoding.UTF8.GetBytes(Text));

            stream.Seek(0, SeekOrigin.Begin);
            using (var reader = new SStreamReader(stream))
                lexer = new Lexer(engine, reader);

            Color color = Colors.Black;
            foreach (var token in lexer.Read())
            {
                switch (token.Type)
                {
                    case TokenType.TRIGGER:
                        color = Colors.Blue;

                        break;
                }
                var value = new string(lexer.Read(token.ValueStartPosition, token.Length));
                int index;
                while ((index = Text.IndexOf(value)) != -1)
                {
                    index -= value.Length;
                    Selection = new Range<int>(index, value.Length);
                    SelectionForeground = color;
                    Selection = new Range<int>();
                }
            }
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
        }

        protected override void OnCaretIndexChanged(EventArgs e)
        {
            ParseTokensForColoring();
            base.OnCaretIndexChanged(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
        }
    }
}