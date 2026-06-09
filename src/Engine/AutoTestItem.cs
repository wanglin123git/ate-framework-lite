namespace AteFramework.Lite.Engine;

/// <summary>
/// One declarative test step. A complete test plan is a list of these, loaded
/// from a CSV profile. <see cref="Quantity"/> doubles as the dispatch key
/// (the paper's "TestFuncEntry") into the engine's delegate table.
/// </summary>
public sealed record AutoTestItem(
    string Group,
    string TestName,
    string Quantity,
    double FreqGhz,
    double Min,
    double Max,
    string Unit);

/// <summary>Result of executing one test step, including the pass/fail verdict.</summary>
public sealed record TestResult(AutoTestItem Item, double Value, bool Pass);
