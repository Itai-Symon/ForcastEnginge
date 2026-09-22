# ForecastEngine

A .NET 8 library that estimates the probability of a software release completing before a target date, using Monte Carlo simulation over historical team throughput data.

---

## How to Run the Sample

```bash
dotnet run --project samples/ForecastEngine.Sample
```

---

## Algorithm

### Overview

The engine uses **Monte Carlo simulation with bootstrap resampling**. Rather than assuming the team's velocity follows a specific statistical distribution (e.g. normal), it learns the distribution directly from historical data by resampling it.

### Step-by-step

1. **Build weekly throughput history** from the historical features: group completed features into weekly buckets (weeks are anchored to the earliest completion date in the history, not calendar/ISO weeks), and sum Story Points per bucket. Weeks with no completions are included as zero — a quiet week is real information about team pace, not missing data.

2. **Run N simulations** (default: 10,000). In each simulation:
   - Randomly draw weeks (with replacement) from the throughput history.
   - Accumulate Story Points until the remaining total is reached.
   - Record how many weeks it took.

3. **Derive results** from the distribution of simulation outcomes:
   - Sort the "weeks taken" values across all simulations.
   - P50 / P85 / P95 completion dates = `startDate + percentile(weeks) * 7 days`.
   - Probability of completion = fraction of simulations that finished within the target date.

### Why Monte Carlo over alternatives

| Approach | Why rejected |
|---|---|
| Parametric model (normal distribution) | Team velocity is rarely symmetric or normal — skewed by holidays, incidents, etc. |
| Linear regression | Gives a point estimate only; does not quantify uncertainty. |
| Bayesian model | Requires a prior on the velocity distribution shape — exactly the assumption we want to avoid. Overkill for this problem size. |
| Fixed-window "how many" | Sample size collapses for long time ranges; requires a separate run per percentile. |

---

## Design Decisions

### Unit of time: week

Throughput is measured in **weeks** (7-day windows). This is a configurable parameter (`TimeUnit`), defaulting to `Week`. Days are a supported extension point for future use.

Weeks are anchored to the **earliest completion date in the history** and counted forward — not aligned to Monday/ISO week boundaries. This avoids arbitrary edge effects at week boundaries.

### Why `remainingStoryPoints` is an integer, not a list of features

The engine only needs the total amount of work remaining, not the individual features. Since the simulation models team throughput (Story Points per week), the number or structure of individual features is irrelevant — 3 features of 5 points each is identical to 15 features of 1 point each.

This also makes the interface more flexible: callers can compute the sum from any backlog system and pass it in directly.

### Underlying assumption: Story Points are a consistent unit

The algorithm assumes that Story Points are roughly homogeneous over time — that a point today represents roughly the same effort as a point six months ago. If a team's estimation scale has drifted significantly, results will be less reliable. This is noted under Limitations.

### Minimum history requirement

At least **4 weeks** of historical data are required (`MinimumHistoryWeeks = 4`). Below this threshold there is not enough diversity in the throughput samples for the bootstrap resampling to produce meaningful variance estimates.

### Zero-throughput weeks are included

Weeks in which no features were completed are recorded as throughput = 0 and included in the resampling pool. Excluding them would bias the distribution upward (making the forecast optimistic), because slow or blocked weeks are a real part of how the team operates.

### Safety cap on simulation iterations

Each individual simulation run has an internal iteration cap to prevent a theoretical infinite loop when the throughput history is heavily zero-weighted. If a single run hits the cap without accumulating enough Story Points, that run is recorded as "did not finish" and contributes 0 to the probability calculation.

---

## Assumptions

- Story Points are estimated consistently over time.
- Historical features are representative of future work (same team, same process).
- The team's throughput distribution is stationary (no systematic upward or downward trend).
- `startDate` is treated as day 0; partial weeks are not modelled.

---

## Limitations

- Does not account for team size changes, process changes, or known upcoming holidays.
- Story Point estimation drift over time will degrade forecast accuracy.
- Very sparse histories (even if ≥ 4 weeks) with many zeros will produce wide, uncertain forecast ranges — which is honest, but may feel unhelpful.
- Percentile dates are snapped to week boundaries (multiples of 7 days from `startDate`).

---

## Future Improvements

- Support `TimeUnit.Day` for day-level granularity.
- Accept a velocity trend parameter to model improving/degrading teams.
- Expose the full simulation output (histogram data) for richer visualisation.
- Parallelise the simulation loop for large iteration counts.
