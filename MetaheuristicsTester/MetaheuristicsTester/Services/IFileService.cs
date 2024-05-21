using MetaheuristicsTester.Models;
using System.Text.Json;

namespace MetaheuristicsTester.Services
{
    public interface IFileService
    {
        public IEnumerable<ExperimentParameters> ReadFile(string path);

        public void WriteFile(string path, IEnumerable<Output> experiments);
    }
}
