﻿using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
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
using System.Runtime.Serialization;
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
using Monkeyspeak.Extensions;
using ICSharpCode.AvalonEdit.Search;
using Monkeyspeak.Editor.Syntax;

namespace Monkeyspeak.Editor.Controls
{
    /// <summary>
    /// Interaction logic for EditorControl.xaml
    /// </summary>
    [Serializable]
    public partial class EditorControl : MetroTabItem, IEditor, INotifyPropertyChanged, ISerializable
    {
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

        private MonkeyspeakEngine engine;
        private readonly Page page;

        private FileSystemWatcher fileWatcher;

        public event PropertyChangedEventHandler PropertyChanged;

        public EditorControl()
        {
            Logger.DebugEnabled = true;

            InitializeComponent();
            DataContext = this;

            //textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".ms");
            SearchPanel.Install(textEditor);
            SyntaxChecker.Install(this);

            textEditor.TextArea.Caret.PositionChanged += (sender, e) =>
                textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
            textEditor.TextChanged += (sender, args) =>
            {
                HasChanges = true;
                Plugins.Plugins.AllEnabled = true;
                Plugins.Plugins.OnEditorTextChanged(this);
            };
            textEditor.TextArea.SelectionChanged += (sender, args) =>
            {
                SelectedLine = Lines[textEditor.TextArea.Caret.Line - 1];
                SelectedText = textEditor.SelectedText;
                //if (!string.IsNullOrWhiteSpace(SelectedText))
                //HighlightAllOccurances(SelectedText);
                Plugins.Plugins.AllEnabled = true;
                Plugins.Plugins.OnEditorSelectionChanged(this);
            };
            textEditor.TextArea.TextEntered += (sender, e) =>
            {
                Intellisense.GenerateTriggerListCompletion(this);
                if (e.Text == " ")
                    SyntaxChecker.Check(this);
            };
            textEditor.TextArea.TextEntering += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Text))
                {
                    Intellisense.TextEntered(e);
                }
            };
            textEditor.KeyDown += (sender, e) =>
            {
                var settings = Properties.Settings.Default;

                if (e.Key == Key.Return)
                {
                    textEditor.TextArea.IndentationStrategy.IndentLines(textEditor.Document, 0, textEditor.LineCount);
                }
            };

            textEditor.PreviewMouseHover += (sender, e) =>
            {
                Intellisense.MouseHover(this, e);
            };
            textEditor.PreviewMouseMove += (sender, e) =>
            {
                Intellisense.MouseMove(this, e);
            };
            textEditor.Options.AllowScrollBelowDocument = true;
            textEditor.Options.HighlightCurrentLine = true;
            textEditor.Options.EnableImeSupport = true;
            textEditor.Options.EnableHyperlinks = true;
            textEditor.Options.CutCopyWholeLine = true;
            textEditor.Options.InheritWordWrapIndentation = false;
            textEditor.ShowLineNumbers = true;
            //textEditor.TextArea.IndentationStrategy = new MonkeyspeakIndentationStrategy(page);

            Visibility = Visibility.Visible;

            Title = "new";

            fileWatcher = new FileSystemWatcher
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess,
            };
            fileWatcher.Changed += FileWatcher_Raised;
            fileWatcher.Renamed += FileWatcher_Raised;
            fileWatcher.Deleted += FileWatcher_Raised;

            // set this as the active editor since it was new
            Editors.Instance.Selected = this;
            Keyboard.Focus(textEditor);
        }

        private void FileWatcher_Raised(object sender, FileSystemEventArgs e)
        {
            if (sender == this) return;
            if (e.FullPath != CurrentFilePath && e.ChangeType != WatcherChangeTypes.Renamed) return;
            var result = MessageDialogResult.Negative;
            fileWatcher.EnableRaisingEvents = false;
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    Dispatcher.Invoke(async () =>
                    {
                        result = await DialogManager.ShowMessageAsync(Application.Current.MainWindow as MetroWindow,
                                        "File Changed", $"The file {CurrentFilePath} was changed from a outside source, would you like to...?", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                                        new MetroDialogSettings { DefaultButtonFocus = MessageDialogResult.Affirmative, AffirmativeButtonText = "Load Changes", NegativeButtonText = "Ignore", FirstAuxiliaryButtonText = "Save As" });

                        if (result == MessageDialogResult.Affirmative)
                        {
                            await Reload();
                        }
                        else if (result == MessageDialogResult.FirstAuxiliary)
                        {
                            SaveAs();
                        }
                    }).Wait();
                    break;

                case WatcherChangeTypes.Renamed:
                    CurrentFilePath = e.FullPath;
                    break;

                case WatcherChangeTypes.Deleted:
                    Dispatcher.Invoke(async () =>
                    {
                        result = await DialogManager.ShowMessageAsync(Application.Current.MainWindow as MetroWindow,
                        "File Deleted", $"The file {CurrentFilePath} was deleted from a outside source, would you like to...?  The editor will remain open regardless of your choice so that you don't lose changes.", MessageDialogStyle.AffirmativeAndNegative,
                        new MetroDialogSettings { DefaultButtonFocus = MessageDialogResult.Affirmative, AffirmativeButtonText = "Save Changes", NegativeButtonText = "Ignore This" });

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

        public string HighlighterLanguage
        {
            get => textEditor.SyntaxHighlighting.Name;
            set
            {
                textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(value) ??
              HighlightingManager.Instance.GetDefinition("Monkeyspeak");
                if (textEditor.SyntaxHighlighting.Name == "Monkeyspeak")
                    textEditor.TextArea.IndentationStrategy = new MonkeyspeakIndentationStrategy(page);
            }
        }

        public int CaretLine
        {
            get => textEditor.TextArea.Caret.Line;
        }

        public bool HasChanges { get => _hasChanges; set => SetField(ref _hasChanges, value); }

        public void InsertAtCaretLine(string text)
        {
            var curLine = textEditor.Document.GetLineByOffset(textEditor.CaretOffset);
            int lineOffset = curLine.Offset;
            if (curLine.NextLine != null)
                lineOffset = curLine.NextLine.Offset;
            text = text.Replace("\n", string.Empty);
            BeginUndoGroup();
            textEditor.Document.Insert(lineOffset, text + "\n", AnchorMovementType.AfterInsertion);
            EndUndoGroup();
            textEditor.CaretOffset = lineOffset;
        }

        public void AddLine(string text)
        {
            text = text.Replace(Environment.NewLine, "\n");
            var lastLine = textEditor.Document.Lines[textEditor.LineCount - 1];
            BeginUndoGroup();
            textEditor.Document.Replace(lastLine.Offset, 0, text + "\n", OffsetChangeMappingType.KeepAnchorBeforeInsertion);
            EndUndoGroup();
            textEditor.CaretOffset = lastLine.Offset;
        }

        public void AddLine(string text, Color color)
        {
            var lastLine = textEditor.Document.Lines[textEditor.LineCount - 1];
            text = text.Replace(Environment.NewLine, "\n");
            BeginUndoGroup();
            textEditor.Document.Replace(lastLine.Offset, 0, text + "\n", OffsetChangeMappingType.KeepAnchorBeforeInsertion);
            SetTextColor(color, textEditor.LineCount, 0, text.Length);
            EndUndoGroup();
            textEditor.CaretOffset = lastLine.Offset;
        }

        /// <summary>
        /// Sets the text <seealso cref="Color"/> by navigating to the specified line and setting the color between the start and end position.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="color">Text color to set</param>
        public void SetTextColor(Color color, int line, int start, int end)
        {
            if (start < 0 || end < 0) return;
            BeginUndoGroup();
            textEditor.TextArea.TextView.LineTransformers.Add(new WordColorizer(color, line, start, end));
            EndUndoGroup();
            this.textEditor.TextArea.TextView.Redraw();
        }

        /// <summary>
        /// Sets the text <seealso cref="FontWeight"/> by navigating to the specified line and setting the color between the start and end position.
        /// </summary>
        /// <param name="weight">The weight.</param>
        /// <param name="line">The line.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public void SetTextWeight(FontWeight weight, int line, int start, int end)
        {
            if (start < 0 || end < 0) return;
            BeginUndoGroup();
            textEditor.TextArea.TextView.LineTransformers.Add(new FontWeightTransformer(weight, line, start, end));
            EndUndoGroup();
            this.textEditor.TextArea.TextView.Redraw();
        }

        private void HighlightAllOccurances(string text)
        {
            BeginUndoGroup();
            textEditor.TextArea.TextView.LineTransformers.Add(new HighlightSelectedColorizer(text));
            EndUndoGroup();
            this.textEditor.TextArea.TextView.Redraw();
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

        public int WordCount => textEditor.Text.Split(' ').Length;

        public string CurrentLine => Lines[textEditor.TextArea.Caret.Line - 1];

        public string SelectedText { get; private set; }

        /// <summary>
        /// Gets the selected line without the ending newline character.
        /// </summary>
        /// <value>
        /// The selected line.
        /// </value>
        public string SelectedLine { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is the active editor, the one with the editor open.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active editor; otherwise, <c>false</c>.
        /// </value>
        public bool IsActiveEditor { get => Editors.Instance.Selected == this; }

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

        public bool Open()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                RestoreDirectory = false, // opens dialog at last location used
                DefaultExt = ".ms",
                Filter = "Monkeyspeak Script|*.ms|Monkeyspeak Compiled Script|*.msx|All files (*.*)|*.*"
            };
            var opened = dlg.ShowDialog();
            if (opened ?? false)
            {
                CurrentFilePath = dlg.FileName;
                if (System.IO.Path.GetExtension(CurrentFilePath) == ".msx")
                {
                    var page = MonkeyspeakRunner.LoadCompiled(CurrentFilePath);
                    foreach (var trigger in page.Triggers)
                    {
                        AddLine(trigger.RebuildToString(page.Engine.Options));
                    }
                }
                else
                {
                    Plugins.Plugins.AllEnabled = false;
                    textEditor.Load(CurrentFilePath);
                    textEditor.Text = TextUtilities.NormalizeNewLines(textEditor.Text, "\n");
                    textEditor.SyntaxHighlighting =
                            HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(CurrentFilePath)) ??
                            HighlightingManager.Instance.GetDefinition("Monkeyspeak");
                }
                textEditor.Document.UndoStack.MarkAsOriginalFile();
            }
            return opened ?? false;
        }

        public void Save(bool forced = false)
        {
            fileWatcher.EnableRaisingEvents = false;
            if (CurrentFilePath == null)
            {
                SaveFileDialog dlg = new SaveFileDialog
                {
                    DefaultExt = ".ms",
                    AddExtension = true,
                    RestoreDirectory = false,
                    Filter = "Monkeyspeak Script |*.ms|All files (*.*)|*.*"
                };

                if (forced)
                {
                    var temp = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "backup_" + System.IO.Path.GetRandomFileName() + ".ms");
                    CurrentFilePath = temp;
                    textEditor.Save(CurrentFilePath);
                    return;
                }
                else if (dlg.ShowDialog() ?? false)
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
            Plugins.Plugins.AllEnabled = true;
            Plugins.Plugins.OnEditorSaveCompleted(this);
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
            Plugins.Plugins.AllEnabled = true;
            Plugins.Plugins.OnEditorSaveCompleted(this);
        }

        public async Task Reload()
        {
            if (HasChanges)
            {
                var result = await DialogManager.ShowMessageAsync((MetroWindow)Application.Current.MainWindow,
                    "Save?",
                    "Changes were detected.  Are you sure you don't want to save?", MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings { AffirmativeButtonText = "Save", NegativeButtonText = "Nah" });
                if (result == MessageDialogResult.Affirmative) Save();
                else if (result == MessageDialogResult.FirstAuxiliary) return;
            }

            if (System.IO.Path.GetExtension(CurrentFilePath) == ".msx")
            {
                textEditor.Text = string.Empty;
                var page = MonkeyspeakRunner.LoadCompiled(CurrentFilePath);
                foreach (var trigger in page.Triggers)
                {
                    AddLine(trigger.RebuildToString(page.Engine.Options));
                }
            }
            else
            {
                Plugins.Plugins.AllEnabled = false;
                textEditor.Load(CurrentFilePath);
                textEditor.Text = TextUtilities.NormalizeNewLines(textEditor.Text, "\n");
                textEditor.SyntaxHighlighting =
                        HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(CurrentFilePath)) ??
                        HighlightingManager.Instance.GetDefinition("Monkeyspeak");
            }
            HasChanges = false;
            Plugins.Plugins.AllEnabled = true;
        }

        public async Task<bool> CloseAsync()
        {
            if (HasChanges)
            {
                var result = await DialogManager.ShowMessageAsync((MetroWindow)Application.Current.MainWindow,
                    "Save?",
                    "Changes were detected.  Would you like to save before closing?", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                    new MetroDialogSettings { AffirmativeButtonText = "Save", NegativeButtonText = "Nah", FirstAuxiliaryButtonText = "Cancel" });
                if (result == MessageDialogResult.Affirmative) Save();
                else if (result == MessageDialogResult.FirstAuxiliary) return false;
            }

            Editors.Instance.Remove(this);
            return true;
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
                    propertyGrid.SelectedObject = Plugins.Plugins.All;
                    break;

                case 4:
                    propertyGrid.SelectedObject = MonkeyspeakRunner.Options;
                    break;
            }
        }

        private void GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Editors.Instance.Selected == this) return;
            Editors.Instance.Selected = this;
        }

        private void OnPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Intellisense.Close();
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (Editors.Instance.Selected == this) return;
            Editors.Instance.Selected = this;
            Keyboard.Focus(textEditor);
            Mouse.Capture(textEditor);
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            Intellisense.Close();
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            Intellisense.Close();
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

        private void BeginUndoGroup()
        {
            textEditor.Document.UndoStack.StartUndoGroup();
        }

        private void EndUndoGroup()
        {
            textEditor.Document.UndoStack.EndUndoGroup();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Undo)
            {
                HasChanges = !textEditor.Document.UndoStack.IsOriginalFile;
            }
        }

        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                if (Editors.Instance.Selected == this) return;
                Editors.Instance.Selected = this;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CurrentFilePath", CurrentFilePath);
            //info.AddValue("Content", textEditor.Text);
        }
    }
}