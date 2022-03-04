using GitHubSync.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubSync
{
    class Api
    {
        private Octokit.Internal.HttpClientAdapter? httpClientAdapter;
        protected Octokit.Internal.HttpClientAdapter? HttpClientAdapter => httpClientAdapter ??= GetHttpClientAdapterFromConnection((Octokit.Connection)_connection.Connection);

        private System.Net.Http.HttpClient? httpClient;
        protected System.Net.Http.HttpClient? HttpClient => httpClient ??= GetHttpClientFromAdapter(HttpClientAdapter);

        readonly Octokit.ApiConnection _connection;

        readonly string _repositoryOwner;
        readonly string _repositoryName;

        public Api(string userAgentName, string userAgentVersion, string repositoryOwner, string repositoryName)
        {
            _repositoryOwner = repositoryOwner;
            _repositoryName = repositoryName;

            var cnn = new Octokit.Connection(new Octokit.ProductHeaderValue(userAgentName, userAgentVersion));

            _connection = new Octokit.ApiConnection(cnn);
        }

        private static Octokit.Internal.HttpClientAdapter? GetHttpClientAdapterFromConnection(Octokit.Connection cnn)
        {
            System.Reflection.FieldInfo? fi = typeof(Octokit.Connection).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fi != null)
            {
                object? obj = fi.GetValue(cnn);
                if (obj != null)
                    return obj as Octokit.Internal.HttpClientAdapter;
            }

            return null;
        }

        private static System.Net.Http.HttpClient? GetHttpClientFromAdapter(Octokit.Internal.HttpClientAdapter? adapter)
        {
            if (adapter == null)
                return null;

            System.Reflection.FieldInfo? fi = typeof(Octokit.Internal.HttpClientAdapter).GetField("_http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fi != null)
            {
                object? obj = fi.GetValue(adapter);
                if (obj != null)
                    return obj as System.Net.Http.HttpClient;
            }

            return null;
        }

        public async Task<AssetMdl?> GetLatestAssetAsync(string assetPattern, bool onlyReleases)
        {
            var client = new Octokit.ReleasesClient(_connection);

            Octokit.Release? release;

            if (onlyReleases)
                release = await client.GetLatest(_repositoryOwner, _repositoryName);
            else
            {
                IReadOnlyList<Octokit.Release> allReleases = await client.GetAll(_repositoryOwner, _repositoryName, new Octokit.ApiOptions() { StartPage = 1, PageCount = 1, PageSize = 1 });

                if (allReleases.Count > 0)
                    release = allReleases[0];
                else
                    release = null;
            }

            if (release == null)
                return null;

            if (release.Assets.Count == 0)
                return null;

            Octokit.ReleaseAsset? asset;

            if (string.IsNullOrWhiteSpace(assetPattern))
                asset = release.Assets[0];
            else
            {
                var regex = new System.Text.RegularExpressions.Regex(assetPattern, System.Text.RegularExpressions.RegexOptions.Singleline);
                asset = release.Assets.FirstOrDefault(x => regex.IsMatch(x.Name));
            }

            if (asset == null)
                return null;

            return new AssetMdl(release.Id, asset.Name, asset.BrowserDownloadUrl, release.PublishedAt ?? release.CreatedAt);
        }

        public async Task DownloadAsync(string url, string filePath)
        {
            string? parentDirectory = Path.GetDirectoryName(filePath);
            if (parentDirectory != null)
            {
                if (!Directory.Exists(parentDirectory))
                    Directory.CreateDirectory(parentDirectory);
            }

            if (HttpClientAdapter == null || HttpClient == null)
                throw new MissingFieldException("The internal HttpClient couldn't be obtained");

            System.Net.Http.HttpResponseMessage response = await HttpClientAdapter.SendAsync(
                new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url),
                System.Threading.CancellationToken.None);

            response.EnsureSuccessStatusCode();

            using Stream httpStream = await HttpClient.GetStreamAsync(response.RequestMessage?.RequestUri);

            using FileStream fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            await httpStream.CopyToAsync(fileStream);
        }
    }
}
