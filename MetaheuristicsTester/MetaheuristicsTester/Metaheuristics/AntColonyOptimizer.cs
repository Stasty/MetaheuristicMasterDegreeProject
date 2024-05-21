using MathNet.Numerics.Distributions;
using MetaheuristicsTester.Constants;
using MetaheuristicsTester.Functions;
using MetaheuristicsTester.Models;

namespace MetaheuristicsTester.Metaheuristics
{
    public class AntColonyOptimizer : IExperiment
    {
        private double _lowerBound =0.0;
        private double _upperBound =0.0;

        private int _dimensions = 30;
        private int _kernels = 50;
        private int _numberOfGenerations = 50;

        private readonly Random _random = new Random();

        public string Name => nameof(AntColonyOptimizer);

        public string FunctionName { get; set; }

        public async Task<Result> ProcideExperiment()
        {
            var population = await InitializePopulation();
            population =  await SortByFitness(population);
            var populationsDeviations = await GetDeviations(population);
            var bestFitPerIteration = new List<double>();
            
            for(int i = 0; i < _numberOfGenerations; i++)
            {
                bestFitPerIteration.Add(await GetFittnessValueForRow(population, 0));
                var newPopulation = await GenerateNewPopulation(population, populationsDeviations);
                population = await GetFinalPopulationFromNewAndOld(population, newPopulation);
                populationsDeviations = await GetDeviations(population);
            }
            var sums = 0.0;
            for(int i = 0; i< _kernels; i++)
            {
                sums += await GetFittnessValueForRow(population, i);
            }
            var bestRow = Enumerable.Range(0, _dimensions).Select(x => population[0, x]).ToArray();
            bestFitPerIteration.Add(await GetFittnessValueForRow(population, 0));
            var result = new Result()
            {
                BestFit = await GetFittnessValueForRow(population, 0),
                BestFitArguments = bestRow,
                MeanFit = sums / _kernels,
                BestFitsPerIteration = bestFitPerIteration
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
            if (hyperParameters.ContainsKey(AntColonyOptimizerConstants.dimensions))
                _dimensions = (int)hyperParameters[AntColonyOptimizerConstants.dimensions];
            if (hyperParameters.ContainsKey(AntColonyOptimizerConstants.numberOfGenerations))
                _numberOfGenerations = (int)hyperParameters[AntColonyOptimizerConstants.numberOfGenerations];
            if (hyperParameters.ContainsKey(AntColonyOptimizerConstants.kernels))
                _kernels = (int)hyperParameters[AntColonyOptimizerConstants.kernels];
        }

        public async Task<double[,]> InitializePopulation()
        {
            double[,] sample = new double[_kernels, _dimensions];
            // Generate random values within each interval for each dimension

            for (int i = 0; i < _dimensions; i++)
            {
                double step = (_upperBound - _lowerBound) / _kernels;
                for (int j = 0; j < _kernels; j++)
                {
                    sample[j, i] = _lowerBound + _random.NextDouble() * step + j * step;
                }
                await ShuffleDimension(sample, _kernels, i);
            }

            return sample;
        }

        private async Task ShuffleDimension(double[,] sample, int n, int dimension)
        {
            for (int i = n - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                if (i != j)
                {
                    double temp = sample[i, dimension];
                    sample[i, dimension] = sample[j, dimension];
                    sample[j, dimension] = temp;
                }
            }
        }

        private async Task<double[,]> SortByFitness(double[,] sample)
        {
            var soritngArray = new Dictionary<int, double>();
            for (int i = 0; i < sample.GetLength(0); i++)
                soritngArray[i] = await GetFittnessValueForRow(sample, i);

            var sorting = soritngArray.OrderBy(x => x.Value).Select(x => x.Key).ToArray();
            var sortedArray = new double[sample.GetLength(0), sample.GetLength(1)];
            for(int j = 0; j < sample.GetLength(0); j++)
            {
                for (int i = 0; i < sample.GetLength(1); i++)
                    sortedArray[j,i] = sample[sorting[j],i];
            }
            return sortedArray;
        }

        private async Task<double[,]> GetDeviations(double[,] population)
        {
            var deviations = new double[_kernels, _dimensions];
            
            for (int i = 0; i < _dimensions; i++)
            {
                for (int j = 0; j < _kernels; j++)
                {
                    var sums = 0.0;
                    for (int k = 0; k < _kernels; k++)
                    {
                        if (k != j)
                            sums += (population[k, i] - population[j, i])/(_kernels-1);
                    }
                    deviations[j, i] = Math.Abs(double.Epsilon * sums);
                }
            }
            return deviations;
        }

        private async Task<double> GetFittnessValueForRow(double[,] array, int row)
        {
            var rowEnumarable = Enumerable.Range(0, array.GetLength(1)).Select(x => array[row, x]);
            return BenchmarkFunctions.InvokeFunction(FunctionName, rowEnumarable.ToArray());
        }

        private async Task<double[,]> GenerateNewPopulation(double[,] population, double[,] deviations)
        {
            var newPopulation = new double[_kernels, _dimensions];
            for(int i = 0; i < _kernels; i++)
            {
                for(int j = 0; j < _dimensions; j++)
                {
                    var q = _random.NextDouble();
                    Normal normal;
                    if(q > 0.5)
                        normal = new Normal(population[i, j], deviations[i, j]);
                    else
                        normal = new Normal(population[0, j], deviations[0, j]);
                    newPopulation[i, j] = normal.Sample();
                }
            }
            return newPopulation;
        }

        private async Task<double[,]> GetFinalPopulationFromNewAndOld(double[,] oldPopulation, double[,] newPopulation)
        {
            var wholePopulation = new double[_kernels*2, _dimensions];
            for (int i = 0; i < _kernels; i++)
            {
                for(int j = 0; j<_dimensions; j++)
                {
                    wholePopulation[i,j] = oldPopulation[i,j];
                    wholePopulation[i + _kernels, j] = newPopulation[i, j]; 
                }
            }
            var sortedPopulation = await SortByFitness(wholePopulation);
            var finalPopulation = new double[_kernels, _dimensions];
            for (int i = 0; i < _kernels; i++)
            {
                for (int j = 0; j < _dimensions; j++)
                {
                    finalPopulation[i, j] = sortedPopulation[i, j];
                }
            }

            return finalPopulation;
        }

    }
}
