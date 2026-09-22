namespace ForecastEngine;

public class MonteCarloForecastEngine : IForecastEngine
{
    public const int MinimumHistoryWeeks = 4;

    private readonly int _iterations;
    private readonly int? _seed;

    public MonteCarloForecastEngine(int iterations = 10_000, int? seed = null)
    {
        _iterations = iterations;
        _seed = seed;
    }

    public ForecastResult Forecast(
        IReadOnlyCollection<Feature> historicalFeatures,
        int remainingStoryPoints,
        DateOnly startDate,
        DateOnly targetDate)
    {
        ValidateInputs(historicalFeatures, remainingStoryPoints, startDate, targetDate);

        throw new NotImplementedException();
    }

    private static void ValidateInputs(
        IReadOnlyCollection<Feature> historicalFeatures,
        int remainingStoryPoints,
        DateOnly startDate,
        DateOnly targetDate)
    {
        if (historicalFeatures is null)
            throw new ArgumentNullException(nameof(historicalFeatures));

        if (historicalFeatures.Count == 0)
            throw new ArgumentException("Historical features must not be empty.", nameof(historicalFeatures));

        if (historicalFeatures.Any(f => f is null))
            throw new ArgumentException("Historical features must not contain null entries.", nameof(historicalFeatures));

        if (historicalFeatures.Any(f => f.StoryPoints < 0))
            throw new ArgumentException("All Story Points in history must be non-negative.", nameof(historicalFeatures));

        if (remainingStoryPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(remainingStoryPoints), "Remaining Story Points must not be negative.");

        if (targetDate < startDate)
            throw new ArgumentException("Target date must be on or after start date.", nameof(targetDate));

        var earliest = historicalFeatures.Min(f => f.CompletionDate);
        var latest = historicalFeatures.Max(f => f.CompletionDate);
        var weekCount = (latest.DayNumber - earliest.DayNumber) / 7 + 1;

        if (weekCount < MinimumHistoryWeeks)
            throw new ArgumentException(
                $"At least {MinimumHistoryWeeks} weeks of historical data are required, " +
                $"but the provided history spans only {weekCount} week(s).",
                nameof(historicalFeatures));
    }
}
