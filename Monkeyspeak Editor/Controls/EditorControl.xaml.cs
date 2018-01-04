using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Monkeyspeak.Editor.HelperClasses;
using Monkeyspeak.Editor.Interfaces.Plugins;
using Monkeyspeak.Editor.Plugins;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Xml;

namespace Monkeyspeak.Editor.Controls
{
    /// <summary>
    /// Interaction logic for EditorControl.xaml
    /// </summary>
    public partial class EditorControl : MetroTabItem, IEditor, INotifyPropertyChanged
    {
        public static EditorControl Current { get => Editors.Instance.Selected; set => Editors.Instance.Selected = value; }
        private static IPluginContainer pluginContainer = new DefaultPluginContainer();

        static EditorControl()
        {
            // load up monkeyspeak syntax higlighting
            IHighlightingDefinition monkeyspeakHighlighting;
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("Monkeyspeak.Editor.MonkeyspeakSyntax_default.xshd"))
            {
                if (s == null)
                    return;
                else
                {
                    using (XmlReader reader = new XmlTextReader(s))
                    {
                        monkeyspeakHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                            HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                    HighlightingManager.Instance.RegisterHighlighting(nameof(Monkeyspeak), new string[] { ".ms", ".ds" }, monkeyspeakHighlighting);
                }
            }
        }

        private string currentFilePath;
        private string _title;
        private bool _hasChanges;

        private FileSystemWatcher fileWatcher;

        public event PropertyChangedEventHandler PropertyChanged;

        public EditorControl()
        {
            InitializeComponent();
            DataContext = this;
            //textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".ms");
            Visibility = Visibility.Visible;
            textEditor.ShowLineNumbers = true;
            Title = "New";

            fileWatcher = new FileSystemWatcher
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess,
            };
            fileWatcher.Changed += FileWatcher_Raised;
            fileWatcher.Renamed += FileWatcher_Raised;
            fileWatcher.Deleted += FileWatcher_Raised;
        }

