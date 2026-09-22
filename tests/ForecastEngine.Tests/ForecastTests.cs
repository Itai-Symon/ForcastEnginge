using Xunit;
using ForecastEngine;

namespace ForecastEngine.Tests;

public class ForecastTests
{
    // Creates features with a fixed 5-point throughput spread across `weeks` weeks.
    private static List<Feature> CreateHistory(int weeks = 8)
    {
        var start = new DateOnly(2024, 1, 1);
        return Enumerable.Range(0, weeks)
            .Select(w => new Feature($"F{w}", 5, start.AddDays(w * 7)))
            .ToList();
    }

    private static readonly DateOnly Start  = new(2025, 1, 1);
    private static readonly DateOnly Target = new(2025, 6, 1);

    // ── Input validation ────────────────────────────────────────────────────

    [Fact]
    public void NullHistoricalFeatures_ThrowsArgumentNullException()
    {
        var engine = new MonteCarloForecastEngine(seed: 1);
        Assert.Throws<ArgumentNullException>(() =>
            engine.Forecast(null!, 10, Start, Target));
    }

    [Fact]
    public void EmptyHistoricalFeatures_ThrowsArgumentException()
    {
        var engine = new MonteCarloForecastEngine(seed: 1);
        Assert.Throws<ArgumentException>(() =>
            engine.Forecast([], 10, Start, Target));
    }

    [Fact]
    public void HistoryContainsNullEntry_ThrowsArgumentException()
    {
        var engine = new MonteCarloForecastEngine(seed: 1);
        var history = new List<Feature> { null! };
        Assert.Throws<ArgumentException>(() =>
            engine.Forecast(history, 10, Start, Target));
    }

    [Fact]
    public void NegativeStoryPointsInHistory_ThrowsArgumentException()
    {
        var engine = new MonteCarloForecastEngine(seed: 1);
        var history = new List<Feature> { new("A", -1, new DateOnly(2024, 1, 1)) };
        Assert.Throws<ArgumentException>(() =>
            engine.Forecast(history, 10, Start, Target));
    }

    [Fact]
    public void NegativeRemainingStoryPoints_ThrowsArgumentOutOfRangeException()
    {
        var engine = new MonteCarloForecastEngine(seed: 1);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            engine.Forecast(CreateHistory(), -1, Start, Target));
    }

    [Fact]
    public void TargetDateBeforeStartDate_ThrowsArgumentException()
    {
        var engine = new MonteCarloForecastEngine(seed: 1);
        Assert.Throws<ArgumentException>(() =>
            engine.Forecast(CreateHistory(), 10, Start, Start.AddDays(-1)));
    }

    [Fact]
    public void InsufficientHistory_ThrowsArgumentException()
    {
        var engine = new MonteCarloForecastEngine(seed: 1);
        // Only 2 weeks of history — below the 4-week minimum.
        var tooShort = CreateHistory(weeks: 2);
        Assert.Throws<ArgumentException>(() =>
            engine.Forecast(tooShort, 10, Start, Target));
    }

    // ── Result correctness ───────────────────────────────────────────────────

    [Fact]
    public void ZeroRemainingPoints_ProbabilityIsOne_AllDatesAreStartDate()
    {
        var engine = new MonteCarloForecastEngine(seed: 1);

        var result = engine.Forecast(CreateHistory(), remainingStoryPoints: 0, Start, Target);

        Assert.Equal(1.0, result.ProbabilityOfCompletion);
        Assert.Equal(Start, result.P50CompletionDate);
        Assert.Equal(Start, result.P85CompletionDate);
        Assert.Equal(Start, result.P95CompletionDate);
    }

    [Fact]
    public void GenerousTargetDate_HighProbability()
    {
        var engine = new MonteCarloForecastEngine(seed: 42);
        // 5 points remaining, history averages 5 points/week — target is 2 years away.
        var farTarget = Start.AddDays(365 * 2);

        var result = engine.Forecast(CreateHistory(), remainingStoryPoints: 5, Start, farTarget);

        Assert.True(result.ProbabilityOfCompletion > 0.95);
    }

    [Fact]
    public void VeryTightTargetDate_LowProbability()
    {
        var engine = new MonteCarloForecastEngine(seed: 42);
        // 100 points remaining, history averages 5 points/week — target is tomorrow.
        var tightTarget = Start.AddDays(1);

        var result = engine.Forecast(CreateHistory(), remainingStoryPoints: 100, Start, tightTarget);

        Assert.True(result.ProbabilityOfCompletion < 0.05);
    }

    [Fact]
    public void PercentileDates_AreInAscendingOrder()
    {
        var engine = new MonteCarloForecastEngine(seed: 42);

        var result = engine.Forecast(CreateHistory(), remainingStoryPoints: 20, Start, Target);

        Assert.True(result.P50CompletionDate <= result.P85CompletionDate);
        Assert.True(result.P85CompletionDate <= result.P95CompletionDate);
    }

}
