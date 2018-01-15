using Monkeyspeak.Libraries;
using Monkeyspeak.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        public event Action<string, string, string> TriggerSelected;

        public TriggerList()

        {
            InitializeComponent();
            TriggerDescriptions = new ObservableCollection<Tuple<string, string, string>>();
            this.DataContext = this;
            trigger_view.ItemsSource = TriggerDescriptions;
        }

        public void Add(Page page, Trigger trigger, TriggerHandler handler, BaseLibrary lib)
        {
            var triggerDescriptions = ReflectionHelper.GetAllAttributesFromMethod<TriggerDescriptionAttribute>(handler.Method);
            var pair = new Tuple<string, string, string>(lib.ToString(trigger), lib.GetType().Name,
                triggerDescriptions.FirstOrDefault()?.Description ?? string.Empty);
            if (!TriggerDescriptions.Contains(pair))
                TriggerDescriptions.Add(pair);
        }

        public TriggerCategory TriggerCategory { get; set; }
        public ObservableCollection<Tuple<string, string, string>> TriggerDescriptions { get; set; }

        private void Content_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = trigger_view.SelectedItem as Tuple<string, string, string>;
            if (item != null)
            {
                TriggerSelected?.Invoke(item.Item1, item.Item3, item.Item2);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (var lib in MonkeyspeakRunner.CurrentPage.Libraries)
                {
                    foreach (var kv in lib.Handlers.Where(h => h.Key.Category == this.TriggerCategory))
                    {
                        Add(page, kv.Key, kv.Value, lib);
                    }
                }
            });
        }

        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private GridViewColumnHeader lastHeaderClicked = null;

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            ListSortDirection direction;
            GridViewColumnHeader colHeader = (GridViewColumnHeader)e.OriginalSource;
            if (colHeader.Role == GridViewColumnHeaderRole.Padding) return;
            if (colHeader != lastHeaderClicked)
            {
                direction = ListSortDirection.Ascending;
            }
            else
            {
                if (_lastDirection == ListSortDirection.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }
                else direction = ListSortDirection.Ascending;
            }

            string colName = colHeader.Content.ToString();

            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(trigger_view.ItemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(colName, direction));
            view.Refresh();
            lastHeaderClicked = colHeader;
            _lastDirection = direction;
        }
    }
}