using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Monkeyspeak.Editor.Interfaces.Notifications;
using Octokit;

namespace Monkeyspeak.Editor.Notifications
{
    internal class NeedUpdateNotification : INotification
    {
        private TextBlock content;

        public static async Task<bool> Check()
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Uri uri = new Uri("https://github.com/captkirk88/monkeyspeak");
            var web = new WebClient();
            var github = new GitHubClient(new ProductHeaderValue("Monkeyspeak Editor"), uri);
            var release = await github.Repository.Release.GetLatest("captkirk88", "monkeyspeak");
            if (new Version(release.Body) > currentVersion)
            {
                foreach (var asset in release.Assets)
                {
                    if (asset.Name.Contains("Editor") && asset.Name.Contains("Binaries"))
                    {
                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            var result = await DialogManager.ShowMessageAsync(System.Windows.Application.Current.MainWindow as MetroWindow,
                            "Update Found!", $"A update was found, would you like to download the latest version?", MessageDialogStyle.AffirmativeAndNegative,
                            new MetroDialogSettings { DefaultButtonFocus = MessageDialogResult.Affirmative, AffirmativeButtonText = "Yes!", NegativeButtonText = "No" });

                            if (result == MessageDialogResult.Affirmative)
                            {
                                Process.Start(asset.BrowserDownloadUrl);
                            }
                        });
                        return true;
                    }
                }
            }
            return false;
        }

        public NeedUpdateNotification()
        {
            this.content = new TextBlock
            {
                Text = "Update found!",
                TextWrapping = System.Windows.TextWrapping.WrapWithOverflow,
            };
        }

        public object Content => content;
    }
}