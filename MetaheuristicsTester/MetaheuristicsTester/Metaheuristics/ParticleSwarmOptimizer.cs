using MetaheuristicsTester.Constants;
using MetaheuristicsTester.Functions;
using MetaheuristicsTester.Models;

namespace MetaheuristicsTester.Metaheuristics
{
    public class ParticleSwarmOptimizer : IExperiment
    {
        private double _lowerBound=0;
        private double _upperBound=0;

        private double _a = 0.003;
        private double _b = 0;
        private double _c = 0.5;
        private double _d = 1.5;
        private double _minWeigth = 0.5;
        private double _maxWeigth = 1;

        private int _dimensions;
        private int _numberOfGenerations;
        private int _populationSize;

        private readonly Random _random = new Random();

        public string Name => nameof(ParticleSwarmOptimizer);

        public string FunctionName { get; set; }

        public async Task<Result> ProcideExperiment()
        {
            var result = new Result();
            var population = await InitializePopulation();
            var bestFitPerIteration = new List<double>();
            Particle bestFitCurrnet;
            for (int i = 0; i < _numberOfGenerations; i++)
            {
                bestFitCurrnet = await UpdateBests(population);
                bestFitPerIteration.Add(BenchmarkFunctions.InvokeFunction(FunctionName, bestFitCurrnet.PBest));
                var weight = _minWeigth - (_minWeigth - _maxWeigth) * i / _numberOfGenerations;
                foreach(var particle in population)
                {
                    for(int k = 0; k < _dimensions; k++)
                    {
                        var r1 = _random.NextDouble();
                        var r2 = _random.NextDouble();
                        var cpi = CalculateCoeficients(particle.PBest[k], particle.Positions[k]);
                        var cgi = CalculateCoeficients(bestFitCurrnet.PBest[k], particle.Positions[k]);
                        particle.Velocity[k] = weight * particle.Velocity[k] + cpi * (particle.PBest[k] - particle.Positions[k]) + cgi * (bestFitCurrnet.PBest[k] - particle.Positions[k]);
                        particle.Positions[k] += particle.Velocity[k];
                    }
                }
            }
            bestFitCurrnet = await UpdateBests(population);
            bestFitPerIteration.Add(BenchmarkFunctions.InvokeFunction(FunctionName, bestFitCurrnet.PBest));
            result.BestFitArguments = bestFitCurrnet.PBest;
            result.BestFit = BenchmarkFunctions.InvokeFunction(FunctionName, result.BestFitArguments);
            result.BestFitsPerIteration = bestFitPerIteration;
            result.MeanFit = population.Select(x => BenchmarkFunctions.InvokeFunction(FunctionName, x.PBest)).Average();
            return result;
        }

        public void SetBoundaries(double lowerBound, double upperBound)
        {
            _lowerBound = lowerBound;
            _upperBound = upperBound;
        }

        public void SetHyperParameters(IDictionary<string, double> hyperParameters)
        {
            if (hyperParameters.ContainsKey(ParticleSwarmOptimizerConstants.dimensions))
                _dimensions = (int)hyperParameters[ParticleSwarmOptimizerConstants.dimensions];
            if (hyperParameters.ContainsKey(ParticleSwarmOptimizerConstants.a))
                _a = hyperParameters[ParticleSwarmOptimizerConstants.a];
            if (hyperParameters.ContainsKey(ParticleSwarmOptimizerConstants.b))
                _b = hyperParameters[ParticleSwarmOptimizerConstants.b];
            if (hyperParameters.ContainsKey(ParticleSwarmOptimizerConstants.c))
                _c = hyperParameters[ParticleSwarmOptimizerConstants.c];
            if (hyperParameters.ContainsKey(ParticleSwarmOptimizerConstants.d))
                _d = hyperParameters[ParticleSwarmOptimizerConstants.d];
            if (hyperParameters.ContainsKey(ParticleSwarmOptimizerConstants.numberOfGenerations))
                _numberOfGenerations = (int)hyperParameters[ParticleSwarmOptimizerConstants.numberOfGenerations];
            if (hyperParameters.ContainsKey(ParticleSwarmOptimizerConstants.populationSize))
                _populationSize = (int)hyperParameters[ParticleSwarmOptimizerConstants.populationSize];
        }


        public async Task<IEnumerable<Particle>> InitializePopulation()
        {
            var population = new List<Particle>();
            for (int i = 0; i < _populationSize; i++)
                population.Add(new Particle(_dimensions, _lowerBound, _upperBound));
            return population;
        }

        public async Task<Particle> UpdateBests(IEnumerable<Particle> population)
        {
            foreach (var particle in population)
            {
                var pBestFit = BenchmarkFunctions.InvokeFunction(FunctionName, particle.PBest);
                var particleFit = BenchmarkFunctions.InvokeFunction(FunctionName, particle.Positions);
                if(pBestFit > particleFit)
                    particle.Positions.CopyTo(particle.PBest,0);
            }
                
            return population.OrderBy(x => BenchmarkFunctions.InvokeFunction(FunctionName, x.PBest)).First();
        }

        private double CalculateCoeficients(double pos1, double pos2)
        {
            var g = pos1 - pos2;
            return _d+ _b / 1+ Math.Exp(-_a * (g - _c));
        }
    }
}
