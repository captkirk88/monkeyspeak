using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Monkeyspeak.Editor.HelperClasses;

namespace Monkeyspeak.Editor.Controls
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : MetroWindow
    {
        public SettingsDialog()
        {
            InitializeComponent();
            DataContext = this;
            settingsProps.SelectedObject = Properties.Settings.Default;
            syntaxProps.SelectedObject = new { TODO = "TODO!" };
            HotkeyManager.Populate(this, hotkeysContainer, hotkeysContainer.FirstChild as StackPanel, hotkeysContainer.SecondChild as StackPanel);
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            HotkeyManager.Save();
            Properties.Settings.Default.Save();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}