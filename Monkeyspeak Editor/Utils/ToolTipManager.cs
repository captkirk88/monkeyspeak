using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Monkeyspeak.Editor.Utils
{
    public static class ToolTipManager
    {
        private static ToolTip tooltip;
        private static StackPanel container;

        static ToolTipManager()
        {
            tooltip = new ToolTip();
            container = new StackPanel();
            tooltip.Content = container;
        }

        public static UIElement Target
        {
            get => tooltip.PlacementTarget;
            set => tooltip.PlacementTarget = value;
        }

        public static void Add(object content)
        {
            if (content is UIElement element)
            {
                if (element != null)
                {
                    container.Children.Add(element);
                    Opened = true;
                }
            }
            else
            {
                container.Children.Add(new Label { Content = content });
                Opened = true;
            }
        }

        public static void Clear()
        {
            container.Children.Clear();
        }

        public static bool Opened
        {
            get => tooltip.IsOpen;
            set => tooltip.IsOpen = value;
        }
    }
}