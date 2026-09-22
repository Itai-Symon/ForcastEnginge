using Xunit;
using ForecastEngine;

namespace ForecastEngine.Tests;

public class RunSimulationsTests
{
    [Fact]
    public void ZeroRemainingPoints_AllSimulationsFinishInZeroWeeks()
    {
        var throughput = new[] { 5, 10, 3, 8 };

        var results = MonteCarloForecastEngine.RunSimulations(throughput, remainingStoryPoints: 0, iterations: 100, seed: 42);

        Assert.Equal(100, results.Length);
        Assert.All(results, w => Assert.Equal(0, w));
    }

    [Fact]
    public void HighThroughput_AllSimulationsFinishInOneWeek()
    {
        // throughput of 100 per week always covers 1 remaining point in a single draw
        var throughput = new[] { 100 };

        var results = MonteCarloForecastEngine.RunSimulations(throughput, remainingStoryPoints: 1, iterations: 50, seed: 42);

        Assert.Equal(50, results.Length);
        Assert.All(results, w => Assert.Equal(1, w));
    }

    [Fact]
    public void AllZeroThroughput_SafetyCapTriggered_NoSimulationsFinish()
    {
        var throughput = new[] { 0, 0, 0, 0 };

        var results = MonteCarloForecastEngine.RunSimulations(throughput, remainingStoryPoints: 10, iterations: 50, seed: 42);

        Assert.Empty(results);
    }

    [Fact]
    public void SameSeed_ProducesSameResults()
    {
        var throughput = new[] { 3, 7, 0, 5, 8, 2 };

        var first  = MonteCarloForecastEngine.RunSimulations(throughput, remainingStoryPoints: 20, iterations: 200, seed: 123);
        var second = MonteCarloForecastEngine.RunSimulations(throughput, remainingStoryPoints: 20, iterations: 200, seed: 123);

        Assert.Equal(first, second);
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentResults()
    {
        var throughput = new[] { 3, 7, 0, 5, 8, 2 };

        var first  = MonteCarloForecastEngine.RunSimulations(throughput, remainingStoryPoints: 20, iterations: 200, seed: 1);
        var second = MonteCarloForecastEngine.RunSimulations(throughput, remainingStoryPoints: 20, iterations: 200, seed: 2);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void ResultCount_NeverExceedsIterations()
    {
        var throughput = new[] { 5, 0, 3, 8 };

        var results = MonteCarloForecastEngine.RunSimulations(throughput, remainingStoryPoints: 15, iterations: 100, seed: 42);

        Assert.True(results.Length <= 100);
    }
}
