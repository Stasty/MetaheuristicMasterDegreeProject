namespace MetaheuristicsTester.Constants
{
    public class FunctionsConstants
    {
        public static Dictionary<string, (double, double)> FunctionsBoundaries => new Dictionary<string, (double, double)>()
        {
            {"Rastrigin", (-5.12, 5.12)},
            {"Sphere", (-5.12, 5.12)},
            {"Ackley", (-35, 35) },
            {"BentCigar", (-100, 100) },
            {"Griewank", (-100, 100) },
            {"Step", (-100, 100) },
            {"Zakharov", (-5,10) },
            {"Katsuura", (0,100) },
            {"SchafterF6", (-100,100)}
        };
    }
}
