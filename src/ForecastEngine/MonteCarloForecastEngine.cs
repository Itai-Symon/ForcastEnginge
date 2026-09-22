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

        var throughput = BuildWeeklyThroughput(historicalFeatures);
        var weeksToComplete = RunSimulations(throughput, remainingStoryPoints, _iterations, _seed);

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

    internal static int[] RunSimulations(int[] throughput, int remainingStoryPoints, int iterations, int? seed)
    {
        const int safetyCap = 10_000;
        var random = seed.HasValue ? new Random(seed.Value) : new Random();
        var results = new List<int>(iterations);

        for (var i = 0; i < iterations; i++)
        {
            var accumulated = 0;
            var weeks = 0;

            while (accumulated < remainingStoryPoints && weeks < safetyCap)
            {
                accumulated += throughput[random.Next(throughput.Length)];
                weeks++;
            }

            if (accumulated >= remainingStoryPoints)
                results.Add(weeks);
        }

        return [.. results];
    }

    internal static int[] BuildWeeklyThroughput(IReadOnlyCollection<Feature> features)
    {
        var earliest = features.Min(f => f.CompletionDate);

        var buckets = new Dictionary<int, int>();
        foreach (var feature in features)
        {
            var weekIndex = (feature.CompletionDate.DayNumber - earliest.DayNumber) / 7;
            buckets.TryGetValue(weekIndex, out var current);
            buckets[weekIndex] = current + feature.StoryPoints;
        }

        var maxWeek = buckets.Keys.Max();
        var throughput = new int[maxWeek + 1];
        foreach (var (weekIndex, points) in buckets)
            throughput[weekIndex] = points;

        return throughput;
    }
}
