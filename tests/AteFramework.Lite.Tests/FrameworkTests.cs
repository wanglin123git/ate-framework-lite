using AteFramework.Lite.Dut;
using AteFramework.Lite.Engine;
using AteFramework.Lite.Instruments;
using Xunit;

namespace AteFramework.Lite.Tests;

public class SyntheticLnaTests
{
    [Theory]
    [InlineData("gain", 1.0, 20.50)]
    [InlineData("gain", 2.4, 20.28)]
    [InlineData("gain", 5.8, 19.73)]
    [InlineData("nf",   2.4, 1.79)]
    [InlineData("p1",   1.0, -3.06)]
    [InlineData("psat", 1.0, 19.82)]
    public void Model_is_deterministic_without_noise(string quantity, double fGhz, double expected)
    {
        var dut = new SyntheticLna(noiseSigmaDb: 0.0);
        Assert.Equal(expected, dut.Evaluate(quantity, fGhz), precision: 2);
    }

    [Fact]
    public void Unknown_quantity_throws()
    {
        var dut = new SyntheticLna();
        Assert.Throws<ArgumentException>(() => dut.Evaluate("xyz", 1.0));
    }
}

public class CrossDialectTests
{
    [Fact]
    public void Legacy_and_scpi_yield_identical_results()
    {
        var dut = new SyntheticLna(noiseSigmaDb: 0.0);
        var plan = new[]
        {
            new AutoTestItem("RX", "Gain@1", "gain", 1.0, 19, 22, "dB"),
            new AutoTestItem("RX", "NF@2.4", "nf",   2.4, 0,  3,  "dB"),
            new AutoTestItem("RX", "P1@5.8", "p1",   5.8, -6, 0,  "dBm"),
        };
        var engine = new TestEngine();

        var legacy = new LegacyMnemonicInstrument(dut); legacy.Open("SIM");
        var scpi   = new ScpiInstrument(dut);           scpi.Open("SIM");

        var rl = engine.Run(legacy, plan);
        var rs = engine.Run(scpi, plan);

        Assert.Equal(rl.Count, rs.Count);
        for (int i = 0; i < rl.Count; i++)
            Assert.Equal(rl[i].Value, rs[i].Value, precision: 9);
    }
}

public class TestEngineTests
{
    [Fact]
    public void Limit_evaluation_sets_pass_and_fail()
    {
        var dut = new SyntheticLna(noiseSigmaDb: 0.0); // gain@1.0 = 20.50
        var inst = new ScpiInstrument(dut); inst.Open("SIM");
        var engine = new TestEngine();

        var pass = engine.Run(inst, new[] { new AutoTestItem("G", "ok",  "gain", 1.0, 19.0, 21.0, "dB") });
        var fail = engine.Run(inst, new[] { new AutoTestItem("G", "bad", "gain", 1.0, 21.0, 22.0, "dB") });

        Assert.True(pass[0].Pass);
        Assert.False(fail[0].Pass);
    }

    [Fact]
    public void Unregistered_quantity_throws()
    {
        var inst = new ScpiInstrument(new SyntheticLna()); inst.Open("SIM");
        var engine = new TestEngine();
        Assert.Throws<InvalidOperationException>(
            () => engine.Run(inst, new[] { new AutoTestItem("G", "x", "vswr", 1.0, 0, 2, "") }));
    }
}

public class CsvProfileTests
{
    [Fact]
    public void Load_parses_rows_and_skips_header_and_comments()
    {
        string path = Path.GetTempFileName();
        File.WriteAllText(path,
            "# comment line\n" +
            "Group,TestName,Quantity,FreqGhz,Min,Max,Unit\n" +
            "RX,Gain@1.0,gain,1.0,19.0,21.5,dB\n" +
            "RX,NF@2.4,nf,2.4,0.0,2.5,dB\n");

        var items = CsvProfile.Load(path);
        File.Delete(path);

        Assert.Equal(2, items.Count);
        Assert.Equal("gain", items[0].Quantity);
        Assert.Equal(2.4, items[1].FreqGhz, precision: 3);
        Assert.Equal("dB", items[1].Unit);
    }
}
