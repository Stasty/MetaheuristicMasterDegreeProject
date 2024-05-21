namespace MetaheuristicsTester.Models
{
    public class GreyWolfIndividual
    {
        private readonly Random _random;

        public IEnumerable<double> Positions { get; set; }
        public double Fitness { get; set; }

        public GreyWolfIndividual(int dimensions, double lowerBound, double upperBound, int seed)
        { 
            _random = new Random(seed);
            var position = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                position[i] = _random.NextDouble() * (upperBound - lowerBound) + lowerBound;
            }
            Positions = position;
        }

        public GreyWolfIndividual(GreyWolfIndividual individual)
        {
            _random = individual._random;
            var position = new List<double>();
            foreach (var pos in individual.Positions)
                position.Add(pos);
            Positions = position;
            Fitness = individual.Fitness;

        }
    }
}
