using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Monkeyspeak.Editor.Interfaces.Notifications;

namespace Monkeyspeak.Editor.Notifications
{
    internal class ExceptionNotification : ICriticalNotification
    {
        private static ExceptionNotification _inst = new ExceptionNotification();

        public static ExceptionNotification Instance
        {
            get
            {
                try
                {
                    return _inst;
                }
                finally { _inst = null; }
            }
        }

        private readonly string message;
        private StackPanel content;

        public object Content => content;

        private ExceptionNotification()
        {
            double screenLeft = Application.Current.MainWindow.Left;
            double screenTop = Application.Current.MainWindow.Top;
            double screenWidth = Application.Current.MainWindow.Width;
            double screenHeight = Application.Current.MainWindow.Height;

            using (Bitmap bmp = new Bitmap((int)screenWidth,
                (int)screenHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    var path = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Monkeyspeak", "logs", "ScreenCapture.png");
                    g.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, bmp.Size);
                    bmp.Save(path);
                }
            }

            message = $"An exception occured!\nA screenshot and log have been made.\nPlease send the log and the screenshot to the developer!";
            MahApps.Metro.IconPacks.PackIconModernKind result = MahApps.Metro.IconPacks.PackIconModernKind.TransitHazard;
            System.Windows.Media.Brush foreground = System.Windows.Media.Brushes.Red;
            var img = new Button()
            {
                Content = new MahApps.Metro.IconPacks.PackIconModern
                {
                    Kind = result,
                    Foreground = foreground
                },
                IsHitTestVisible = false,
                BorderBrush = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left,
                HorizontalContentAlignment = HorizontalAlignment.Left,
            };

            var tb = new TextBlock()
            {
                Text = message,
                TextTrimming = TextTrimming.None,
                TextWrapping = TextWrapping.Wrap,
                Width = double.NaN,
                ToolTip = message,
            };

            var getLogButton = new Button()
            {
                Content = "Get Log"
            };
            getLogButton.Click += (sender, e) =>
                System.Diagnostics.Process.Start(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Monkeyspeak", "logs"));

            var iconAndTextLayout = new StackPanel() { Orientation = Orientation.Horizontal, Width = double.NaN };
            content = new StackPanel();
            iconAndTextLayout.Children.Add(img);
            iconAndTextLayout.Children.Add(tb);
            content.Children.Add(iconAndTextLayout);
            content.Children.Add(getLogButton);
        }
    }
}