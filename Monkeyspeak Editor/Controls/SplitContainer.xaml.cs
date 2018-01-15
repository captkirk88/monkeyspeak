using System;
using System.Collections.Generic;
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
    /// Interaction logic for SplitContainer.xaml
    /// </summary>
    public partial class SplitContainer : UserControl
    {
        public SplitContainer()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SplitContainer), new PropertyMetadata(Orientation.Horizontal));

        public UIElement FirstChild
        {
            get { return (UIElement)GetValue(FirstChildProperty); }
            set { SetValue(FirstChildProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstChildProperty =
            DependencyProperty.Register("FirstChild", typeof(UIElement), typeof(SplitContainer), new PropertyMetadata(null));

        public UIElement SecondChild
        {
            get { return (UIElement)GetValue(SecondChildProperty); }
            set { SetValue(SecondChildProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondChildProperty =
            DependencyProperty.Register("SecondChild", typeof(UIElement), typeof(SplitContainer), new PropertyMetadata(null));
    }
}