using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
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
        Red,
        Green,
        Blue,
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
        public ResourceDictionary ThemeDictionary
        {
            // You could probably get it via its name with some query logic as well.
            get { return Resources.MergedDictionaries[0]; }
        }

        public void SetColor(AppColor color)
        {
            Logger.Info(Enum.GetName(typeof(AppColor), color));
            Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
            ThemeManager.ChangeAppStyle(Application.Current,
                                    ThemeManager.GetAccent(Enum.GetName(typeof(AppColor), color)),
                                    appStyle.Item1);
        }

        public void SetTheme(AppTheme accent)
        {
            Logger.Info(Enum.GetName(typeof(AppTheme), accent));
            Tuple<MahApps.Metro.AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
            ThemeManager.ChangeAppStyle(Application.Current,
                                    appStyle.Item2,
                                    ThemeManager.GetAppTheme(Enum.GetName(typeof(AppTheme), accent)));
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error($"{sender.GetType().Name}: {e.Exception}");
        }
    }
}