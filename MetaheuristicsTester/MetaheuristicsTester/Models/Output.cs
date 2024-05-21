namespace MetaheuristicsTester.Models
{
    public class Output
    {
        public string AlgorithmName { get; set; }
        public string FunctionName { get; set; }
        public IEnumerable<Result> Results { get; set; }
    }
}
