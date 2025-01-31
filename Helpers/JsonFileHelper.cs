// Ruta sugerida: Helpers/JsonFileHelper.cs
using System.Text.Json;

namespace Javo2.Helpers
{
    public static class JsonFileHelper
    {
        /// <summary>
        /// Carga un archivo JSON desde la ruta especificada y lo deserializa a la clase genérica T.
        /// Si no existe el archivo o hay algún error, retorna una instancia nueva de T.
        /// </summary>
        public static T LoadFromJsonFile<T>(string filePath) where T : new()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new T(); // Retorna objeto nuevo si no existe el archivo
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
                // En caso de error, retornamos objeto nuevo
                return new T();
            }
        }

        /// <summary>
        /// Serializa la instancia de T en JSON y la guarda en el archivo especificado.
        /// </summary>
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
                // Manejo de errores opcional (log, etc.)
            }
        }
    }
}
