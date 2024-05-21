using MetaheuristicsTester.Constants;
using MetaheuristicsTester.Functions;
using MetaheuristicsTester.Models;
using System;
using System.Collections;
using System.Diagnostics.Eventing.Reader;
using System.Linq;

namespace MetaheuristicsTester.Metaheuristics
{
    public class GeneticAlgorithm : IExperiment
    {
        private int _populationSize = 100;
        private double _mutationRate = 0.1;
        private double _crossoverRate = 0.9;
        private int _numberOfGenerations = 50;
        private int _dimensions = 100;
        private double _lowerBound = 0;
        private double _upperBound = 0;


        public string Name => nameof(GeneticAlgorithm);
        public string FunctionName { get; set; }

        public async Task<Result> ProcideExperiment()
        {
            var population = await InitializePopulation();
            var result = new Result();
            var bestFitPerIteration = new List<double>();

            for (int i = 0; i < _numberOfGenerations; i++)
            {
                var bestFitMember = population.OrderBy(x => BenchmarkFunctions.InvokeFunction(FunctionName, x)).First();
                var bestFitValue = BenchmarkFunctions.InvokeFunction(FunctionName, bestFitMember);
                bestFitPerIteration.Add(bestFitValue);

                var parents = await RankSelection(population);
                //var parents = await TournamentSelection(population);
                population = await CreateNewPopulation(parents, i);
            }

            result.BestFitArguments = population.OrderBy(x => BenchmarkFunctions.InvokeFunction(FunctionName, x)).First();
            result.BestFit = BenchmarkFunctions.InvokeFunction(FunctionName, result.BestFitArguments);
            bestFitPerIteration.Add(result.BestFit);
            result.BestFitsPerIteration = bestFitPerIteration;
            result.MeanFit = population.Select(x => BenchmarkFunctions.InvokeFunction(FunctionName, x)).Average();

            return result;
        }

        public void SetBoundaries(double lowerBound, double upperBound)
        {
            _lowerBound = lowerBound;
            _upperBound = upperBound;
        }

        public void SetHyperParameters(IDictionary<string, double> hyperParameters)
        {
            if(hyperParameters.ContainsKey(GeneticAlgorithmConstants.dimensions))
                _dimensions = (int)hyperParameters[GeneticAlgorithmConstants.dimensions];
            if (hyperParameters.ContainsKey(GeneticAlgorithmConstants.mutationRate))
                _mutationRate = hyperParameters[GeneticAlgorithmConstants.mutationRate];
            if (hyperParameters.ContainsKey(GeneticAlgorithmConstants.crossoverRate))
                _crossoverRate = hyperParameters[GeneticAlgorithmConstants.crossoverRate];
            if (hyperParameters.ContainsKey(GeneticAlgorithmConstants.numberOfGenerations))
                _numberOfGenerations = (int)hyperParameters[GeneticAlgorithmConstants.numberOfGenerations];
            if (hyperParameters.ContainsKey(GeneticAlgorithmConstants.populationSize))
                _populationSize = (int)hyperParameters[GeneticAlgorithmConstants.populationSize];
        }

        private async Task<IEnumerable<IEnumerable<double>>> InitializePopulation()
        {
            var population = new List<IEnumerable<double>>();
            var random = new Random();

            for (int i = 0; i < _populationSize; i++)
            {
                var gene = Enumerable.Range(0, _dimensions).Select(j => random.NextDouble() * (_upperBound - _lowerBound) + _lowerBound).ToList();
                population.Add(gene);
            }

            return population;
        }

        private async Task<IEnumerable<double>> Mutate(IEnumerable<double> gene, int genNum)
        {
            var random = new Random();
            if (random.NextDouble() > _mutationRate)
                return gene;

                var geneList = gene.ToList();
            

            var i = random.Next(0, geneList.Count);
                    
            if(random.Next(0,2) < 1)
            {
                var deltaT = (_upperBound - geneList[i]) * (1- Math.Pow( random.NextDouble(),Math.Pow(1 - genNum/_numberOfGenerations,3)));
                geneList[i] -= deltaT; 
                
            }
            else
            {
                var deltaT = (geneList[i] - _lowerBound) * (1 - Math.Pow(random.NextDouble(), Math.Pow(1 - genNum / _numberOfGenerations, 3)));
                geneList[i] += deltaT;
            }
            if (geneList[i] < _lowerBound) geneList[i] = _lowerBound;
            if (geneList[i] > _upperBound) geneList[i] = _upperBound;

            return geneList;
        }


