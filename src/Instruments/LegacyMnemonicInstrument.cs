using System.Globalization;
using AteFramework.Lite.Dut;

namespace AteFramework.Lite.Instruments;

/// <summary>
/// Simulated instrument that speaks a legacy short-mnemonic dialect
/// (HP-8590-class style): e.g. "cf &lt;Hz&gt;" to set frequency and "mg?"/"mn?"/
/// "mp?"/"ms?" to query gain / noise figure / input P-1 / saturated power.
/// </summary>
public sealed class LegacyMnemonicInstrument : SimulatedInstrument
{
    public LegacyMnemonicInstrument(SyntheticLna dut) : base(dut)
    {
        DeviceType = DeviceType.SpectrumAnalyzer;
    }

    public override string Name => "SimAnalyzer(Legacy)";

    public override void SetFrequencyGhz(double ghz)
        => WriteString("cf " + ((long)(ghz * 1e9)).ToString(CultureInfo.InvariantCulture));

    public override double Measure(string quantity)
    {
        string q = quantity.ToLowerInvariant() switch
        {
            "gain" => "mg?",
            "nf"   => "mn?",
            "p1"   => "mp?",
            "psat" => "ms?",
            _ => throw new ArgumentException($"Unknown quantity '{quantity}'.")
        };
        WriteString(q);
        return ParseInvariant(ReadString());
    }

    protected override string? Process(string cmd)
    {
        // set center frequency: "cf <Hz>"
        if (cmd.StartsWith("cf ", StringComparison.OrdinalIgnoreCase))
        {
            FreqGhz = ParseInvariant(cmd[3..].Trim()) / 1e9;
            return null;
        }
        return cmd.ToLowerInvariant() switch
        {
            "mg?" => Dut.GainDb(FreqGhz).ToString(CultureInfo.InvariantCulture),
            "mn?" => Dut.NoiseFigureDb(FreqGhz).ToString(CultureInfo.InvariantCulture),
            "mp?" => Dut.InputP1dBm(FreqGhz).ToString(CultureInfo.InvariantCulture),
            "ms?" => Dut.SatOutputDbm(FreqGhz).ToString(CultureInfo.InvariantCulture),
            "ip"  => null, // reset, no-op
            _ => throw new NotSupportedException($"Legacy dialect: unknown command '{cmd}'.")
        };
    }
}
