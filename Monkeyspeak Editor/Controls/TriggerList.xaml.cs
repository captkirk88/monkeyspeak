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
        public event Action<TriggerCompletionData> TriggerSelected;

        public TriggerList()

        {
            InitializeComponent();
            this.DataContext = this;
            trigger_view.SelectionMode = SelectionMode.Single;
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
            if (Intellisense.TriggerCompletions.Count == 0)
                Intellisense.GenerateTriggerListCompletion(Editors.Instance.Selected);
            foreach (var item in Intellisense.TriggerCompletions.Where(data => data.Trigger.Category == TriggerCategory))
            {
                if (item.IsValid)
                    trigger_view.Items.Add(item);
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