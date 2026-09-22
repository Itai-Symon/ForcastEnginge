namespace ForecastEngine;

public interface IForecastEngine
{
    ForecastResult Forecast(
        IReadOnlyCollection<Feature> historicalFeatures,
        int remainingStoryPoints,
        DateOnly startDate,
        DateOnly targetDate);
}
