using Monkeyspeak.Libraries;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Monkeyspeak.Editor.Controls
{
    /// <summary>
    /// Interaction logic for TriggerList.xaml
    /// </summary>
    public partial class TriggerList : UserControl
    {
        private static MonkeyspeakEngine engine = null;
        private static Page page = null;

        public event Action<string, string> TriggerSelected;

        public TriggerList()

        {
            InitializeComponent();
            TriggerDescriptions = new ObservableCollection<Tuple<string, string>>();
            this.DataContext = this;
            trigger_view.ItemsSource = TriggerDescriptions;
        }

        public void Add(Page page, Trigger trigger, BaseLibrary lib)
        {
            var pair = new Tuple<string, string>(page.GetTriggerDescription(trigger, true), lib.GetType().Name);
            if (!TriggerDescriptions.Contains(pair))
                TriggerDescriptions.Add(pair);
        }

        public TriggerCategory TriggerCategory { get; set; }
        public ObservableCollection<Tuple<string, string>> TriggerDescriptions { get; set; }

        private void Content_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = trigger_view.SelectedItem as Tuple<string, string>;
            if (item != null)
            {
                TriggerSelected?.Invoke(item.Item1, item.Item2);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (engine == null)
            {
                var opts = new Options
                {
                    CanOverrideTriggerHandlers = false,
                    TriggerLimit = 100000
                };
                engine = new MonkeyspeakEngine(opts);
                page = new Page(engine);
                page.LoadAllLibraries();
            }

            this.Dispatcher.Invoke(() =>
            {
                foreach (var lib in page.Libraries)
                {
                    foreach (var trigger in lib.Handlers.Where(h => h.Key.Category == this.TriggerCategory).Select(kv => kv.Key))
                    {
                        Add(page, trigger, lib);
                    }
                }
            });
        }
    }
}