using System.Globalization;
using AteFramework.Lite.Dut;

namespace AteFramework.Lite.Instruments;

/// <summary>
/// Base class for a simulated instrument. It owns the synthetic DUT and a tiny
/// command interpreter; subclasses define the concrete command dialect. The
/// WriteString/ReadString pair is the only transport, exactly as in
/// <see cref="IDeviceControl"/>; the capability methods build dialect strings
/// and go through that same transport, so the abstraction is genuine.
/// </summary>
public abstract class SimulatedInstrument : ICapabilityInstrument
{
    protected readonly SyntheticLna Dut;
    protected double FreqGhz;
    private string _lastResponse = "";

    protected SimulatedInstrument(SyntheticLna dut) => Dut = dut;

    public bool IsConnected { get; private set; }
    public abstract string Name { get; }
    public DeviceType DeviceType { get; protected init; } = DeviceType.SpectrumAnalyzer;
    public DeviceSetting DeviceSetting { get; set; } = new();

    public bool Open(string connectStr)
    {
        DeviceSetting.ConnectStr = connectStr;
        IsConnected = true;
        return true;
    }

    public void Close() => IsConnected = false;

    public void WriteString(string cmd)
    {
        if (!IsConnected) throw new InvalidOperationException($"{Name} not connected.");
        string? resp = Process(cmd.Trim());
        if (resp is not null) _lastResponse = resp;
    }

    public string ReadString() => _lastResponse;

    /// <summary>
    /// Interpret one dialect command. Returns a response string for queries,
    /// or null for commands that only change state.
    /// </summary>
    protected abstract string? Process(string cmd);

    // ---- capability layer (same for all dialects) ----
    public abstract void SetFrequencyGhz(double ghz);
    public abstract double Measure(string quantity);

    protected static double ParseInvariant(string s) =>
        double.Parse(s, CultureInfo.InvariantCulture);
}
