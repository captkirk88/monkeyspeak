﻿using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Commands;
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
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private App()
        {
            InitializeComponent();
            Logger.LogCallingMethod = false;
            Logger.LogOutput = new MultiLogOutput(new FileLogger(), new FileLogger(Level.Debug));
            Exception lastException = null;
            DispatcherUnhandledException += (sender, e) =>
            {
                e.Handled = true;
                e.Exception.Log();
                if (e.Exception.TargetSite == lastException?.TargetSite)
                {
                    new ForceSaveAllCommand().Execute(null);
                    new HelperClasses.GithubIssueTracker().SubmitIssue($"Auto-Generated Issue for {e?.Exception?.GetType()?.Name}", e.Exception).Wait();
                    Application.Current.Shutdown(404);
                }
                lastException = e.Exception;
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