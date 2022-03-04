using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubSync.Models
{
    class DataConfigMdl : Utils.JsonFile
    {
        public string CurrentRepositoryOwner { get; set; } = string.Empty;
        public string CurrentRepositoryName { get; set; } = string.Empty;
        public DateTime LastCheck { get; set; }
        public int CurrentAssetId { get; set; }
    }
}
