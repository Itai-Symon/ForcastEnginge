using ForecastEngine;

// ── Shared historical data ────────────────────────────────────────────────────
// Simulates a team's completed features over ~6 months.
var history = new List<Feature>
{
    new("F01",  5, new DateOnly(2024,  1,  5)),
    new("F02",  3, new DateOnly(2024,  1,  9)),
    new("F03",  8, new DateOnly(2024,  1, 16)),
    new("F04",  0, new DateOnly(2024,  1, 23)),
    new("F05",  6, new DateOnly(2024,  1, 30)),
    new("F06",  4, new DateOnly(2024,  2,  6)),
    new("F07", 10, new DateOnly(2024,  2, 13)),
    new("F08",  2, new DateOnly(2024,  2, 20)),
    new("F09",  7, new DateOnly(2024,  2, 27)),
    new("F10",  5, new DateOnly(2024,  3,  5)),
    new("F11",  0, new DateOnly(2024,  3, 12)),
    new("F12",  9, new DateOnly(2024,  3, 19)),
    new("F13",  3, new DateOnly(2024,  3, 26)),
    new("F14",  6, new DateOnly(2024,  4,  2)),
    new("F15",  8, new DateOnly(2024,  4,  9)),
    new("F16",  4, new DateOnly(2024,  4, 16)),
    new("F17",  5, new DateOnly(2024,  4, 23)),
    new("F18", 11, new DateOnly(2024,  4, 30)),
    new("F19",  2, new DateOnly(2024,  5,  7)),
    new("F20",  7, new DateOnly(2024,  5, 14)),
    new("F21",  6, new DateOnly(2024,  5, 21)),
    new("F22",  4, new DateOnly(2024,  5, 28)),
    new("F23",  9, new DateOnly(2024,  6,  4)),
    new("F24",  5, new DateOnly(2024,  6, 11)),
};

var startDate = new DateOnly(2024, 7, 1);
var engine = new MonteCarloForecastEngine(iterations: 10_000, seed: 42);

RunScenario(
    title:                 "Scenario 1 — Comfortable deadline",
    description:           "40 points remaining, 13 weeks to target. Team expects to finish comfortably.",
    remainingStoryPoints:  40,
    targetDate:            new DateOnly(2024, 9, 30));

RunScenario(
    title:                 "Scenario 2 — Borderline deadline",
    description:           "40 points remaining, only 8 weeks to target. It's going to be close.",
    remainingStoryPoints:  40,
    targetDate:            new DateOnly(2024, 8, 26));

RunScenario(
    title:                 "Scenario 3 — Aggressive deadline",
    description:           "70 points remaining, 8 weeks to target. Management is pushing hard.",
    remainingStoryPoints:  70,
    targetDate:            new DateOnly(2024, 8, 26));

// ── Helpers ───────────────────────────────────────────────────────────────────

void RunScenario(string title, string description, int remainingStoryPoints, DateOnly targetDate)
{
    Console.WriteLine(new string('═', 55));
    Console.WriteLine(title);
    Console.WriteLine(description);
    Console.WriteLine(new string('─', 55));

    var result = engine.Forecast(history, remainingStoryPoints, startDate, targetDate);

    Console.WriteLine($"Remaining story points : {remainingStoryPoints}");
    Console.WriteLine($"Start date             : {startDate}");
    Console.WriteLine($"Target date            : {targetDate}");
    Console.WriteLine();
    Console.WriteLine($"Probability of completion : {result.ProbabilityOfCompletion:P1}");
    Console.WriteLine($"P50 completion date       : {result.P50CompletionDate}");
    Console.WriteLine($"P85 completion date       : {result.P85CompletionDate}");
    Console.WriteLine($"P95 completion date       : {result.P95CompletionDate}");
    Console.WriteLine();

    PrintHistogram(result.SimulationWeeks, targetDate, startDate);
    Console.WriteLine();
}

void PrintHistogram(IReadOnlyList<int> simulationWeeks, DateOnly targetDate, DateOnly startDate)
{
    if (simulationWeeks.Count == 0)
    {
        Console.WriteLine("No simulations completed — throughput history may be all zeros.");
        return;
    }

    Console.WriteLine("Simulation distribution (weeks to complete):");

    var counts   = simulationWeeks.GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());
    var minWeek  = counts.Keys.Min();
    var maxWeek  = counts.Keys.Max();
    var maxCount = counts.Values.Max();
    var deadline = (targetDate.DayNumber - startDate.DayNumber) / 7;

    const int barWidth = 36;

    for (var week = minWeek; week <= maxWeek; week++)
    {
        var count  = counts.GetValueOrDefault(week, 0);
        var bar    = new string('█', (int)((double)count / maxCount * barWidth));
        var pct    = 100.0 * count / simulationWeeks.Count;
        var marker = week == deadline ? " ← target" : "";
        Console.WriteLine($"{week,3} weeks | {bar,-36} {pct:0.0}%{marker}");
    }
}
