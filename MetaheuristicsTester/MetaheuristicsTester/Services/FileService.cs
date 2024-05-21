using MetaheuristicsTester.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaheuristicsTester.Services
{
    public class FileService: IFileService
    {
        public IEnumerable<ExperimentParameters> ReadFile(string path)
        {
            var json = new List<ExperimentParameters>();
            try
            {
                using (var file = File.OpenText(path))
                {
                    var textFile = file.ReadToEnd();
                    json = JsonSerializer.Deserialize<List<ExperimentParameters>>(textFile);
                    file.Close();
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return json;
        }

        public void WriteFile(string path, IEnumerable<Output> experiments)
        {
            var options = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            };
            var json = JsonSerializer.Serialize(experiments, options);
            using (var file = File.CreateText(path))
            {
                file.Write(json);
                file.Close();
            }
        }
    }
}
