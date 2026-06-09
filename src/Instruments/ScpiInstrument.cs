using System.Globalization;
using AteFramework.Lite.Dut;

namespace AteFramework.Lite.Instruments;

/// <summary>
/// Simulated instrument that speaks modern SCPI for the same operations:
/// "FREQ:CENT &lt;v&gt; GHZ" to set frequency and
/// "MEAS:GAIN?"/"MEAS:NFIG?"/"MEAS:P1DB?"/"MEAS:PSAT?" to query.
/// </summary>
public sealed class ScpiInstrument : SimulatedInstrument
{
    public ScpiInstrument(SyntheticLna dut) : base(dut)
    {
        DeviceType = DeviceType.SpectrumAnalyzer;
    }

    public override string Name => "SimAnalyzer(SCPI)";

    public override void SetFrequencyGhz(double ghz)
        => WriteString("FREQ:CENT " + ghz.ToString(CultureInfo.InvariantCulture) + " GHZ");

    public override double Measure(string quantity)
    {
        string q = quantity.ToLowerInvariant() switch
        {
            "gain" => "MEAS:GAIN?",
            "nf"   => "MEAS:NFIG?",
            "p1"   => "MEAS:P1DB?",
            "psat" => "MEAS:PSAT?",
            _ => throw new ArgumentException($"Unknown quantity '{quantity}'.")
        };
        WriteString(q);
        return ParseInvariant(ReadString());
    }

    protected override string? Process(string cmd)
    {
        string upper = cmd.ToUpperInvariant();

        // "FREQ:CENT <v> GHZ" (also accept Hz if no unit)
        if (upper.StartsWith("FREQ:CENT"))
        {
            string rest = cmd[(cmd.IndexOf("CENT", StringComparison.OrdinalIgnoreCase) + 4)..].Trim();
            string[] parts = rest.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            double val = ParseInvariant(parts[0]);
            string unit = parts.Length > 1 ? parts[1].ToUpperInvariant() : "HZ";
            FreqGhz = unit switch { "GHZ" => val, "MHZ" => val / 1e3, _ => val / 1e9 };
            return null;
        }

        return upper switch
        {
            "MEAS:GAIN?" => Dut.GainDb(FreqGhz).ToString(CultureInfo.InvariantCulture),
            "MEAS:NFIG?" => Dut.NoiseFigureDb(FreqGhz).ToString(CultureInfo.InvariantCulture),
            "MEAS:P1DB?" => Dut.InputP1dBm(FreqGhz).ToString(CultureInfo.InvariantCulture),
            "MEAS:PSAT?" => Dut.SatOutputDbm(FreqGhz).ToString(CultureInfo.InvariantCulture),
            "*RST" => null,
            "SYST:PRES" => null,
            _ => throw new NotSupportedException($"SCPI dialect: unknown command '{cmd}'.")
        };
    }
}
