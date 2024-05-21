using MetaheuristicsTester.Constants;
using MetaheuristicsTester.Functions;
using MetaheuristicsTester.Models;

namespace MetaheuristicsTester.Metaheuristics
{
    public class ArtificialBeeColony : IExperiment
    {
        private double _lowerBound =0.0;
        private double _upperBound =0.0;
        private double _c = 0.0;
        private int _dimensions = 30;
        private int _foodSourcePoolSize = 100;
        private int _neighbourSearch=5;
        private int _abondmendLimit=100;
        private int _maxIterations=3000*30;
        private readonly Random _random = new Random();


        public string Name => nameof(ArtificialBeeColony);

        public string FunctionName { get; set; }

        public async Task<Result> ProcideExperiment()
        {
            var FEs = _foodSourcePoolSize;
            var population = InitializePopulation();
            var result = new Result();
            var bestFitPerIteration = new List<double>();
            while (FEs < _maxIterations)
            {
                bestFitPerIteration.Add(population.OrderBy(x => x.Fitness).First().Fitness);
                population = EmployedBeePhase(population, ref FEs);
                population = OnlookerBeePhase(population, ref FEs);
                population = ScoutBeePhase(population, ref FEs);
            }
            var best = population.OrderBy(x => x.Fitness).First();
            bestFitPerIteration.Add(best.Fitness);
            result.BestFitArguments = best.Positions;
            result.BestFit = best.Fitness;
            result.BestFitsPerIteration = bestFitPerIteration;
            result.MeanFit = population.Select(x => x.Fitness).Average();
            return result;
        }

        public void SetBoundaries(double lowerBound, double upperBound)
        {
            _lowerBound = lowerBound;
            _upperBound = upperBound;
        }

        public void SetHyperParameters(IDictionary<string, double> hyperParameters)
        {
            if(hyperParameters.ContainsKey(ArtificialBeeColonyConstants.c))
                _c = hyperParameters[ArtificialBeeColonyConstants.c];
            if (hyperParameters.ContainsKey(ArtificialBeeColonyConstants.dimensions))
                _dimensions = (int)hyperParameters[ArtificialBeeColonyConstants.dimensions];
            if (hyperParameters.ContainsKey(ArtificialBeeColonyConstants.foodSourcePoolSize));
                _foodSourcePoolSize = (int)hyperParameters[ArtificialBeeColonyConstants.foodSourcePoolSize];
            if (hyperParameters.ContainsKey(ArtificialBeeColonyConstants.neighbourSearch))
                _neighbourSearch = (int)hyperParameters[ArtificialBeeColonyConstants.neighbourSearch];
            if (hyperParameters.ContainsKey(ArtificialBeeColonyConstants.abondmendLimit))
                _abondmendLimit = (int)hyperParameters[ArtificialBeeColonyConstants.abondmendLimit];
            if (hyperParameters.ContainsKey(ArtificialBeeColonyConstants.maxIterations))
                _maxIterations = (int)hyperParameters[ArtificialBeeColonyConstants.maxIterations];
            _maxIterations *= _dimensions;
        }

        private IEnumerable<FoodSource> InitializePopulation()
        {
            var population = new List<FoodSource>();
            for(int i = 0; i < _foodSourcePoolSize; i++)
            {
                var foodSource = CreateBasicFoodSource();
                population.Add(foodSource);
            }
            return population;
        }

        private FoodSource CreateBasicFoodSource()
        {
            var foodSource = new FoodSource();
            foodSource.Positions = new double[_dimensions];
            for (int j = 0; j < _dimensions; j++)
            {
                foodSource.Positions[j] = _lowerBound + (_upperBound - _lowerBound) * _random.NextDouble();
            }
            foodSource.Fitness = BenchmarkFunctions.InvokeFunction(FunctionName, foodSource.Positions);
            foodSource.Limit = 0;
            return foodSource;
        }

        private IEnumerable<FoodSource> EmployedBeePhase(IEnumerable<FoodSource> foodSources, ref int FEs)
        {
            var r = new Random();
            var foodSourcePool = foodSources.ToArray();
            for(int i = 0; i < _foodSourcePoolSize; i++)
            {
                var j = _random.Next(0, _dimensions);
                var bestFoodSource = foodSourcePool.OrderBy(x => x.Fitness).First();
                var neighbourhoodsBestJ = GetNeighbourhood(foodSourcePool, i).OrderBy(x=>x.Fitness).First().Positions[j];

                var phiJR = -1 + 2 * _random.NextDouble();
                var phiC = _random.NextDouble()*_c;
                var k = GetRandomIndexExcluding([i]);

                var Xkj = foodSourcePool[k].Positions[j];
                var V = new double[_dimensions];
                foodSourcePool[i].Positions.CopyTo(V, 0);

                V[j] = neighbourhoodsBestJ + phiJR * (neighbourhoodsBestJ - Xkj) + phiC * (bestFoodSource.Positions[j] - neighbourhoodsBestJ);
                var fitness = BenchmarkFunctions.InvokeFunction(FunctionName, V);

                if (fitness < foodSourcePool[i].Fitness)
                {
                    foodSourcePool[i].Fitness = fitness;
                    foodSourcePool[i].Positions = V;
                }
                else
                    foodSourcePool[i].Limit++;
                FEs++;
            }
            return foodSourcePool;
        }

