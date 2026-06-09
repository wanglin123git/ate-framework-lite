using System.Globalization;

namespace AteFramework.Lite.Engine;

/// <summary>Loads/saves a test plan as a CSV profile (the configuration file).</summary>
public static class CsvProfile
{
    private const string Header = "Group,TestName,Quantity,FreqGhz,Min,Max,Unit";

    public static List<AutoTestItem> Load(string path)
    {
        var items = new List<AutoTestItem>();
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            if (line.StartsWith("Group,", StringComparison.OrdinalIgnoreCase)) continue; // header

            var c = line.Split(',');
            if (c.Length < 7) continue;
            items.Add(new AutoTestItem(
                Group: c[0].Trim(),
                TestName: c[1].Trim(),
                Quantity: c[2].Trim(),
                FreqGhz: double.Parse(c[3], CultureInfo.InvariantCulture),
                Min: double.Parse(c[4], CultureInfo.InvariantCulture),
                Max: double.Parse(c[5], CultureInfo.InvariantCulture),
                Unit: c[6].Trim()));
        }
        return items;
    }

    public static void WriteReport(string path, string instrument, IReadOnlyList<TestResult> results)
    {
        using var w = new StreamWriter(path);
        w.WriteLine("# Instrument: " + instrument);
        w.WriteLine("Group,TestName,Quantity,FreqGhz,Value,Unit,Min,Max,Verdict");
        foreach (var r in results)
        {
            var i = r.Item;
            w.WriteLine(string.Join(',',
                i.Group, i.TestName, i.Quantity,
                i.FreqGhz.ToString(CultureInfo.InvariantCulture),
                r.Value.ToString(CultureInfo.InvariantCulture),
                i.Unit,
                i.Min.ToString(CultureInfo.InvariantCulture),
                i.Max.ToString(CultureInfo.InvariantCulture),
                r.Pass ? "PASS" : "FAIL"));
        }
    }
}
