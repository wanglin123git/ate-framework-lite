namespace AteFramework.Lite;

/// <summary>Instrument category.</summary>
public enum DeviceType
{
    SignalAnalyzer = 1,
    SpectrumAnalyzer,
    PowerAnalyzer,
    WaveAnalyzer,
    NoiseAnalyzer
}

/// <summary>Connection/transport settings, independent of vendor.</summary>
public sealed class DeviceSetting
{
    public string CommunicateMode { get; set; } = "TCPIP";
    public string ConnectStr { get; set; } = "";
    public string Ip { get; set; } = "";
    public int Port { get; set; }
    public int GpibPort { get; set; }
}

/// <summary>
/// The single low-level driver contract every instrument implements.
/// Higher layers depend only on this, never on a concrete vendor/dialect.
/// </summary>
public interface IDeviceControl
{
    bool IsConnected { get; }
    string Name { get; }
    DeviceType DeviceType { get; }
    DeviceSetting DeviceSetting { get; set; }

    bool Open(string connectStr);
    void WriteString(string cmd);
    string ReadString();
    void Close();
}

/// <summary>
/// Capability-oriented contract layered above IDeviceControl. Test logic is
/// written against this, so the same test plan runs unchanged regardless of
/// which command dialect the underlying driver speaks.
/// </summary>
public interface ICapabilityInstrument : IDeviceControl
{
    /// <summary>Tune the instrument to a frequency (GHz).</summary>
    void SetFrequencyGhz(double ghz);

    /// <summary>
    /// Measure a named RF quantity of the DUT.
    /// quantity in: "gain" | "nf" | "p1" | "psat".
    /// </summary>
    double Measure(string quantity);
}
