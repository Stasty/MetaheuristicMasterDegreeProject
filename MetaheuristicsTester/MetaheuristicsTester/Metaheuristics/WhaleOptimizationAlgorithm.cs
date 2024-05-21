using MetaheuristicsTester.Constants;
using MetaheuristicsTester.Functions;
using MetaheuristicsTester.Models;

namespace MetaheuristicsTester.Metaheuristics
{
    public class WhaleOptimizationAlgorithm : IExperiment
    {
        private double _lowerBound;
        private double _upperBound;

        private double _b;

        private int _maxIterations;
        private int _populationSize;
        private int _dimensions;
      
        private readonly Random _random = new Random();

        public string Name => nameof(WhaleOptimizationAlgorithm);

        public string FunctionName { get; set; }

        public async Task<Result> ProcideExperiment()
        {
            var population = (await InitializePopulation()).ToList();
            var bestFitPerIteration = new List<double>();
            var bestFit = population.OrderBy(x => BenchmarkFunctions.InvokeFunction(FunctionName, x)).First();
            bestFitPerIteration.Add(BenchmarkFunctions.InvokeFunction(FunctionName, bestFit));
            for (int i = 0; i < _maxIterations; i++)
            {
                
                for(int j = 0; j < _populationSize; j++)
                {
                    var r = _random.NextDouble();
                    var c = 2 * r;
                    var a1 = 2 - i * (2 / _maxIterations);
                    var a = 2 * a1 * r - a1;
                    var l = 2 * _random.NextDouble() - 1;
                    var p = _random.NextDouble();
                    var position = new List<double>();

                    if (p < 0.5)
                    {
                        if (Math.Abs(a) < 1)
                            position = (await SearchingePrey(population[j], population, c, a)).ToList();
                        else
                            position = (await EncirclingThePrey(population[j], bestFit, c, a)).ToList();
                    }
                    else
                    {
                        position = (await SpiralBubbleNetAttack(population[j], bestFit, l)).ToList();
                    }
                    for (int k = 0; k < _dimensions; k++)
                    {
                        if (position[k] < _lowerBound || position[k] > _upperBound)
                        {
                            position[k] = _lowerBound + _random.NextDouble() * (_upperBound - _lowerBound);
                        }
                    }
                    var currentFit = BenchmarkFunctions.InvokeFunction(FunctionName, population[j]);
                    var newFit = BenchmarkFunctions.InvokeFunction(FunctionName, position);
                    if(newFit < currentFit)
                        population[j] = position;
                    if (newFit < BenchmarkFunctions.InvokeFunction(FunctionName, bestFit))
                    {
                        bestFit = population.OrderBy(x => BenchmarkFunctions.InvokeFunction(FunctionName, x)).First();
                    }
                }
                bestFitPerIteration.Add(BenchmarkFunctions.InvokeFunction(FunctionName, bestFit));
            }
            var result = new Result()
            {
                BestFit = BenchmarkFunctions.InvokeFunction(FunctionName, bestFit),
                MeanFit = population.Select(x => BenchmarkFunctions.InvokeFunction(FunctionName, x)).Average(),
                BestFitsPerIteration = bestFitPerIteration,
                BestFitArguments = bestFit
            };
            return result;
        }

        public void SetBoundaries(double lowerBound, double upperBound)
        {
            _lowerBound = lowerBound;
            _upperBound = upperBound;
        }

        public void SetHyperParameters(IDictionary<string, double> hyperParameters)
        {
            if (hyperParameters.ContainsKey(WhaleOptimizationAlgorithmConstants.populationSize))
                _populationSize = (int)hyperParameters[WhaleOptimizationAlgorithmConstants.populationSize];
            if (hyperParameters.ContainsKey(WhaleOptimizationAlgorithmConstants.dimensions))
                _dimensions = (int)hyperParameters[WhaleOptimizationAlgorithmConstants.dimensions];
            if (hyperParameters.ContainsKey(WhaleOptimizationAlgorithmConstants.maxIterations))
                _maxIterations = (int)hyperParameters[WhaleOptimizationAlgorithmConstants.maxIterations];
            if (hyperParameters.ContainsKey(WhaleOptimizationAlgorithmConstants.b))
                _b = hyperParameters[WhaleOptimizationAlgorithmConstants.b];
        }

        public async Task<IEnumerable<IEnumerable<double>>> InitializePopulation()
        {
            var population = new List<List<double>>();
            for (int i = 0; i < _populationSize; i++)
            {
                var whale = new List<double>();
                for (int j = 0; j < _dimensions; j++)
                {
                    whale.Add(_lowerBound + _random.NextDouble() * (_upperBound - _lowerBound));
                }
                population.Add(whale);
            }
            return population;
        }
        private async Task<IEnumerable<double>> EncirclingThePrey(IEnumerable<double> whale, IEnumerable<double> prey, double c, double a)
        {
            var newPositions = new List<double>();
            var whaleList = whale.ToList();
            var preyList = prey.ToList();
            for (int i = 0; i < _dimensions; i++)
            {
                var d = Math.Abs(c*preyList[i] - whaleList[i]);
                newPositions.Add(preyList[i]-a*d);
            }
            return newPositions;
        }

        private async Task<IEnumerable<double>> SearchingePrey(IEnumerable<double> whale, IEnumerable< IEnumerable<double>> population, double c, double a)
        {
            var newPositions = new List<double>();
            var whaleList = whale.ToList();
            var index = _random.Next(population.Count());
            var populationList = population.ToList();
            while(populationList[index] == whale)
                index = _random.Next(population.Count());
            var preyList = populationList[index].ToList();
            for (int i = 0; i < _dimensions; i++)
            {
                var d = Math.Abs(c * preyList[i] - whaleList[i]);
                newPositions.Add(preyList[i] - a * d);
            }
            return newPositions;
        }

        private async Task<IEnumerable<double>> SpiralBubbleNetAttack(IEnumerable<double> whale, IEnumerable<double> prey, double l)
        {
            var newPositions = new List<double>();
            var whaleList = whale.ToList();
            var preyList = prey.ToList();
            for (int i = 0; i < _dimensions; i++)
            {
                var d = Math.Abs(preyList[i] - whaleList[i]);
                newPositions.Add(d* Math.Exp(_b*l)*Math.Cos(2*Math.PI*l) + preyList[i]);
            }
            return newPositions;
        }
    }
}
