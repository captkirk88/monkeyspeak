using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;
using Monkeyspeak.Logging;

namespace Monkeyspeak.Editor.Extensions
{
    public static class ThemeHelper
    {
        public static Brush ToThemeBackground()
        {
            var theme = (Application.Current.MainWindow as MainWindow).GetTheme();
            if (theme == AppTheme.Dark)
                return Brushes.DarkGray;
            else return Brushes.White;
        }

        public static Brush ToThemeForeground()
        {
            var theme = (Application.Current.MainWindow as MainWindow).GetTheme();
            if (theme == AppTheme.Dark)
                return Brushes.White;
            else return Brushes.Black;
        }
    }
}