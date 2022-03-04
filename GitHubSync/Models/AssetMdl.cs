using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubSync.Models
{
    class AssetMdl
    {
        public int Id { get; }
        public string Name { get; }
        public string Url { get; }
        public DateTimeOffset DateTime { get; }

        public AssetMdl(
            int id,
            string name,
            string url,
            DateTimeOffset dateTime)
        {
            Id = id;
            Name = name;
            Url = url;
            DateTime = dateTime;
        }
    }
}
