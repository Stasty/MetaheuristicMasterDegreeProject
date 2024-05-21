using MetaheuristicsTester.Models;


namespace MetaheuristicsTester.Services
{
    public interface IExperimentRunner
    {
        public ExperimentParameters experimentParameters { get; set; }
        public static string AlgorithmName { get; set; }

        public Task<Output> Run();
    }
}
