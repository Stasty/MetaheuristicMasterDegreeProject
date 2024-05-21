using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaheuristicsTester.Extensions;
using MetaheuristicsTester.Services;
using MetaheuristicsTester.Models;
using System.Diagnostics;
using MetaheuristicsTester.Functions;


var serviceProvider = new ServiceCollection().ConfigureServices().BuildServiceProvider();

string inputFileName = string.Empty;
string outputFileName = string.Empty;

for (int i = 0; i< args.Length; i++)
{
    if(args[i] == "-i" && i + 1 < args.Length)
        inputFileName = args[i + 1];
    else if (args[i] == "-o" && i + 1 < args.Length)
        outputFileName = args[i + 1];
}

if (string.IsNullOrEmpty(inputFileName) || string.IsNullOrEmpty(outputFileName))
    return;

//var shortList = new List<double>();
//for (int i = 0; i < 30; i++)
//{
//    shortList.Add(1.0);
//}

//Console.WriteLine(BenchmarkFunctions.InvokeFunction("Katsuura", shortList));

var fileService = serviceProvider.GetService<IFileService>();

var experiments = fileService.ReadFile(inputFileName);

var outputList = new List<Output>();
var timer = Stopwatch.StartNew();

foreach (var experiment in experiments)
{
    Console.WriteLine($"Running experiment: {experiment.AlgorithmName} - {experiment.FunctionName} start: {timer.ElapsedMilliseconds}");
    ExperimentRunner.AlgorithmName = experiment.AlgorithmName;
    var experimentRunner = serviceProvider.GetService<IExperimentRunner>();
    experimentRunner.experimentParameters = experiment;
    var output = await experimentRunner.Run();
    outputList.Add(output);
    Console.WriteLine($"Finishing experiment: {experiment.AlgorithmName} - {experiment.FunctionName} start: {timer.ElapsedMilliseconds}");
}

fileService.WriteFile(outputFileName, outputList);


