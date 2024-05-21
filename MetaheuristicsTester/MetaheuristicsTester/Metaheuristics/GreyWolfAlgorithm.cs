using MetaheuristicsTester.Constants;
using MetaheuristicsTester.Functions;
using MetaheuristicsTester.Models;
using System;

namespace MetaheuristicsTester.Metaheuristics
{
    public class GreyWolfAlgorithm : IExperiment
    {
        private double _lowerBound = 0.0;
        private double _upperBound = 0.0;
        private int _populationSize = 0;
        private int _maxGenerations = 0;
        private int _dimensions = 0;


        public string Name => nameof(GreyWolfAlgorithm);

        public string FunctionName { get; set; }

        public async Task<Result> ProcideExperiment()
        {
            var output = new Result();

            var bestResultPerIterations = new List<double>();

            var random = new Random();
            var population = new List<GreyWolfIndividual>();
            for (int i = 0; i < _populationSize; i++)
            {
                population.Add(new GreyWolfIndividual(_dimensions, _lowerBound, _upperBound, random.Next()));
                population.Last().Fitness = BenchmarkFunctions.InvokeFunction(FunctionName, population.Last().Positions);
            }

            population.OrderBy(x => x.Fitness);

            var alphas = new GreyWolfIndividual[3] { new GreyWolfIndividual(population[0]), new GreyWolfIndividual(population[1]), new GreyWolfIndividual(population[2]) };

            for (int i = 0; i < _maxGenerations; i++)
            {
                bestResultPerIterations.Add(alphas[0].Fitness);
                double a = 2*(1 - i / _maxGenerations);

                var alphasPos = new List<double>[] { alphas[0].Positions.ToList(), alphas[1].Positions.ToList(), alphas[2].Positions.ToList() };

                var tasks = new List<Task<GreyWolfIndividual>>();
                for (int j = 0; j < _populationSize; j++)
                {
                    tasks.Add(UpdateIndividuals(population[j],random, a, alphasPos));
                }
                population = (await Task.WhenAll(tasks)).ToList();
                population.OrderBy(x => x.Fitness);
                alphas = [new GreyWolfIndividual(population[0]), new GreyWolfIndividual(population[1]), new GreyWolfIndividual(population[2])];
            }

            output.BestFit = alphas[0].Fitness;
            output.MeanFit = population.Average(x => x.Fitness);
            bestResultPerIterations.Add(output.BestFit);

            output.BestFitsPerIteration = bestResultPerIterations;
            output.BestFitArguments = alphas[0].Positions;
            return output;
        }

        public void SetBoundaries(double lowerBound, double upperBound)
        {
            _lowerBound = lowerBound;
            _upperBound = upperBound;
        }

        public void SetHyperParameters(IDictionary<string, double> hyperParameters)
        {
            if (hyperParameters.ContainsKey(GreyWolfAlgorithmConstants.dimensions))
                _dimensions = (int)hyperParameters[GreyWolfAlgorithmConstants.dimensions];
            if (hyperParameters.ContainsKey(GreyWolfAlgorithmConstants.maxGenerations))
                _maxGenerations = (int)hyperParameters[GreyWolfAlgorithmConstants.maxGenerations];
            if (hyperParameters.ContainsKey(GreyWolfAlgorithmConstants.populationSize))
                _populationSize = (int)hyperParameters[GreyWolfAlgorithmConstants.populationSize];
        }

        private async Task<GreyWolfIndividual> UpdateIndividuals(GreyWolfIndividual individual, Random random, double a, List<double>[] alphasPos)
        {
            var populationPos = individual.Positions.ToList();
            var ATable = new double[3] { a * (2 * random.NextDouble() - 1), a * (2 * random.NextDouble() - 1), a * (2 * random.NextDouble() - 1) };
            var CTable = new double[3] { 2 * random.NextDouble(), 2 * random.NextDouble(), 2 * random.NextDouble() };

            var Xnew = new double[_dimensions];
            for (var k = 0; k < _dimensions; k++)
            {
                var X1 = alphasPos[0][k] + ATable[0] * Math.Abs(CTable[0] * alphasPos[0][k] - populationPos[k]);
                var X2 = alphasPos[1][k] + ATable[1] * Math.Abs(CTable[1] * alphasPos[1][k] - populationPos[k]);
                var X3 = alphasPos[2][k] + ATable[2] * Math.Abs(CTable[2] * alphasPos[2][k] - populationPos[k]);
                Xnew[k] = (X1 + X2 + X3) / 3;
                if (Xnew[k] > _upperBound) Xnew[k] = _upperBound;
                if (Xnew[k] < _lowerBound) Xnew[k] = _lowerBound;
            }
            var fnew = BenchmarkFunctions.InvokeFunction(FunctionName, Xnew);
            if (fnew < individual.Fitness)
            {
                individual.Positions = Xnew;
                individual.Fitness = fnew;
            }
            return individual;
        }
    }
}
