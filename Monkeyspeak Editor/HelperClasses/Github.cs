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
using System.Net;
using Monkeyspeak.Logging;

using Monkeyspeak.Extensions;

using System.Configuration;
using System.Collections.Specialized;

namespace Monkeyspeak.Editor.HelperClasses
{
    public class Github
    {
        private static readonly string owner = "captkirk88";
        private static readonly string repo = "monkeyspeak";

        private static GitHubClient github = Initialize();

        public static GitHubClient Initialize()
        {
            var github = new GitHubClient(new ProductHeaderValue(repo));
            return github;
        }

        public static async Task SubmitIssue(Exception ex = null)
        {
            if (ex == null) return;
            try
            {
                var title = $"{ex.GetType().Name}";
                var sb = new StringBuilder()
                    .AppendLine(ex.Flatten())
                    .AppendLine("**Log:**")
                    .AppendLine(ConsoleWindow.Current?.Text);

                var apiOpts = new ApiOptions()
                {
                    PageSize = 100
                };
                var existingIssues = await github.Issue.GetAllForRepository(owner, repo, apiOpts);
                var existingIssue = existingIssues.FirstOrDefault(issue => issue.Title.Equals(title));
                if (existingIssue != null) sb.AppendLine($"Updated on {DateTime.UtcNow}");

                if (existingIssue == null)
                {
                    var newIssue = new NewIssue(title)
                    {
                        Body = sb.ToString()
                    };
                    newIssue.Labels.Add("auto-generated");
                    await github.Issue.Create(owner, repo, newIssue);
                }
                else
                {
                    var updateExistingIssue = existingIssue.ToUpdate();
                    updateExistingIssue.Body = sb.ToString();
                    await github.Issue.Update(owner, repo, existingIssue.Number, updateExistingIssue);
                }
            }
            catch (Exception e) { e.Log<Github>(); }
        }

        public static async Task<Release> GetLatestRelease()
        {
            try
            {
                return await github.Repository.Release.GetLatest(owner, repo);
            }
            catch (Exception ex)
            {
                ex.Log<Github>();
            }
            return null;
        }
    }
}