using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Forms;

namespace Monkeyspeak.Editor
{
    internal class MainWindow : Form
    {
        private Splitter container;
        private TabControl triggerTabs;
        private TabControl documentTabs;

        private MenuBar mainMenu;
        private Menu fileMenu;
        private Menu buildMenu;

        public MainWindow()
        {
        }

        public MainWindow(IHandler handler) : base(handler)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Title = "Monkeyspeak Editor";
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.Default;
            this.Size = new Eto.Drawing.Size(800, 600);
            container = new Splitter
            {
                Orientation = Orientation.Vertical,
                Visible = true
            };
            documentTabs = new TabControl();
            documentTabs.Size = new Eto.Drawing.Size(-1, -1);

            var editBox = new CodeTextBox() { Size = new Eto.Drawing.Size(-1, -1) };

            documentTabs.Pages.Add(new TabPage(new TextBox() { Size = new Eto.Drawing.Size(-1, -1) }));
            triggerTabs = new TabControl();
            triggerTabs.Size = new Eto.Drawing.Size(-1, -1);
            documentTabs.Pages.Add(new TabPage(new ListBox() { Size = new Eto.Drawing.Size(-1, -1) }));
            container.Panel1 = documentTabs;
            container.Panel2 = triggerTabs;
            container.Size = new Eto.Drawing.Size(-1, -1);
            this.Content = container;
        }
    }
}