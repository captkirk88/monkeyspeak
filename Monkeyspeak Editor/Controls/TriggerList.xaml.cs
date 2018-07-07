using ICSharpCode.AvalonEdit.CodeCompletion;
using Monkeyspeak.Editor.HelperClasses;
using Monkeyspeak.Editor.Syntax;
using Monkeyspeak.Extensions;
using Monkeyspeak.Libraries;
using Monkeyspeak.Utils;
using System;
using System.Collections.Generic;
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

        private List<ICompletionData> triggers = new List<ICompletionData>();

        public TriggerList()

        {
            InitializeComponent();
            this.DataContext = this;
            trigger_view.SelectionMode = SelectionMode.Single;

            searchBox.TextChanged += SearchBox_TextChanged;
            searchBox.KeyUp += SearchBox_KeyUp;
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            triggers.Clear();
            trigger_view.Items.Clear();
            if (searchBox.Text.IsNullOrBlank())
                triggers.AddRange(Intellisense.GetTriggerCompletionData().Where(data => data is TriggerCompletionData d && d.Trigger.Category == TriggerCategory && d.IsValid));
            else
                triggers.AddRange(Intellisense.GetTriggerCompletionData().Where(data => (data is TriggerCompletionData d) && d.Trigger.Category == TriggerCategory && d.IsValid && (d.Text.IndexOf(searchBox.Text, StringComparison.InvariantCultureIgnoreCase) >= 0 || d.Text.CompareTo(searchBox.Text) == 0)));
            foreach (var trigger in triggers)
                trigger_view.Items.Add(trigger);
        }

        public TriggerCategory TriggerCategory
        {
            get => _triggerCategory;
            set
            {
                _triggerCategory = value;
                triggers.Clear();
                trigger_view.Items.Clear();
                triggers.AddRange(Intellisense.GetTriggerCompletionData().Where(data => (data is TriggerCompletionData d) && d.Trigger.Category == TriggerCategory && d.IsValid));
                foreach (var trigger in triggers)
                    trigger_view.Items.Add(trigger);
            }
        }

        private void Content_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (trigger_view.SelectedItem is TriggerCompletionData item)
            {
                TriggerSelected?.Invoke(item);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private GridViewColumnHeader lastHeaderClicked = null;
        private TriggerCategory _triggerCategory;

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