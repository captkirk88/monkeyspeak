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

namespace Monkeyspeak.Editor.Controls
{
    /// <summary>
    /// Interaction logic for TriggerList.xaml
    /// </summary>
    public partial class TriggerList : UserControl
    {
        public event Action<KeyValuePair<string, string>> SelectionChanged;

        public TriggerList()
        {
            InitializeComponent();
            TriggerDescriptions = new ObservableCollection<KeyValuePair<string, string>>();
            this.DataContext = this;

            this.Dispatcher.InvokeAsync(() =>
            {
                var opts = new Options
                {
                    CanOverrideTriggerHandlers = false,
                    TriggerLimit = 100000
                };
                var engine = new MonkeyspeakEngine(opts);
                using (var page = new Page(engine))
                {
                    page.LoadAllLibraries();
                    // causes
                    foreach (var lib in page.Libraries)
                    {
                        Logger.Debug(lib.GetType().Name);
                        foreach (var cause in lib.Handlers.Where(h => h.Key.Category == this.TriggerCategory).Select(kv => kv.Key))
                        {
                            Add(page, cause, lib);
                        }
                    }
                }
            });
        }

        public void Add(Page page, Trigger trigger, BaseLibrary lib)
        {
            TriggerDescriptions.Add(new KeyValuePair<string, string>(page.GetTriggerDescription(trigger, true), lib.GetType().Name));
        }

        public TriggerCategory TriggerCategory { get; set; }
        public ObservableCollection<KeyValuePair<string, string>> TriggerDescriptions { get; set; }

        private void Content_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (KeyValuePair<string, string>)Content.SelectedItem;
            SelectionChanged?.Invoke(item);
            Logger.Debug<TriggerList>(item);
        }
    }
}