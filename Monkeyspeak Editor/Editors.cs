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
    public static class Editors
    {
        private static List<EditorControl> s_all;

        public static event Action<EditorControl> Added;

        public static event Action<EditorControl> Removed;

        static Editors()
        {
            s_all = new List<EditorControl>();
            Add();
        }

        public static IReadOnlyCollection<EditorControl> All { get => s_all; }

        public static void Add(string title = null)
        {
            if (string.IsNullOrEmpty(title)) title = $"new {s_all.Count}";
            var @new = new EditorControl { Title = title };
            s_all.Add(@new);
            Added?.Invoke(@new);
        }

        public static void Remove(EditorControl control)
        {
            s_all.Remove(control);
            Removed?.Invoke(control);

            if (s_all.Count == 0) Add();
        }
    }
}