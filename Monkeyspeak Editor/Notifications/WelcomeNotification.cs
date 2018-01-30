using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro.Controls;
using Monkeyspeak.Editor.Commands;
using Monkeyspeak.Editor.HelperClasses;
using Monkeyspeak.Editor.Interfaces.Notifications;
using Monkeyspeak.Editor.Keybindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Notifications
{
    public class WelcomeNotification : TimedNotification
    {
        public WelcomeNotification(INotificationManager manager) : base(manager, TimeSpan.FromSeconds(20))
        {
        }

        public override object SetContent()
        {
            var scroll = new ScrollViewer();
            var stackPanel = new StackPanel();

            StringBuilder welcome = new StringBuilder();
            welcome.AppendLine("Thank you for using the Monkeyspeak Editor!")
                .AppendLine("Did you know?")
                .AppendLine("- Right click on a tab for shortcuts.")
                .AppendLine($"- {HotkeyManager.GetHotkey<CompletionCommand>()} for intellisense.")
                .AppendLine($"- {HotkeyManager.GetHotkey<SaveCommand>()} to save.")
                .AppendLine($"- {HotkeyManager.GetHotkey<NewEditorCommand>()} for new document.")
                .AppendLine($"- {HotkeyManager.GetHotkey<CloseCurrentEditorCommand>()} close current document.")
                .AppendLine($"- Ctrl+A will select all in the document and in the error list.")
                .AppendLine("- You can type words to filter triggers.");
            var tb = new TextBlock
            {
                Text = welcome.ToString(),
                TextWrapping = System.Windows.TextWrapping.Wrap,
            };
            var associateExtButton = new Button()
            {
                Content = "Register .ms extension"
            };
            associateExtButton.Click += (sender, args) => EnsureFileAssociations();
            stackPanel.Children.Add(tb);
            stackPanel.Children.Add(associateExtButton);
            scroll.Content = stackPanel;
            return scroll;
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