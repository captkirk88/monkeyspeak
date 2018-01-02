using Monkeyspeak.Editor.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor
{
    public class Editors : INotifyPropertyChanged
    {
        public static Editors Instance = new Editors();
        private ObservableCollection<EditorControl> s_all;

        public event Action<EditorControl> Added;

        public event Action<EditorControl> Removed;

        public event PropertyChangedEventHandler PropertyChanged;

        public Editors()
        {
            s_all = new ObservableCollection<EditorControl>();
            s_all.CollectionChanged += S_all_CollectionChanged;
            Add();
        }

        private void S_all_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("All");
        }

        public ObservableCollection<EditorControl> All { get => s_all; set => SetField(ref s_all, value); }

        public bool IsEmpty => s_all.Count == 0;

        public bool AnyHasChanges => s_all.Any(editor => editor.HasChanges);

        public void Add(string title = null)
        {
            if (string.IsNullOrEmpty(title)) title = $"new {s_all.Count}";
            var @new = new EditorControl { Title = title };
            All.Add(@new);
            Added?.Invoke(@new);
        }

        public void Remove(EditorControl control)
        {
            if (All.Remove(control))
            {
                Removed?.Invoke(control);
            }
            if (s_all.Count == 0) Add();
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