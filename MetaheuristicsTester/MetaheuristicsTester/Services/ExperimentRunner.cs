using MetaheuristicsTester.Constants;
using MetaheuristicsTester.Metaheuristics;
using MetaheuristicsTester.Models;

namespace MetaheuristicsTester.Services
{
    public class ExperimentRunner: IExperimentRunner
    {
        private const int NumberOfExperimentsAtOnce = 50;
        private readonly IExperiment experiment;
        public ExperimentParameters experimentParameters { get; set; }
        public static string AlgorithmName { get; set; }

        public ExperimentRunner(Func<string,IExperiment> experimentFactory)
        {
            if(!string.IsNullOrEmpty(AlgorithmName))
                experiment = experimentFactory(AlgorithmName);
        }

        public async Task<Output> Run()
        {
            if (experiment == null)
                throw new Exception("Wrong Algorithm name");
            experiment.FunctionName = experimentParameters.FunctionName;
            if (string.IsNullOrEmpty(experiment.FunctionName) || !FunctionsConstants.FunctionsBoundaries.ContainsKey(experiment.FunctionName))
                throw new Exception("No or wrong function defined");
            var boundaries = FunctionsConstants.FunctionsBoundaries[experiment.FunctionName];
            experiment.SetBoundaries(boundaries.Item1,boundaries.Item2);
            experiment.SetHyperParameters(experimentParameters.Parameters);
            var RunsBatches = experimentParameters.NumberOfExperiments /NumberOfExperimentsAtOnce;
            var results = new List<Result>();
            for(int i = 0; i < RunsBatches; i++)
            {
                var experimentTasks = new List<Task<Result>>();
                for (int j = 0; j < NumberOfExperimentsAtOnce; j++)
                {
                    experimentTasks.Add(experiment.ProcideExperiment());
                }

                var batchResults = await Task.WhenAll(experimentTasks);
                results.AddRange(batchResults);
            }
            return new Output()
            {
                FunctionName = experimentParameters.FunctionName,
                AlgorithmName = AlgorithmName,
                Results = results,
            };
        }
    }
}
