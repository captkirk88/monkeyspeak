using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Logging;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Monkeyspeak.Editor
{
    public enum AppColor
    {
        Blue,
        Red,
        Green,
        Purple,
        Orange,
        Lime,
        Emerald,
        Teal,
        Cyan,
        Cobalt,
        Indigo,
        Violet,
        Pink,
        Magenta,
        Crimson,
        Amber,
        Yellow,
        Brown,
        Olive,
        Steel,
        Mauve,
        Taupe,
        Sienna
    }

    public enum AppTheme { Light, Dark }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private App()
        {
            InitializeComponent();
            Logger.LogOutput = new MultiLogOutput(new FileLogger(), new FileLogger(Level.Debug));
            DispatcherUnhandledException += (sender, e) =>
            {
                e.Handled = true;
                Logger.Error<App>(e.Exception);
            };
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow = new MainWindow(e.Args);
            MainWindow.Show();
        }

        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Logger.Error($"{sender.GetType().Name}: {args.ExceptionObject}");
            var app = new App();
            app.Run();
        }
    }
}