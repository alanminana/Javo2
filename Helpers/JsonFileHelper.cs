using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Javo2.Helpers
{
    public static class JsonFileHelper
    {
        public static async Task<T> LoadFromJsonFileAsync<T>(string filePath) where T : new()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new T();
                }
                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                var data = JsonSerializer.Deserialize<T>(json, options);
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
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(data, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch
            {
                // Manejo de errores opcional (logging, etc.)
            }
        }

        // Métodos síncronos existentes...
        public static T LoadFromJsonFile<T>(string filePath) where T : new()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new T();
                }
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                var data = JsonSerializer.Deserialize<T>(json, options);
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
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(filePath, json);
            }
            catch
            {
                // Manejo de errores opcional
            }
        }
    }
}