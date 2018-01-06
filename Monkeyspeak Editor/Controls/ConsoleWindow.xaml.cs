using ICSharpCode.AvalonEdit.Utils;
using MahApps.Metro.Controls;
using Monkeyspeak.Editor.Interfaces.Console;
using Monkeyspeak.Editor.Logging;
using Monkeyspeak.Editor.Notifications;
using Monkeyspeak.Extensions;
using Monkeyspeak.Logging;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Controls
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : MetroWindow, IConsole
    {
        private Paragraph paragraph;
        internal List<IConsoleCommand> commands;
        internal Deque<string> history;

        public ConsoleWindow()
        {
            InitializeComponent();
            this.paragraph = new Paragraph();
            console.Document = new FlowDocument(paragraph);
            history = new Deque<string>();
            commands = new List<IConsoleCommand>();
            foreach (var asm in ReflectionHelper.GetAllAssemblies())
            {
                foreach (var type in ReflectionHelper.GetAllTypesWithInterface<IConsoleCommand>(asm))
                {
                    if (ReflectionHelper.HasNoArgConstructor(type))
                    {
                        if (ReflectionHelper.TryCreate(type, out var consoleCommand))
                        {
                            commands.Add((IConsoleCommand)consoleCommand);
                        }
                    }
                }
            }
            DataContext = this;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            Hide();
        }

        public void Write(string output, Color color)
        {
            paragraph.Inlines.Add(new Run(output)
            {
                FontFamily = console.FontFamily,
                FontStyle = System.Windows.FontStyles.Normal,
                FontWeight = System.Windows.FontWeights.Normal,
                Foreground = new SolidColorBrush(color)
            });
            //paragraph.Inlines.Add(new LineBreak());
            scroll.ScrollToEnd();
        }

        public void WriteLine(string output, Color color)
        {
            paragraph.Inlines.Add(new Run(output)
            {
                FontFamily = console.FontFamily,
                FontStyle = System.Windows.FontStyles.Normal,
                FontWeight = System.Windows.FontWeights.Normal,
                Foreground = new SolidColorBrush(color)
            });
            paragraph.Inlines.Add(new LineBreak());
            scroll.ScrollToEnd();
            DataContext = this;
        }

        private void input_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                e.Handled = true;
                var commandsFound = commands.FindAll(c => input.Text.StartsWith(c.Command, StringComparison.InvariantCultureIgnoreCase));
                if (commandsFound.Count > 0)
                {
                    history.PushFront(input.Text);
                    foreach (var command in commandsFound)
                    {
                        command.Invoke(this, input.Text.Split(' '));
                    }
                }
                else
                {
                    WriteLine(input.Text, Colors.White);
                }
                input.Text = null;
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                if (history.Count > 0)
                {
                    input.Text = history.PopFront();
                }
            }
        }
    }
}