using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Shell;
using Monkeyspeak.Editor.Commands;
using Monkeyspeak.Editor.Logging;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Monkeyspeak.Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private static Exception lastException = null;

        private App()
        {
            InitializeComponent();

            DispatcherUnhandledException += (sender, e) =>
            {
                e.Handled = true;
                e.Exception.Log();
                if (e.Exception.TargetSite == lastException?.TargetSite)
                {
                    new ForceSaveAllCommand().Execute(null);
                    Application.Current.Shutdown(404);
                }
                lastException = e.Exception;
            };
            Startup += App_Startup;
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            MainWindow = new MainWindow(e.Args);
            MainWindow.Show();
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (MainWindow != null && MainWindow is MainWindow mw)
            {
                mw.ProcessArguments(args.ToArray());
            }
            return true;
        }

        [STAThread]
        public static void Main()
        {
            Logger.LogCallingMethod = false;
            var localAppDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Monkeyspeak", "logs");
            Logger.LogOutput = new MultiLogOutput(new FileLogOutput(localAppDataPath), new FileLogOutput(localAppDataPath, Level.Debug));

            if (SingleInstance<App>.InitializeAsFirstInstance("Monkeyspeak_Editor"))
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => Logger.Error($"{sender.GetType().Name}: {e.ExceptionObject}");
                var app = new App();
                app.Run();

                SingleInstance<App>.Cleanup();
            }
            else
            {
                Current.Shutdown();
            }
        }
    }
}