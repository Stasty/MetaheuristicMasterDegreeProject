namespace MetaheuristicsTester.Models
{
    public record Result
    {
        public double BestFit { get; set; }
        public double MeanFit { get; set; }

        public IEnumerable<double> BestFitsPerIteration { get; set; }
        
        public IEnumerable<double> BestFitArguments { get; set; }

    }
}
