namespace ForecastEngine;

public record ForecastResult(
    double ProbabilityOfCompletion,
    DateOnly P50CompletionDate,
    DateOnly P85CompletionDate,
    DateOnly P95CompletionDate);
