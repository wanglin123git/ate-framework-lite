using System.Diagnostics;
using AteFramework.Lite.Engine;
using AteFramework.Lite.Instruments;
using AteFramework.Lite.Dut;

namespace AteFramework.Lite;

/// <summary>
/// Throughput micro-benchmark. On a pure simulation there is no human operator,
/// so instead of inventing manual wall-clock times we report objective,
/// countable quantities: the measured engine run time, and the number of
/// operator actions and transcription steps a manual workflow would require
/// versus the framework. Per measurement, a manual workflow needs three
/// operator actions (tune, read, record); the framework needs one action to
/// start the whole plan and performs zero manual transcription.
/// </summary>
public static class Benchmark
{
    public sealed record Stats(
        int Steps,
        double AutomatedMsPerRun,
        int ManualOperatorActions,
        int AutomatedOperatorActions,
        int ManualTranscriptionSteps,
        int AutomatedTranscriptionSteps);

    public static Stats Run(IReadOnlyList<AutoTestItem> plan, int repetitions = 2000)
    {
        var engine = new TestEngine();
        var dut = new SyntheticLna(noiseSigmaDb: 0.0);
        var inst = new ScpiInstrument(dut);
        inst.Open("SIM::0::INSTR");

        // Warm up (JIT) then time the steady-state cost.
        _ = engine.Run(inst, plan);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < repetitions; i++)
            _ = engine.Run(inst, plan);
        sw.Stop();
        inst.Close();

        int steps = plan.Count;
        return new Stats(
            Steps: steps,
            AutomatedMsPerRun: sw.Elapsed.TotalMilliseconds / repetitions,
            ManualOperatorActions: steps * 3,   // tune + read + record per item
            AutomatedOperatorActions: 1,        // one action starts the whole plan
            ManualTranscriptionSteps: steps,    // one hand-copied value per item
            AutomatedTranscriptionSteps: 0);
    }

    public static void Print(Stats s)
    {
        Console.WriteLine("== Throughput micro-benchmark ==");
        Console.WriteLine($"  plan steps (M)              : {s.Steps}");
        Console.WriteLine($"  automated engine run time   : {s.AutomatedMsPerRun:0.000} ms/plan");
        Console.WriteLine($"  operator actions (manual)   : {s.ManualOperatorActions}  (3 per measurement)");
        Console.WriteLine($"  operator actions (framework): {s.AutomatedOperatorActions}");
        Console.WriteLine($"  transcription steps (manual): {s.ManualTranscriptionSteps}");
        Console.WriteLine($"  transcription steps (auto)  : {s.AutomatedTranscriptionSteps}");
    }
}
