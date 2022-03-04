using GitHubSync.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GitHubSync
{
    class Program
    {
        static async Task Main()
        {
            ConfigMdl cfg;
            try
            {
                cfg = await Utils.JsonFile.GetAsync<ConfigMdl>(nameof(GitHubSync) + "_Config.json");

                if (string.IsNullOrWhiteSpace(cfg.RepositoryOwner) || string.IsNullOrWhiteSpace(cfg.RepositoryName))
                {
                    Console.WriteLine("Please fill repository information in the configuration file.");
                    Console.ReadKey();
                    return;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return;
            }

            DataConfigMdl dataCfg;
            try
            {
                dataCfg = await Utils.JsonFile.GetAsync<DataConfigMdl>(nameof(GitHubSync) + "_Data.json");

                if (dataCfg.CurrentRepositoryOwner != cfg.RepositoryOwner ||
                    dataCfg.CurrentRepositoryName != cfg.RepositoryName)
                {
                    dataCfg.CurrentRepositoryOwner = cfg.RepositoryOwner;
                    dataCfg.CurrentRepositoryName = cfg.RepositoryName;
                    dataCfg.LastCheck = default;
                    dataCfg.CurrentAssetId = default;
                    await dataCfg.SaveJsonAsync();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return;
            }

            if (dataCfg.LastCheck < DateTime.Today)
            {
                try
                {
                    dataCfg.LastCheck = DateTime.Today;
                    await dataCfg.SaveJsonAsync();

                    var api = new Api(
                        nameof(GitHubSync),
                        typeof(Program).Assembly.GetName().Version?.ToString(2) ?? "1.0",
                        cfg.RepositoryOwner,
                        cfg.RepositoryName);

                    Console.WriteLine("Checking for version...");

                    var asset = await api.GetLatestAssetAsync(cfg.AssetNamePattern, cfg.OnlyReleases);

                    if (asset != null)
                    {
                        if (asset.Id != dataCfg.CurrentAssetId)
                        {
                            string filePath = Path.Combine(nameof(GitHubSync) + "_Downloads", asset.Name);

                            Console.WriteLine("Downloading new version...");
                            await api.DownloadAsync(asset.Url, filePath);

                            Console.WriteLine("Extracting new version...");

                            if (string.IsNullOrWhiteSpace(cfg.OutputDirectory))
                            {
                                cfg.OutputDirectory = ".";
                                await cfg.SaveJsonAsync();
                            }

                            await Compression.ExtractFileAsync(filePath, cfg.OutputDirectory);

                            dataCfg.CurrentAssetId = asset.Id;
                            await dataCfg.SaveJsonAsync();

                            try
                            {
                                File.Delete(filePath);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine($"Failed to delete \"{filePath}\"{Environment.NewLine}{ex.Message}");
                                Console.ReadKey();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }

            try
            {
                if (string.IsNullOrWhiteSpace(cfg.Executable))
                    return;

                string fullPath = Path.Combine(cfg.OutputDirectory, cfg.Executable);
                System.Diagnostics.Process.Start(fullPath);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        static void HandleException(Exception ex)
        {
            Console.Error.WriteLine(ex);
            Console.ReadKey();
        }
    }
}
