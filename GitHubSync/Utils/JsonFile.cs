using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitHubSync.Utils
{
    class JsonFile
    {
        private string? filePath;

        public static async Task<T> GetAsync<T>(string filePath)
            where T : JsonFile, new()
        {
            T obj;
            if (!File.Exists(filePath))
            {
                obj = new T() { filePath = filePath };
                await obj.SaveJsonAsync();
                return obj;
            }

            using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous))
                obj = (await JsonSerializer.DeserializeAsync<T>(fileStream))!;

            obj.filePath = filePath;
            return obj;
        }

        public async Task SaveJsonAsync()
        {
            if (filePath == null)
                return;

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous);
            await JsonSerializer.SerializeAsync(fileStream, this, GetType(), new JsonSerializerOptions() { WriteIndented = true });
        }
    }
}
