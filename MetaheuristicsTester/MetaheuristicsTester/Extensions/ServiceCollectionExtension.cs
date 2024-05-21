using MetaheuristicsTester.Metaheuristics;
using MetaheuristicsTester.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace MetaheuristicsTester.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddTransient<IExperiment, AntColonyOptimizer>();
            services.AddTransient<IExperiment, ArtificialBeeColony>();
            services.AddTransient<IExperiment, GeneticAlgorithm>();
            services.AddTransient<IExperiment, ParticleSwarmOptimizer>();
            services.AddTransient<IExperiment, GreyWolfAlgorithm>();
            services.AddTransient<IExperiment, KrillHerd>();
            services.AddTransient<IExperiment, WhaleOptimizationAlgorithm>();
            services.AddScoped(s => new Func<string, IExperiment>(x => s.GetServices<IExperiment>().FirstOrDefault(y => y.Name == x)));
            services.AddTransient<IExperimentRunner, ExperimentRunner>();
            services.AddScoped<IFileService, FileService>();
            return services;
        }
    }
}
