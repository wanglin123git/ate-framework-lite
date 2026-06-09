namespace AteFramework.Lite.Engine;

/// <summary>
/// Configuration-driven test sequence engine. Each step is dispatched through a
/// delegate table keyed on its Quantity ("TestFuncEntry"), rather than through
/// hard-coded branching, so new measurements are added by registering a
/// delegate and editing the configuration profile -- not by changing this code.
/// </summary>
public sealed class TestEngine
{
    /// <summary>Maps a dispatch key to the routine that performs the measurement.</summary>
    public Dictionary<string, Func<ICapabilityInstrument, AutoTestItem, double>>
        TestItemFunctionDic { get; }

    public TestEngine()
    {
        // Default registrations. Adding a new quantity = one entry here + one
        // CSV row in the plan; no other code changes.
        Func<ICapabilityInstrument, AutoTestItem, double> measure = (inst, item) =>
        {
            inst.SetFrequencyGhz(item.FreqGhz);
            return inst.Measure(item.Quantity);
        };

        TestItemFunctionDic = new(StringComparer.OrdinalIgnoreCase)
        {
            ["gain"] = measure,
            ["nf"]   = measure,
            ["p1"]   = measure,
            ["psat"] = measure,
        };
    }

    /// <summary>Run a whole plan against one instrument and evaluate limits.</summary>
    public IReadOnlyList<TestResult> Run(ICapabilityInstrument inst, IEnumerable<AutoTestItem> plan)
    {
        var results = new List<TestResult>();
        foreach (var item in plan)
        {
            if (!TestItemFunctionDic.TryGetValue(item.Quantity, out var fn))
                throw new InvalidOperationException(
                    $"No delegate registered for quantity '{item.Quantity}'.");

            double value = fn(inst, item);
            bool pass = value >= item.Min && value <= item.Max;
            results.Add(new TestResult(item, value, pass));
        }
        return results;
    }
}
