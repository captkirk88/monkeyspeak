using Monkeyspeak.Editor.HelperClasses;
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
        private static Page page = MonkeyspeakRunner.CurrentPage;

        private ToolTip selectedToolTip;

        public event Action<TriggerCompletionData> TriggerSelected;

        private ObservableCollection<TriggerCompletionData> Triggers = new ObservableCollection<TriggerCompletionData>();

        public TriggerList()

        {
            InitializeComponent();
            this.DataContext = this;
            trigger_view.SelectionMode = SelectionMode.Single;
            trigger_view.ItemsSource = Triggers;
        }

        public void Add(TriggerCompletionData data)
        {
            Triggers.Add(data);
        }

        public TriggerCategory TriggerCategory { get; set; }

        private void Content_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = trigger_view.SelectedItem as TriggerCompletionData;
            if (item != null)
            {
                TriggerSelected?.Invoke(item);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var lib in MonkeyspeakRunner.CurrentPage.Libraries.OrderByDescending(l => l.GetType().Name))
            {
                foreach (var kv in lib.Handlers.Where(kv => kv.Key.Category == TriggerCategory).OrderBy(kv => kv.Key.Id))
                {
                    Add(new TriggerCompletionData(MonkeyspeakRunner.CurrentPage, lib, kv.Key));
                }
            }
        }

        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private GridViewColumnHeader lastHeaderClicked = null;

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            return; // broken as of right now
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