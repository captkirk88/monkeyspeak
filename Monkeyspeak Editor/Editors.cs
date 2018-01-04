using Monkeyspeak.Editor.Controls;
using Monkeyspeak.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Monkeyspeak.Editor
{
    public class Editors : INotifyPropertyChanged
    {
        public static Editors Instance = new Editors();
        private ObservableCollection<EditorControl> s_all;
        private int docCount = 0;

        public event Action<EditorControl> Added;

        public event Action<EditorControl> Removed;

        public event PropertyChangedEventHandler PropertyChanged;

        public Editors()
        {
            s_all = new ObservableCollection<EditorControl>();
            s_all.CollectionChanged += S_all_CollectionChanged;
        }

        private void S_all_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("All");
        }

        public EditorControl Selected { get; set; }

        public ObservableCollection<EditorControl> All { get => s_all; set => SetField(ref s_all, value); }

        public bool IsEmpty => s_all.Count == 0;

        public bool AnyHasChanges => s_all.Any(editor => editor.HasChanges);

        public EditorControl Add(string filePath = null)
        {
            var editor = new EditorControl();
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                editor.CurrentFilePath = filePath;
                editor.Open();
            }
            else editor.Title = $"new {(docCount == 0 ? "" : docCount.ToString())}";
            //editor.GotKeyboardFocus += (sender, args) => Selected = (EditorControl)sender;
            //editor.GotFocus += (sender, args) => Selected = (EditorControl)sender;
            All.Add(editor);
            docCount++;
            Added?.Invoke(editor);
            return editor;
        }

        public void ForceUpdateAll()
        {
            foreach (var editor in All) Added?.Invoke(editor);
        }

        public void Remove(EditorControl control)
        {
            if (All.Remove(control))
            {
                Removed?.Invoke(control);
            }
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
    }
}