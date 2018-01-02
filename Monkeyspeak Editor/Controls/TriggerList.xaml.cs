using Monkeyspeak.Editor.HelperClasses;
using Monkeyspeak.Libraries;
using Monkeyspeak.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using System.Linq;

namespace Monkeyspeak.Editor.Controls
{
    /// <summary>
    /// Interaction logic for TriggerList.xaml
    /// </summary>
    public partial class TriggerList : UserControl
    {
        private static MonkeyspeakEngine engine = null;
        private static Page page = null;

        public event Action<KeyValuePair<string, string>> TriggerSelected;

        public TriggerList()
        {
            InitializeComponent();
            TriggerDescriptions = new ObservableCollection<KeyValuePair<string, string>>();
            this.DataContext = this;
        }

        public void Add(Page page, Trigger trigger, BaseLibrary lib)
        {
            var pair = new KeyValuePair<string, string>(page.GetTriggerDescription(trigger, true), lib.GetType().Name);
            if (!TriggerDescriptions.Contains(pair))
                TriggerDescriptions.Add(pair);
        }

        public TriggerCategory TriggerCategory { get; set; }
        public ObservableCollection<KeyValuePair<string, string>> TriggerDescriptions { get; set; }

        private void Content_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (KeyValuePair<string, string>)Content.SelectedItem;
            TriggerSelected?.Invoke(item);
            Logger.Debug<TriggerList>(item);
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