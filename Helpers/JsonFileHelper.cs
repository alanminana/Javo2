using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Javo2.Helpers
{
    public static class JsonFileHelper
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public static async Task<T> LoadFromJsonFileAsync<T>(string filePath) where T : new()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new T();
                }
                var json = await File.ReadAllTextAsync(filePath);
                var data = JsonSerializer.Deserialize<T>(json, _options);
                return data ?? new T();
            }
            catch
            {
                return new T();
            }
        }

        public static async Task SaveToJsonFileAsync<T>(string filePath, T data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _options);

                // Asegurar que el directorio exista
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(filePath, json);
            }
            catch
            {
                // Manejo opcional de errores
            }
        }

        // Versiones síncronas
        public static T LoadFromJsonFile<T>(string filePath) where T : new()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new T();
                }
                var json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<T>(json, _options);
                return data ?? new T();
            }
            catch
            {
                return new T();
            }
        }

        public static void SaveToJsonFile<T>(string filePath, T data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _options);

                // Asegurar que el directorio exista
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json);
            }
            catch
            {
                // Manejo opcional de errores
            }
        }
    }
}