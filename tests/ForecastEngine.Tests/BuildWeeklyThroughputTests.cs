using Xunit;
using ForecastEngine;

namespace ForecastEngine.Tests;

public class BuildWeeklyThroughputTests
{
    private static DateOnly D(int daysFromEpoch) =>
        new DateOnly(2024, 1, 1).AddDays(daysFromEpoch);

    [Fact]
    public void SingleFeature_ProducesOneWeekWithItsPoints()
    {
        var features = new[] { new Feature("A", 5, D(0)) };

        var result = MonteCarloForecastEngine.BuildWeeklyThroughput(features);

        Assert.Equal([5], result);
    }

    [Fact]
    public void TwoFeaturesInSameWeek_PointsAreSummed()
    {
        var features = new[]
        {
            new Feature("A", 3, D(0)),
            new Feature("B", 7, D(6)),
        };

        var result = MonteCarloForecastEngine.BuildWeeklyThroughput(features);

        Assert.Equal([10], result);
    }

    [Fact]
    public void FeaturesInConsecutiveWeeks_EachWeekHasCorrectSum()
    {
        var features = new[]
        {
            new Feature("A", 5, D(0)),
            new Feature("B", 8, D(7)),
        };

        var result = MonteCarloForecastEngine.BuildWeeklyThroughput(features);

        Assert.Equal([5, 8], result);
    }

    [Fact]
    public void GapBetweenFeatures_EmptyWeeksAreZero()
    {
        var features = new[]
        {
            new Feature("A", 4, D(0)),
            new Feature("B", 6, D(21)),
        };

        var result = MonteCarloForecastEngine.BuildWeeklyThroughput(features);

        Assert.Equal([4, 0, 0, 6], result);
    }

    [Fact]
    public void WeekIndexIsAnchoredToEarliestFeature_NotDayZero()
    {
        // Both features start on day 100; the anchor should be day 100, not day 0.
        var features = new[]
        {
            new Feature("A", 5, D(100)),
            new Feature("B", 3, D(107)),
        };

        var result = MonteCarloForecastEngine.BuildWeeklyThroughput(features);

        Assert.Equal([5, 3], result);
    }

    [Fact]
    public void FeatureWithZeroPoints_IncludedAsZero()
    {
        var features = new[]
        {
            new Feature("A", 0, D(0)),
            new Feature("B", 5, D(7)),
        };

        var result = MonteCarloForecastEngine.BuildWeeklyThroughput(features);

        Assert.Equal([0, 5], result);
    }
}
