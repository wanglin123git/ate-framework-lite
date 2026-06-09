using AteFramework.Lite.Dut;
using AteFramework.Lite.Engine;
using AteFramework.Lite.Instruments;

namespace AteFramework.Lite;

/// <summary>
/// Demo entry point. Loads a test plan from a CSV profile and runs the SAME
/// plan against two instruments that speak different command dialects
/// (legacy mnemonic vs SCPI), showing identical results -- i.e. the test logic
/// is fully decoupled from the instrument dialect. Writes a CSV report per run.
/// </summary>
public static class Program
{
    public static int Main(string[] args)
    {
        bool bench = args.Contains("--bench");
        string profile = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "profiles/lna_plan.csv";
        if (!File.Exists(profile))
        {
            Console.Error.WriteLine($"Profile not found: {profile}");
            return 1;
        }

        var plan = CsvProfile.Load(profile);

        if (bench)
        {
            Benchmark.Print(Benchmark.Run(plan));
            return 0;
        }

        var engine = new TestEngine();

        // One shared synthetic DUT (noiseless for reproducible demo output).
        var dut = new SyntheticLna(noiseSigmaDb: 0.0);

        var instruments = new ICapabilityInstrument[]
        {
            new LegacyMnemonicInstrument(dut),
            new ScpiInstrument(dut),
        };

        Console.WriteLine($"Loaded {plan.Count} test steps from {profile}\n");

        var perInstrument = new List<IReadOnlyList<TestResult>>();
        foreach (var inst in instruments)
        {
            inst.Open("SIM::0::INSTR");
            var results = engine.Run(inst, plan);
            perInstrument.Add(results);

            string report = $"report-{Sanitize(inst.Name)}.csv";
            CsvProfile.WriteReport(report, inst.Name, results);

            Console.WriteLine($"== {inst.Name} ==");
            foreach (var r in results)
                Console.WriteLine($"  {r.Item.TestName,-12} {r.Value,8:0.00} {r.Item.Unit,-4} -> {(r.Pass ? "PASS" : "FAIL")}");
            Console.WriteLine($"  report written: {report}\n");

            inst.Close();
        }

        // Cross-dialect check: results must match across the two dialects.
        bool identical = CrossDialectIdentical(perInstrument[0], perInstrument[1]);
        Console.WriteLine(identical
            ? "Cross-dialect check: PASS (identical results across legacy and SCPI)."
            : "Cross-dialect check: FAIL (results differ).");

        return identical ? 0 : 2;
    }

    private static bool CrossDialectIdentical(
        IReadOnlyList<TestResult> a, IReadOnlyList<TestResult> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
            if (Math.Abs(a[i].Value - b[i].Value) > 1e-9) return false;
        return true;
    }

    private static string Sanitize(string s)
        => s.Replace("(", "").Replace(")", "").Replace(" ", "");
}
