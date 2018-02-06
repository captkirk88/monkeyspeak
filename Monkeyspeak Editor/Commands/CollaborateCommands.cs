using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Collaborate;

namespace Monkeyspeak.Editor.Commands
{
    internal sealed class CollaborateCreateCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            //var editor = Editors.Instance.Selected;
            //CollaborationManager.Create(editor);

            //CustomDialog dialog = new CustomDialog(Application.Current.MainWindow as MetroWindow);
            //dialog.Title = "Share this code with others to allow collaboration";
            //StackPanel content = new StackPanel();

            //TextBox codeBox = new TextBox();
            //codeBox.IsEnabled = false;
            //codeBox.Text = code;

            //Button copyButton = new Button() { Content = "Copy" };
            //copyButton.Click += (sender, e) => Clipboard.SetText(code, TextDataFormat.Text);
            //content.Children.Add(codeBox);
            //content.Children.Add(copyButton);
            //dialog.Content = content;
            //dialog.Unloaded += async (sender, e) => await dialog.RequestCloseAsync();
            //dialog.ShowModalDialogExternally();

            throw new NotImplementedException("patience!");
        }
    }

    internal sealed class CollaborateOpenCommand : BaseCommand
    {
        public override void Execute(object parameter)
        {
            throw new NotImplementedException("patience!");
            CustomDialog dialog = new CustomDialog(Application.Current.MainWindow as MetroWindow);
            dialog.Title = "Paste the collaboration code here";
            StackPanel content = new StackPanel();

            TextBox codeBox = new TextBox();

            Button openButton = new Button() { Content = "Open" };
            openButton.Click += (sender, e) =>
            {
                var code = codeBox.Text;
                if (!string.IsNullOrWhiteSpace(code))
                {
                    var editor = Editors.Instance.Add();
                    if (!CollaborationManager.Open(editor, code))
                    {
                        editor.Close();
                    }
                }
            };
            content.Children.Add(codeBox);
            content.Children.Add(openButton);
            dialog.Content = content;
            dialog.Unloaded += async (sender, e) => await dialog.RequestCloseAsync();
            dialog.ShowModalDialogExternally();
        }
    }
}