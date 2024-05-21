using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MetaheuristicsTester.Functions
{
    public class BenchmarkFunctions
    {
        public double Sphere(IEnumerable<double> inputs)
        {
            if(inputs.Any(x => Math.Abs(x) > 5.12))
                return double.MaxValue;

            var sum = 0.0;
            foreach(var input in inputs)
                sum += Math.Pow(input,2);
            return sum;
        }

        public double Ackley(IEnumerable<double> inputs)
        {
            if (inputs.Any(x => Math.Abs(x) > 35))
                return double.MaxValue;

            var sum = 0.0;
            var sum2 = 0.0;
            var inputsList = inputs.ToList();
            for (var i = 0; i < inputs.Count(); i++)
            {
                sum += Math.Pow(inputsList[i], 2);
                sum2 += Math.Cos(2 * Math.PI * inputsList[i]);
            }
            var d = (1 / inputs.Count());

            return -20 * Math.Exp(-0.02 * Math.Sqrt(d* sum)) - Math.Exp(d * sum2) + 20 + Math.E;
        }

        public double Rastrigin(IEnumerable<double> inputs)
        {
            if (inputs.Any(x => Math.Abs(x) > 5.12))
                return double.MaxValue;

            var sum = 0.0;
            foreach (var input in inputs)
                sum += Math.Pow(input, 2)-10*Math.Cos(2 * Math.PI*input);
            return sum + 10 * inputs.Count();
        }

        public double BentCigar(IEnumerable<double> inputs)
        {
            if (inputs.Any(x => Math.Abs(x) > 100))
                return double.MaxValue;

            var sum = 0.0;
            foreach (var input in inputs.Skip(1))
                sum += Math.Pow(input,2);
            return Math.Pow(10, 6) * sum + Math.Pow(inputs.First(), 2);
        }

        public double Griewank(IEnumerable<double> inputs)
        {
            if (inputs.Any(x => Math.Abs(x) > 100))
                return double.MaxValue;

            var sum = 0.0;
            var product = 1.0;
            var inputsList = inputs.ToList();
            for (var i = 0; i < inputsList.Count(); i++)
            {
                sum += Math.Pow(inputsList[i], 2);
                product *= Math.Cos(inputsList[i]/Math.Sqrt(i));
            }
            return (1/4000) * sum + product + 1;
        }
        public double Step(IEnumerable<double> inputs)
        {
            if (inputs.Any(x => Math.Abs(x) > 100))
                return double.MaxValue;

            var sum = 0.0;
            foreach (var input in inputs)
                sum += Math.Floor(Math.Pow(input, 2));
            return sum;
        }

        public double Zakharov(IEnumerable<double> inputs)
        {
            if (inputs.Any(x => x < -5 || x > 10))
                return double.MaxValue;

            var sum = 0.0;
            var sum2 = 0.0;
            var inputsList = inputs.ToList();
            for (var i = 0; i < inputsList.Count() - 1; i++)
            {
                sum += Math.Pow(inputsList[i], 2);
                sum2 += i * Math.Pow(inputsList[i], 2);
            }
            return sum+Math.Pow(0.5*sum2,2)+ Math.Pow(0.5 * sum2, 4);
        }

        public double Katsuura(IEnumerable<double> inputs)
        {
            if (inputs.Any(x => x<0 || x > 100))
                return double.MaxValue;

            var dimensions = inputs.Count();
            var product = 1.0;
            var inputsList = inputs.ToList();
            for (var i = 0; i < dimensions; i++)
            {
                var sum = 0.0;
                for (int j = 1; j < 33; j++)
                {
                    var component = Math.Round(Math.Pow(2, j) * inputsList[i]) / Math.Pow(2, j);
                    sum += component;
                }
                product *= 1 + (i+1) * sum;
            }
            return product;
        }

        public double SchafterF6(IEnumerable<double> inputs)
        {
            if (inputs.Any(x => Math.Abs(x)>100))
                return double.MaxValue;

            var sum = 0.0;
            var inputsList = inputs.ToList();
            for (var i = 0; i < inputs.Count() - 1; i++)
            {
                var xi2 = Math.Pow(inputsList[i], 2);
                var xii2 = Math.Pow(inputsList[i+1], 2);
                var top = Math.Pow(Math.Sin(Math.Sqrt(xi2+xii2)), 2) - 0.5;
                var bottom = Math.Pow(1 + 0.0001*(xi2 + xii2), 2);
                sum += 0.5 + top / bottom;
            }
            return sum;
        }

        public static double InvokeFunction(string name, IEnumerable<double> inputs)
        {
            var methods = typeof(BenchmarkFunctions).GetMethods(BindingFlags.Instance | BindingFlags.Public);
            methods = methods.Where(method => method.DeclaringType != typeof(object) && method.Name != "InvokeFunction").ToArray();
            foreach (var method in methods)
            {
                if (method.Name == name)
                    return (double)method.Invoke(new BenchmarkFunctions(), new[] {inputs});
            }
            return double.MaxValue;
        }
    }
}
