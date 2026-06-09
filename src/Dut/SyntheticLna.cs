namespace AteFramework.Lite.Dut;

/// <summary>
/// Behavioral model of a wideband low-noise amplifier (LNA), used as a
/// synthetic Device Under Test so the whole bench is reproducible and free of
/// any proprietary measurement data.
///
/// Parameters (see paper appendix / design doc):
///   Gain(f)  = G0  - kG*(f - f0)
///   NF(f)    = NF0 + kN*f
///   P1(f)    = P10 - kP*f         (input 1-dB compression point)
///   Psat(f)  = Ps0 - kS*f         (saturated output power)
/// with additive Gaussian measurement noise N(0, sigma).
/// f is in GHz.
/// </summary>
public sealed class SyntheticLna
{
    private const double G0 = 20.5, kG = 0.16, F0 = 1.0;
    private const double NF0 = 1.5, kN = 0.12;
    private const double P10 = -2.9, kP = 0.16;
    private const double Ps0 = 20.0, kS = 0.18;

    private readonly double _sigma;
    private readonly Random _rng;

    public SyntheticLna(double noiseSigmaDb = 0.0, int seed = 12345)
    {
        _sigma = noiseSigmaDb;
        _rng = new Random(seed);
    }

    private double Noise() => _sigma <= 0 ? 0.0 : Gaussian() * _sigma;

    // Box-Muller transform for a standard normal sample.
    private double Gaussian()
    {
        double u1 = 1.0 - _rng.NextDouble();
        double u2 = 1.0 - _rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

    public double GainDb(double fGhz)  => Math.Round(G0  - kG * (fGhz - F0) + Noise(), 2);
    public double NoiseFigureDb(double fGhz) => Math.Round(NF0 + kN * fGhz + Noise(), 2);
    public double InputP1dBm(double fGhz)    => Math.Round(P10 - kP * fGhz + Noise(), 2);
    public double SatOutputDbm(double fGhz)  => Math.Round(Ps0 - kS * fGhz + Noise(), 2);

    /// <summary>Evaluate a named quantity at a frequency.</summary>
    public double Evaluate(string quantity, double fGhz) => quantity.ToLowerInvariant() switch
    {
        "gain" => GainDb(fGhz),
        "nf"   => NoiseFigureDb(fGhz),
        "p1"   => InputP1dBm(fGhz),
        "psat" => SatOutputDbm(fGhz),
        _ => throw new ArgumentException($"Unknown quantity '{quantity}'.")
    };
}