        private IEnumerable<FoodSource> GetNeighbourhood(IEnumerable<FoodSource> population, int index)
        {
            var neighbourhood = new List<FoodSource>();
            index += _foodSourcePoolSize;
            var populationList = population.ToList();
            for(int i = 0; i < _neighbourSearch; i++)
            {
                neighbourhood.Add(populationList[(index - i) % _foodSourcePoolSize]);
                neighbourhood.Add(populationList[(index + i) % _foodSourcePoolSize]);
            }
            return neighbourhood;
        }

        private IEnumerable<FoodSource> OnlookerBeePhase(IEnumerable<FoodSource> foodSources, ref int FEs)
        {
            var r = new Random();
            var foodSourcePool = foodSources.ToArray();
            for(int i = 0; i < _foodSourcePoolSize; i++)
            {
                var j = _random.Next(0, _dimensions);
                var neighbourhoodsBest = GetNeighbourhood(foodSourcePool, i).OrderBy(x => x.Fitness).First();
                var phiJR = -1 + 2 * _random.NextDouble();
                var k = GetRandomIndexExcluding([i]);

                var Xkj = foodSourcePool[k].Positions[j];
                var V = new double[_dimensions];
                neighbourhoodsBest.Positions.CopyTo(V, 0);

                V[j] = neighbourhoodsBest.Positions[j] + phiJR * (neighbourhoodsBest.Positions[j] - Xkj);
                var fitness = BenchmarkFunctions.InvokeFunction(FunctionName, V);

                if (fitness < foodSourcePool[i].Fitness)
                {
                    foodSourcePool[i].Fitness = fitness;
                    foodSourcePool[i].Positions = V;
                }
                else
                    foodSourcePool[i].Limit++;
                FEs++;
            }
            return foodSourcePool;
        }
        private IEnumerable<FoodSource> ScoutBeePhase(IEnumerable<FoodSource> foodSources, ref int FEs)
        {
            if (!foodSources.Any(x => x.Limit > _abondmendLimit))
                return foodSources;

            var foodSourcePool = foodSources.ToArray();

            for (int i = 0; i < _foodSourcePoolSize; i++)
            {
                if (foodSourcePool[i].Limit >= _abondmendLimit)
                {
                    var newFoodSources = new List<FoodSource>();
                    var U1 = CreateBasicFoodSource();
                    var bessFromNeighborhood = GetNeighbourhood(foodSourcePool, i).OrderBy(x => x.Fitness).First();
                    var ib = Array.IndexOf(foodSourcePool, bessFromNeighborhood);
                    var r1 = GetRandomIndexExcluding(new int[] { ib });
                    var r2 = GetRandomIndexExcluding(new int[] { r1, ib });

                    var U2 = new FoodSource { Positions = new double[_dimensions], Limit = 0 };
                    var U3 = new FoodSource { Positions = new double[_dimensions], Limit = 0 };

                    for (int j = 0; j < _dimensions; j++)
                    {
                        U2.Positions[j] = bessFromNeighborhood.Positions[j] + _random.NextDouble() * (foodSourcePool[r1].Positions[j] - foodSourcePool[ib].Positions[j]);
                        var xmin = foodSourcePool.Select(x => x.Positions[j]).Min();
                        var xmax = foodSourcePool.Select(x => x.Positions[j]).Max();
                        U3.Positions[j] = xmin + xmax - bessFromNeighborhood.Positions[j];
                    }

                    U2.Fitness = BenchmarkFunctions.InvokeFunction(FunctionName, U2.Positions);
                    U3.Fitness = BenchmarkFunctions.InvokeFunction(FunctionName, U3.Positions);

                    newFoodSources.Add(U1);
                    newFoodSources.Add(U2);
                    newFoodSources.Add(U3);

                    foodSourcePool[i] = newFoodSources.OrderBy(x => x.Fitness).First();
                    FEs += 3;
                }
            }

            return foodSourcePool;
        }

        private int GetRandomIndexExcluding(int[] indexExcluded)
        {
            var r = new Random();
            int randomIndex = _random.Next(0, _foodSourcePoolSize);
            while(indexExcluded.Any(x => x == randomIndex))
            {
                randomIndex = _random.Next(0, _foodSourcePoolSize);
            }
            return randomIndex;
        }
    }
}
