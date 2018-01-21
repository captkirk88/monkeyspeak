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
        private static string gitOwner, gitRepo;
        private static GitHubClient github = null;

        public static GitHubClient Initialize(string owner, string repo)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new ArgumentException("message", nameof(owner));
            }

            if (string.IsNullOrWhiteSpace(repo))
            {
                throw new ArgumentException("message", nameof(repo));
            }
            gitOwner = owner;
            gitRepo = repo;

            var github = new GitHubClient(new ProductHeaderValue(repo));
            return github;
        }

        public static async Task SubmitIssue(Exception ex = null)
        {
            if (ex == null || github == null) return;
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
                var existingClosed = await github.Issue.GetAllForRepository(gitOwner, gitRepo, new RepositoryIssueRequest { State = ItemStateFilter.Closed });
                var existingOpen = await github.Issue.GetAllForRepository(gitOwner, gitRepo, apiOpts);
                var closed = existingClosed.FirstOrDefault(issue => issue.Title.Equals(title));

                // If a previous issue with the same exception type was open
                // reopen the issue with a comment of the log.
                if (closed != null)
                {
                    sb.AppendLine($"Updated on {DateTime.UtcNow}");
                    var reattacked = closed.ToUpdate();
                    reattacked.State = ItemState.Open;
                    reattacked.AddAssignee(gitOwner);
                    var reopen = await github.Issue.Update(gitOwner, gitRepo, closed.Number, reattacked);
                    await github.Issue.Comment.Create(gitOwner, gitRepo, reopen.Number, sb.ToString());
                    return;
                }

                var open = existingOpen.FirstOrDefault(issue => issue.Title.Equals(title));
                if (open != null) sb.AppendLine($"Updated on {DateTime.UtcNow}");

                if (open == null)
                {
                    var newIssue = new NewIssue(title)
                    {
                        Body = sb.ToString()
                    };
                    newIssue.Labels.Add("auto-generated");
                    newIssue.Assignees.Add(gitOwner);
                    await github.Issue.Create(gitOwner, gitRepo, newIssue);
                }
                else
                {
                    var updateExistingIssue = open.ToUpdate();
                    updateExistingIssue.Body = sb.ToString();
                    updateExistingIssue.AddAssignee(gitOwner);
                    await github.Issue.Update(gitOwner, gitRepo, open.Number, updateExistingIssue);
                }
            }
            catch (Exception e) { e.Log<Github>(); }
        }

        public static async Task<Release> GetLatestRelease()
        {
            if (github == null) return default(Release);
            try
            {
                return await github.Repository.Release.GetLatest(gitOwner, gitRepo);
            }
            catch (Exception ex)
            {
                ex.Log<Github>();
            }
            return null;
        }
    }
}