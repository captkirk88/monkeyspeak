using ICSharpCode.AvalonEdit.Highlighting;
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
                var stackPanel = new StackPanel();
                var tb = new TextBlock
                {
                    Text = "Thank you for using the Monkeyspeak Editor!",
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                };
                var associateExtButton = new Button()
                {
                    Content = "Register .ms To This"
                };
                associateExtButton.Click += (sender, args) => EnsureFileAssociations();
                stackPanel.Children.Add(tb);
                stackPanel.Children.Add(associateExtButton);
                return stackPanel;
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