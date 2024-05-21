namespace MetaheuristicsTester.Models
{
    public class ExperimentParameters
    {
        public int NumberOfExperiments { get; set; }
        public string FunctionName { get; set; }
        public string AlgorithmName { get; set; }
        public IDictionary<string, double> Parameters { get; set; }
    }
}
