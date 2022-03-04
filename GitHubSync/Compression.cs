using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubSync
{
    static class Compression
    {
        const string EXECUTABLE = "7za.exe";

        public static async Task ExtractFileAsync(string file, string path)
        {
            if (!File.Exists(EXECUTABLE))
                throw new FileNotFoundException(EXECUTABLE);

            using var process = new Process();
            process.StartInfo.FileName = EXECUTABLE;
            process.StartInfo.CreateNoWindow = true;

            process.StartInfo.Arguments = $"x \"{file}\" -y -o\"{path}\"";

            process.Start();
            await process.WaitForExitAsync();
        }
    }
}
