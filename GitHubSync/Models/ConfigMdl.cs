using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubSync.Models
{
    class ConfigMdl : Utils.JsonFile
    {
        public string RepositoryOwner { get; set; } = string.Empty;
        public string RepositoryName { get; set; } = string.Empty;
        public string AssetNamePattern { get; set; } = string.Empty;
        public bool OnlyReleases { get; set; } = true;
        public string OutputDirectory { get; set; } = ".";
        public string Executable { get; set; } = string.Empty;
    }
}
