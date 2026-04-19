using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Chimp.Common;
using static Chimp.Shell.Localization;

namespace Chimp.Shell;

internal class UpdateChecker(PersistablePropertyBag stateBag)
{
    internal record GitHubRelease([property: JsonPropertyName("tag_name")] string TagName);

    internal record GithubReleaseVersion(DateTimeOffset LastChecked, string? LatestReleaseTag);

    private const string ReleasesUrl = "https://github.com/realbarend/chimpcli/releases";
    private const string ApiUrl = "https://api.github.com/repos/realbarend/chimpcli/releases/latest";
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(4);

    public async Task CheckAndNotify()
    {
        if (Program.Version.ToString(3) == "0.0.0") return;

        var ghRelease = stateBag.Get<GithubReleaseVersion>();

        if (ShouldCheckGithubReleaseVersion(ghRelease))
        {
            var updatedGhRelease = await TryFetchLatestGithubReleaseVersion();
            if (updatedGhRelease is not null)
            {
                ghRelease = updatedGhRelease;
                stateBag.Set(ghRelease);
            }
        }

        if (IsNewerVersionAvailable(ghRelease))
        {
            Console.WriteLine();
            WriteLocalized("A newer version of TimeChimp CLI is available ({LatestVersion}): {Url}", ghRelease.LatestReleaseTag, ReleasesUrl);
        }
    }

    private static async Task<GithubReleaseVersion?> TryFetchLatestGithubReleaseVersion()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("chimpcli",
                Program.Version.ToString(3)));
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var json = await client.GetStringAsync(ApiUrl, cts.Token);
            var latestReleaseTag = JsonContext.Deserialize<GitHubRelease>(json)?.TagName;

            return new GithubReleaseVersion(DateTimeOffset.UtcNow, latestReleaseTag);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private static bool ShouldCheckGithubReleaseVersion(GithubReleaseVersion? ghRelease)
    {
        return ! (DateTimeOffset.UtcNow - ghRelease?.LastChecked < CheckInterval);
    }

    private static bool IsNewerVersionAvailable([NotNullWhen(true)] GithubReleaseVersion? ghRelease)
    {
        if (ghRelease?.LatestReleaseTag is null) return false;

        if (!Version.TryParse(ghRelease.LatestReleaseTag.TrimStart('v').TrimEnd("-self-contained"), out var latestVersion)) return false;

        return latestVersion > Program.Version;
    }
}
