using System.Numerics;

namespace MetaheuristicsTester.Models
{
    public class Krill: ICloneable
    {
        public double[] Position { get; set; }
        public double[] MotionInduced { get; set; }
        public double[] FoodInducing { get; set; }
        public double[] Diffusion { get; set; }

        public double Fitness { get; set; }

        public object Clone()
        {
            var clone = new Krill();
            clone.Position = new double[Position.Length];
            Position.CopyTo(clone.Position, 0);
            clone.MotionInduced = new double[MotionInduced.Length];
            Position.CopyTo(clone.MotionInduced, 0);
            clone.FoodInducing = new double[FoodInducing.Length];
            Position.CopyTo(clone.FoodInducing, 0);
            clone.Diffusion = new double[Diffusion.Length];
            Position.CopyTo(clone.Diffusion, 0);
            clone.Fitness = Fitness;
            return clone;
        }
    }
}
