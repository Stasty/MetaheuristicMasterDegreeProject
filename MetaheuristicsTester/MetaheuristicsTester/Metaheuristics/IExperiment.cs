using MetaheuristicsTester.Models;

namespace MetaheuristicsTester.Metaheuristics
{
    public interface IExperiment
    {
        public string Name { get; }
        public string FunctionName { get; set; }

        public void SetBoundaries(double lowerBound, double upperBound);

        public Task<Result> ProcideExperiment();
        public void SetHyperParameters(IDictionary<string, double> hyperParameters);
    }
}
