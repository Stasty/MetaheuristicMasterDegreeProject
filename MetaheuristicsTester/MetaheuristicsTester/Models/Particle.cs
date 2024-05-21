namespace MetaheuristicsTester.Models
{
    public class Particle
    {
        public double[] Positions { get; set; }
        public double[] PBest { get; set; }
        public double[] Velocity { get; set; }

        public Particle(int dimension, double _lowerBound, double _upperBound)
        {
            var random = new Random();
            Positions = new double[dimension];
            PBest = new double[dimension];
            Velocity = new double[dimension];
            for (int i = 0; i < dimension; i++)
            {
                Velocity[i] = 0;
                PBest[i] = Positions[i] = random.NextDouble() * (_upperBound - _lowerBound) + _lowerBound;
            }
        }
    }
}
