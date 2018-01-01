﻿using MahApps.Metro.Controls;
using Monkeyspeak.Editor.Logging;
using Monkeyspeak.Logging;
using System.Windows.Documents;
using System.Windows.Media;

namespace Monkeyspeak.Editor
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : MetroWindow
    {
        private Paragraph paragraph;

        public ConsoleWindow()
        {
            InitializeComponent();
            this.paragraph = new Paragraph();
            console.Document = new FlowDocument(paragraph);

            Logger.LogOutput = new ConsoleWindowLogOutput(this);
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
            DataContext = this;
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
    }
}