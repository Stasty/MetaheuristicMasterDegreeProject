using MathNet.Numerics;
using MetaheuristicsTester.Constants;
using MetaheuristicsTester.Extensions;
using MetaheuristicsTester.Functions;
using MetaheuristicsTester.Models;
using System.Collections.Generic;
using System.Numerics;



namespace MetaheuristicsTester.Metaheuristics
{
    public class KrillHerd : IExperiment
    {
        private double _lowerBound;
        private double _upperBound;

        private int _dimensions = 30;
        private int _numberOfGenerations = 100;
        private int _populationSize = 100;

        private double _maxInductionSpeed = 1.0;
        private double _maxDiffusionSpeed = 1.0;
        private double _speedToEmbraceFood = 0.02;
        private double _alpha = 0.02;

        private readonly Random _random = new Random();

        public string Name => nameof(KrillHerd);

        public string FunctionName { get; set; }

        public async Task<Result> ProcideExperiment()
        {
            var population = (await InitializePopulation()).OrderBy(x=>x.Fitness);
            var foodPosition = await CreateFood(population);
            var bestPerIteration = new List<double>();
            
            var bestKrill = (Krill)population.First().Clone();
            var worstKrill = (Krill)population.Last().Clone();
            bestPerIteration.Add(bestKrill.Fitness);
            var inertiaAtraction = _random.NextDouble();
            var induceWeight = _random.NextDouble();
            var foodWeight = _random.NextDouble();
            for (int i = 0; i < _numberOfGenerations; i++)
            {
                inertiaAtraction = ChaoticTentMap(inertiaAtraction);
                induceWeight = ChaoticTentMap(induceWeight);
                foodWeight = ChaoticTentMap(foodWeight);
                foodPosition = await CreateFood(population);
                foreach(var krill in population)
                {
                    await InducedMotion(krill, population, i, induceWeight, bestKrill, worstKrill);
                    await FoodMotion(krill, population, i, foodWeight, bestKrill, worstKrill, foodPosition);
                    await DiffusionMotion(krill, i);
                    krill.Position = krill.Position.Add(krill.MotionInduced.Add(krill.FoodInducing).Add(krill.Diffusion).Multiply(0.002*_dimensions*(_upperBound-_lowerBound)));
                    await FineTune(krill, bestKrill, inertiaAtraction);
                    for (int j =0; j < _dimensions; j++)
                    {
                        if (krill.Position[j] < _lowerBound)
                            krill.Position[j] = _lowerBound;
                        else if (krill.Position[j] > _upperBound)
                            krill.Position[j] = _upperBound;
                    }
                    

                    krill.Fitness = BenchmarkFunctions.InvokeFunction(FunctionName, krill.Position);
                }
                population = population.OrderBy(x => x.Fitness);
                bestKrill = (Krill)population.First().Clone();
                worstKrill = (Krill)population.Last().Clone();
                bestPerIteration.Add(bestKrill.Fitness);
            }
            var krillBestPos = new double[_dimensions];
            bestKrill.Position.CopyTo(krillBestPos,0);
            var result = new Result()
            {
                BestFit = bestKrill.Fitness,
                BestFitArguments = krillBestPos,
                MeanFit = population.Select(x => x.Fitness).Average(),
                BestFitsPerIteration = bestPerIteration
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
            if (hyperParameters.ContainsKey(KrillHerdConstants.dimensions))
                _dimensions = (int)hyperParameters[KrillHerdConstants.dimensions];
            if (hyperParameters.ContainsKey(KrillHerdConstants.maxInductionSpeed))
                _maxInductionSpeed = hyperParameters[KrillHerdConstants.maxInductionSpeed];
            if (hyperParameters.ContainsKey(KrillHerdConstants.maxDiffusionSpeed))
                _maxDiffusionSpeed = hyperParameters[KrillHerdConstants.maxDiffusionSpeed];
            if (hyperParameters.ContainsKey(KrillHerdConstants.speedToEmbraceFood))
                _speedToEmbraceFood = hyperParameters[KrillHerdConstants.speedToEmbraceFood];
            if(hyperParameters.ContainsKey(KrillHerdConstants.alpha))
                _alpha = hyperParameters[KrillHerdConstants.alpha];
            if (hyperParameters.ContainsKey(KrillHerdConstants.numberOfGenerations))
                _numberOfGenerations = (int)hyperParameters[KrillHerdConstants.numberOfGenerations];
            if (hyperParameters.ContainsKey(KrillHerdConstants.populationSize))
                _populationSize = (int)hyperParameters[KrillHerdConstants.populationSize];
        }

        private double ChaoticTentMap(double x) => 0.8 *(1-2*Math.Abs(x - 1/2));

        private async Task<IEnumerable<Krill>> InitializePopulation()
        {
            var population = new List<Krill>();
            for (int i = 0; i < _populationSize; i++)
            {
                var position = new double[_dimensions];
                var velocity = new double[_dimensions];
                var food = new double[_dimensions];
                var diffusion = new double[_dimensions];
                for (int j = 0; j < _dimensions; j++)
                {
                    position[j] = _lowerBound + _random.NextDouble() * (_upperBound - _lowerBound);
                    velocity[j] = _maxInductionSpeed;
                    food[j] = _speedToEmbraceFood;
                    diffusion[j] = _maxDiffusionSpeed;
                }
                var krill = new Krill() { 
                    Position =position,
                    MotionInduced = velocity,
                    FoodInducing = food,
                    Diffusion = diffusion,
                    Fitness = BenchmarkFunctions.InvokeFunction(FunctionName, position)
                };
                population.Add(krill);
            }
            return population;
        }

        private async Task InducedMotion(Krill individual,IEnumerable<Krill> population, int iteration, double weight, Krill bestKrill, Krill worstKrill)
        {
            var KrillList = population.ToList();
            var alpha = await GetAlpha(individual, KrillList, worstKrill, bestKrill, iteration);
            individual.MotionInduced = alpha.Multiply(_maxInductionSpeed).Add(individual.MotionInduced.Multiply(weight));
        }

        private async Task FoodMotion(Krill individual,IEnumerable<Krill> population, int iteration, double weight, Krill bestKrill, Krill worstKrill, double[] foodPosition)
        {
           var betaBest = (await X_i_j(individual.Position, bestKrill.Position)).Multiply(await K_i_j(individual.Fitness, bestKrill.Fitness, worstKrill.Fitness, bestKrill.Fitness));
            double C_food = 2*(1- (iteration / _numberOfGenerations));
            double K_i_food = await K_i_j(individual.Fitness, BenchmarkFunctions.InvokeFunction(FunctionName, foodPosition), worstKrill.Fitness, bestKrill.Fitness);
            var food = (await X_i_j(individual.Position, foodPosition)).Multiply(K_i_food * C_food);
            var beta = food.Add(betaBest);

            individual.FoodInducing = beta.Multiply(_speedToEmbraceFood).Add(individual.FoodInducing.Multiply(weight));
        }

        private async Task DiffusionMotion(Krill krill, int iteration)
        {
           var epsilon = 2*_random.NextDouble() -1;
           for (int i = 0; i < _dimensions; i++)
           {
                krill.Diffusion[i] = _maxDiffusionSpeed*(1-iteration/_numberOfGenerations) * epsilon;
           }
        }

        private async Task<double[]> CreateFood(IEnumerable<Krill> population)
        {
            double sum = 0;

            var positionOfFood = new double[_dimensions];

            foreach (var krill in population)
            {
                positionOfFood = positionOfFood.Add(krill.Position.Divide(krill.Fitness + 0.00001));
                sum += (1 / (krill.Fitness + 0.000001));
            }


            positionOfFood = positionOfFood.Divide(sum);
            for(int i = 0; i < _dimensions; i++)
            {
                if (positionOfFood[i] < _lowerBound || positionOfFood[i] > _upperBound)
                    positionOfFood[i] = _lowerBound + (_upperBound-_lowerBound)*_random.NextDouble();
                else
                    positionOfFood[i] = positionOfFood[i];
            }

            return positionOfFood;
        }

        private async Task<double[]> GetAlpha(Krill krill, IEnumerable<Krill> population, Krill worst, Krill best, int iteration)
        {
            var alphaLocal = new double[_dimensions];
            Array.ForEach(alphaLocal, x => x = 0);
            var neigbours = await GetNeighbourhood(krill, population);
            foreach (var neighbour in neigbours) 
            {
                if (neighbour != krill)
                {
                    var K_i_j_value = await K_i_j(krill.Fitness, neighbour.Fitness, worst.Fitness, best.Fitness);
                    alphaLocal = alphaLocal.Add((await X_i_j(krill.Position, neighbour.Position)).Multiply(K_i_j_value));
                }
            }

            var K_i_best = await K_i_j(krill.Fitness, best.Fitness, worst.Fitness, best.Fitness);
            var X_i_best = await X_i_j(krill.Position, best.Position);
            var C_best = 2 * (_random.NextDouble() + (iteration / _numberOfGenerations));

            var alphaTarget = X_i_best.Multiply(C_best * K_i_best);
            return alphaLocal.Add(alphaTarget);
        }

        public async Task<double[]> X_i_j(double[] krillCoordinates, double[] neihghbourCoordinates) =>
            krillCoordinates.Substract(neihghbourCoordinates).Divide(await SqrtSumOfSqr(krillCoordinates, neihghbourCoordinates) + 0.0001);

        public async Task<double> K_i_j(double krillFitness, double neihghbourFitness, double worstFitness, double bestFitness) =>
            (krillFitness - neihghbourFitness) / (worstFitness - bestFitness + 0.00001);

        public async Task FineTune(Krill indivudual, Krill best, double beta)
        {
            for (int i = 0; i < _dimensions; i++)
            {
                var r = _random.NextDouble();
                indivudual.Position[i] = (1 - beta) * indivudual.Position[i] + beta * best.Position[i] + _alpha * r;
            }
        }

        public async Task<double> SqrtSumOfSqr(double[] vector1, double[] vector2)
        {
            double distance = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                distance += Math.Pow(vector1[i] - vector2[i], 2);
            }

            return Math.Sqrt(distance);
        }

        public async Task<IEnumerable<Krill>> GetNeighbourhood(Krill krill, IEnumerable<Krill> population)
        {
            double sensingDistance = await SensingDistance(krill, population);

            var neighbours = new List<Krill>();

            foreach (var neighbour in population)
            {
                if (neighbour != krill)
                {
                    double distance = Distance.Euclidean(krill.Position, neighbour.Position);

                    if (distance < sensingDistance)
                    {
                        neighbours.Add(neighbour);
                    }
                }
            }

            return neighbours;
        }

        private async Task<double> SensingDistance(Krill krill, IEnumerable<Krill> population)
        {
            double N = population.Count();

            double firstPart = (1 / (5 * N));
            double secondPart = 0;

            foreach (var otherKrill in population)
            {
                secondPart += await SqrtSumOfSqr(krill.Position, otherKrill.Position);
            }

            double result = firstPart * secondPart;
            return Math.Round(result, 2);
        }
    }
}
