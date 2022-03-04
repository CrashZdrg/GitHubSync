# GitHubSync
This acts like a launcher that downloads the latest release asset from a GitHub repository, extract it over a folder and then (optionally) run a executable.

Powered by [Octokit](https://github.com/octokit/octokit.net) and [7-Zip](https://www.7-zip.org/).

Only for Windows currently.

---
Run it once to generate an empty config file that you should edit like this:

| Property | Description |
|----------|-------------|
| RepositoryOwner | The owner of the repository. e.g.: CrashZdrg. |
| RepositoryName | The name of the repository. e.g.: GitHubSync. |
| AssetNamePattern | A RegEx pattern that match the asset name. Empty to get the first asset. |
| OnlyReleases | `true` for Releases only. `false` to include PreReleases, etc. |
| OutputDirectory | Path to extract the asset. |
| Executable | Full or relative path to an executable to run. Empty to disable. |

---
To force an Update, delete the Data.json file.