        private async Task<(IEnumerable<double>, IEnumerable<double>)> Crossover(IEnumerable<double> parent1, IEnumerable<double> parent2, int numCuts = 3)
        {
            var offspring1 = new List<double>(_dimensions);
            var offspring2 = new List<double>(_dimensions);

            var parent1List = parent1.ToList();
            var parent2List = parent2.ToList();
            var random = new Random();
            var d = random.NextDouble();
            for (int i = 0; i < _dimensions; i++)
            {
                offspring1.Add(parent1List[i]*d+parent2List[i]*(1-d));
                offspring2.Add(parent2List[i] * d + parent1List[i]*(1-d));
            }

            return (offspring1, offspring2);
        }



        private async Task<IEnumerable<IEnumerable<double>>> RankSelection(IEnumerable<IEnumerable<double>> population)
        {
            Random r = new Random();
            var listOfParents = new List<IEnumerable<double>>();
            var listOfFitness = new List<(int index, double fitness)>();
            var populationList = population.ToList();
            for (int i = 0; i < populationList.Count; i++)
            {
                var fitness = BenchmarkFunctions.InvokeFunction(FunctionName, populationList[i]);
                listOfFitness.Add((i, fitness));
            }

            listOfFitness = listOfFitness.OrderBy(f => f.fitness).ToList();
            for (int i = 0; i < listOfFitness.Count; i++)
            {
                listOfFitness[i] = (listOfFitness[i].index, 2*(1.0 - Math.Exp(-i/2.0)));

            }

            var totalWeight = listOfFitness.Sum(x => x.fitness);
            for (int i = 0; i < _populationSize; i++)
            {
                var itemWeightIndex = r.NextDouble() * totalWeight;
                double currentWeightIndex = 0.0;

                foreach (var item in listOfFitness)
                {
                    currentWeightIndex += item.fitness;

                    
                    if (currentWeightIndex <= itemWeightIndex)
                    {
                        listOfParents.Add(populationList[item.index]);
                        break;
                    }
                        
                }
            }
            return listOfParents.OrderBy(_ => r.Next());
        }

        private async Task<IEnumerable<IEnumerable<double>>> TournamentSelection(IEnumerable<IEnumerable<double>> population)
        {
            var parents = new List<IEnumerable<double>>();
            var populationList = population.ToList();

            for (int i = 0; i < _populationSize; i++)
            {
                var tounamentPopulation = new List<IEnumerable<double>>();
                var random = new Random();
                var ks = new List<int>();
                for (int j = 0; j < populationList.Count / 4; j++)
                {
                    var k = random.Next(0, populationList.Count);
                    while(ks.Contains(k))
                        k = random.Next(0, populationList.Count);
                    ks.Add(k);
                    tounamentPopulation.Add(populationList[k]);
                }
                var parent = tounamentPopulation.OrderBy(f => BenchmarkFunctions.InvokeFunction(FunctionName, f)).First();
                parents.Add(parent);
            }
            return parents;
            
        }

        private async Task<IEnumerable<IEnumerable<double>>> CreateNewPopulation(IEnumerable<IEnumerable<double>> parents, int genNum)
        {
            var newPopulation = new List<IEnumerable<double>>();
            var r = new Random();

            // Perform crossover
            for (int i = 0; i < _populationSize / 2; i += 2)
            {
                if (r.NextDouble() < _crossoverRate)
                {
                    var (offspring1, offspring2) = await Crossover(parents.ElementAt(i), parents.ElementAt(i + 1));
                    newPopulation.Add(offspring1);
                    newPopulation.Add(offspring2);
                }
                else
                {
                    newPopulation.Add(parents.ElementAt(i));
                    newPopulation.Add(parents.ElementAt(i + 1));
                }
            }

            // Perform mutation
            for (int i = 0; i < newPopulation.Count; i++)
            {
                newPopulation[i] = await Mutate(newPopulation[i], genNum);
            }

            return newPopulation;
        }
    }
}
