using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro.Controls;
using Monkeyspeak.Editor.Interfaces.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Notifications
{
    public class WelcomeNotification : INotification
    {
        public object Content
        {
            get
            {
                var scroll = new ScrollViewer();
                var stackPanel = new StackPanel();

                StringBuilder welcome = new StringBuilder();
                welcome.AppendLine("Thank you for using the Monkeyspeak Editor!")
                    .AppendLine("Did you know?")
                    .AppendLine("- Right click on a tab for shortcuts.")
                    .AppendLine("- Ctrl+Space for intellisense.")
                    .AppendLine("- Ctrl+S to save")
                    .AppendLine("- Ctrl+N for new document")
                    .AppendLine("- Ctrl+X close current document");
                var tb = new TextBlock
                {
                    Text = welcome.ToString(),
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                };
                var associateExtButton = new Button()
                {
                    Content = "Register .ms extension To This"
                };
                associateExtButton.Click += (sender, args) => EnsureFileAssociations();
                stackPanel.Children.Add(tb);
                stackPanel.Children.Add(associateExtButton);
                scroll.Content = stackPanel;
                return scroll;
            }
        }

        public void EnsureFileAssociations()
        {
            var def = HighlightingManager.Instance.GetDefinition("Monkeyspeak");
            HelperClasses.FileAssociations.EnsureAssociationsSet(new HelperClasses.FileAssociation
            {
                Extension = ".ms",
                FileTypeDescription = "Monkeyspeak Script",
                ProgId = "Monkeyspeak_Editor"
            });
        }
    }
}