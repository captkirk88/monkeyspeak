using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monkeyspeak.Editor.Controls;
using Octokit;
using Monkeyspeak.Extensions;
using System.IO;
using Octokit.Internal;

namespace Monkeyspeak.Editor.HelperClasses
{
    public sealed class GithubIssueTracker
    {
        private GitHubClient github;

        public GithubIssueTracker()
        {
            Uri uri = new Uri("https://github.com/captkirk88/monkeyspeak");
            github = new GitHubClient(new ProductHeaderValue("monkeyspeak"), uri);
        }

        public async Task SubmitIssue(string title, Exception ex = null)
        {
            bool canCreate = !string.IsNullOrEmpty(title) && ex != null;
            if (!canCreate) return;
            var issue = new NewIssue(title);
            var sb = new StringBuilder()
                .AppendLine(ex.Flatten())
                .AppendLine("Log:")
                .AppendLine(ConsoleWindow.Current.Text);
            issue.Body = sb.ToString();
            await github.Issue.Create("captkirk88", "monkeyspeak", issue).
                ContinueWith(created => { if (created.Exception != null) created.Exception.Log(); }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}