        private void FileWatcher_Raised(object sender, FileSystemEventArgs e)
        {
            if (sender == this) return;
            if (e.FullPath != CurrentFilePath && e.ChangeType != WatcherChangeTypes.Renamed) return;
            var result = MessageDialogResult.Negative;
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    fileWatcher.EnableRaisingEvents = false;
                    Dispatcher.Invoke(async () =>
                    {
                        result = await DialogManager.ShowMessageAsync(Application.Current.MainWindow as MetroWindow,
                                        "File Changed", $"The file {CurrentFilePath} was changed from a outside source, would you like to...?", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                                        new MetroDialogSettings { AffirmativeButtonText = "Load Changes", NegativeButtonText = "Ignore Current/Future Changes", FirstAuxiliaryButtonText = "Save As" });

                        if (result == MessageDialogResult.Affirmative)
                        {
                            Reload();
                        }
                        else if (result == MessageDialogResult.FirstAuxiliary)
                        {
                            SaveAs();
                        }
                    }).Wait();
                    break;

                case WatcherChangeTypes.Renamed:
                    fileWatcher.EnableRaisingEvents = false;
                    CurrentFilePath = e.FullPath;
                    break;

                case WatcherChangeTypes.Deleted:
                    fileWatcher.EnableRaisingEvents = false;
                    Dispatcher.Invoke(async () =>
                    {
                        result = await DialogManager.ShowMessageAsync(Application.Current.MainWindow as MetroWindow,
                        "File Deleted", $"The file {CurrentFilePath} was deleted from a outside source, would you like to...?  The editor will remain open regardless of your choice so that you don't lose changes.", MessageDialogStyle.AffirmativeAndNegative,
                        new MetroDialogSettings { AffirmativeButtonText = "Save Changes", NegativeButtonText = "Ignore This" });

                        if (result == MessageDialogResult.Affirmative)
                        {
                            Save();
                        }
                    }).Wait();
                    break;
            }
            fileWatcher.EnableRaisingEvents = true;
        }

        public string Title { get => _title; set => SetField(ref _title, value); }
        public string HighlighterLanguage => textEditor.SyntaxHighlighting.Name;

        public int CaretLine
        {
            get => textEditor.TextArea.Caret.Line;
        }

        public bool HasChanges { get => _hasChanges; set => SetField(ref _hasChanges, value); }

        public void InsertLine(int line, string text)
        {
            text = text.Replace(Environment.NewLine, string.Empty);
            var lines = new List<string>(textEditor.Text.Split('\n'));
            if (line > lines.Count)
                lines.Add(text);
            else
                lines.Insert(line, text);
            for (int i = lines.Count - 1; i >= 0; i--) lines[i] = lines[i].Replace("\n", string.Empty);
            textEditor.Text = string.Join("\n", lines);
        }

        public void AddLine(string text)
        {
            text = text.Replace(Environment.NewLine, string.Empty);
            textEditor.Text += Environment.NewLine + text;
        }

        public IList<string> Lines
        {
            get
            {
                var lines = new List<string>(textEditor.Text.Split('\n'));
                for (int i = lines.Count - 1; i >= 0; i--) lines[i] = lines[i].Replace("\n", string.Empty);
                return lines;
            }
        }

        public int WordCount => textEditor.Text.Length;

        // TODO improve selection to detect variables surrounded by {%var}
        public string SelectedWord { get => textEditor.Text.Substring(textEditor.SelectionStart, textEditor.Text.IndexOf(" ", textEditor.SelectionStart)); }

        /// <summary>
        /// Gets the selected line without the ending newline character.
        /// </summary>
        /// <value>
        /// The selected line.
        /// </value>
        public string SelectedLine { get => Lines[textEditor.TextArea.Caret.Line]; }

        /// <summary>
        /// Gets a value indicating whether this instance is the active editor, the one with the editor open.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active editor; otherwise, <c>false</c>.
        /// </value>
        public bool IsActiveEditor { get => Current == this; }

        public string CurrentFilePath
        {
            get => currentFilePath;
            set
            {
                currentFilePath = value;
                Title = System.IO.Path.GetFileNameWithoutExtension(currentFilePath);
                fileWatcher.Path = System.IO.Path.GetDirectoryName(currentFilePath);
                fileWatcher.Filter = System.IO.Path.GetFileName(currentFilePath);
                fileWatcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Sets the text color by navigating to the specified line and setting the color between the start and end position.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="color">Text color to set</param>
        public void SetTextColor(Color color, int line, int start, int end)
        {
            textEditor.TextArea.TextView.LineTransformers.Add(new WordColorizer(color, line, start, end));
        }

        public bool Open()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                RestoreDirectory = false, // opens dialog at last location used
                DefaultExt = ".ms",
                Filter = "Monkeyspeak Script |*.ms|All files (*.*)|*.*"
            };
            var opened = dlg.ShowDialog();
            if (opened ?? false)
            {
                CurrentFilePath = dlg.FileName;
                textEditor.Load(CurrentFilePath);
                textEditor.SyntaxHighlighting =
                        HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(CurrentFilePath)) ??
                        HighlightingManager.Instance.GetDefinition("Monkeyspeak");
            }
            return opened.GetValueOrDefault(false);
        }

        public void Save()
        {
            fileWatcher.EnableRaisingEvents = false;
            if (CurrentFilePath == null)
            {
                SaveFileDialog dlg = new SaveFileDialog
                {
                    DefaultExt = ".ms",
                    AddExtension = true,
                    Filter = "Monkeyspeak Script |*.ms|All files (*.*)|*.*"
                };
                if (dlg.ShowDialog() ?? false)
                {
                    CurrentFilePath = dlg.FileName;
                }
                else
                {
                    return;
                }
            }
            textEditor.Save(CurrentFilePath);
            HasChanges = false;
            fileWatcher.EnableRaisingEvents = true;
        }

        public void SaveAs(string fileName = null)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                DefaultExt = ".ms",
                AddExtension = true,
                FileName = fileName ?? System.IO.Path.GetFileName(CurrentFilePath),
                Filter = "Monkeyspeak Script |*.ms|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() ?? false)
            {
                CurrentFilePath = dlg.FileName;
            }
            else
            {
                return;
            }
            textEditor.Save(CurrentFilePath);
            HasChanges = false;
        }

        public void Reload()
        {
            textEditor.Load(CurrentFilePath);
            Save();
            textEditor.SyntaxHighlighting =
                HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(CurrentFilePath)) ??
                HighlightingManager.Instance.GetDefinition("Monkeyspeak");
        }

        public async Task CloseAsync()
        {
            if (HasChanges)
            {
                var result = await DialogManager.ShowMessageAsync((MetroWindow)Application.Current.MainWindow,
                    "Save?",
                    "Changes were detected.  Would you like to save before closing?", MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings { AffirmativeButtonText = "Save", NegativeButtonText = "Nah" });
                if (result == MessageDialogResult.Affirmative) Save();
            }

            Editors.Instance.Remove(this);
        }

        private void highlightingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void propertyGridComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (propertyGrid == null)
                return;
            switch (propertyGridComboBox.SelectedIndex)
            {
                case 0:
                    propertyGrid.SelectedObject = textEditor;
                    break;

                case 1:
                    propertyGrid.SelectedObject = textEditor.TextArea;
                    break;

                case 2:
                    propertyGrid.SelectedObject = textEditor.Options;
                    break;

                case 3:
                    propertyGrid.SelectedObject = pluginContainer.Plugins.ToArray();
                    break;
            }
        }

        private void GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Current == this) return;
            Current = this;
            pluginContainer.Execute(Current);
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (Current == this) return;
            Current = this;
            pluginContainer.Execute(Current);
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            HasChanges = true;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Undo)
            {
                // TODO remove save changes prompt if under eliminated those changes
            }
        }
    }
}