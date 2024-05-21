namespace MetaheuristicsTester.Extensions
{
    public static class ArrayExtension
    {
        public static double[] Add(this double[] array, double[] value) => array.Zip(value, (x, y) => x + y).ToArray();
        public static double[] Substract(this double[] array, double[] value) => array.Zip(value, (x, y) => x - y).ToArray();
        public static double[] Multiply(this double[] array, double[] value) => array.Zip(value, (x, y) => x * y).ToArray();
        public static double[] Divide(this double[] array, double[] value) => array.Zip(value, (x, y) => x / y).ToArray();
        public static double[] Add(this double[] array, double value) => array.Select(x => x + value).ToArray();
        public static double[] Substract(this double[] array, double value) => array.Select( x=>x-value).ToArray();
        public static double[] Multiply(this double[] array, double value) => array.Select(x => x * value).ToArray();
        public static double[] Divide(this double[] array, double value) => array.Select(x => x / value).ToArray();
    }
}
